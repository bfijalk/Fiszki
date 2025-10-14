using FluentValidation;
using Fiszki.Database;
using Fiszki.Database.Entities;
using Fiszki.Services.Commands;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Mapping;
using Fiszki.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Fiszki.Services.Services;

public class FlashcardService : IFlashcardService
{
    private readonly FiszkiDbContext _db;
    private readonly IValidator<CreateFlashcardCommand> _createValidator;
    private readonly IValidator<UpdateFlashcardCommand> _updateValidator;

    public FlashcardService(FiszkiDbContext db, IValidator<CreateFlashcardCommand> createValidator, IValidator<UpdateFlashcardCommand> updateValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<FlashcardDto> CreateAsync(Guid userId, CreateFlashcardCommand command, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(command, ct);
        var entity = new Flashcard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FrontContent = command.FrontContent.Trim(),
            BackContent = command.BackContent.Trim(),
            CreationSource = CreationSource.Manual,
            Tags = command.Tags?.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task<FlashcardDto?> GetAsync(Guid userId, Guid flashcardId, CancellationToken ct = default)
    {
        var entity = await _db.Flashcards.AsNoTracking().FirstOrDefaultAsync(f => f.Id == flashcardId && f.UserId == userId, ct);
        return entity?.ToDto();
    }

    public async Task<IReadOnlyCollection<FlashcardDto>> ListAsync(Guid userId, int skip = 0, int take = 100, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 500);
        skip = Math.Max(skip, 0);
        var query = _db.Flashcards.AsNoTracking().Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt).Skip(skip).Take(take);
        var list = await query.ToListAsync(ct);
        return list.Select(f => f.ToDto()).ToList();
    }

    public async Task<FlashcardDto> UpdateAsync(Guid userId, UpdateFlashcardCommand command, CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(command, ct);
        var entity = await _db.Flashcards.FirstOrDefaultAsync(f => f.Id == command.FlashcardId && f.UserId == userId, ct);
        if (entity == null)
        {
            throw new DomainNotFoundException("Flashcard not found");
        }
        if (command.ExpectedUpdatedAtUtc.HasValue && entity.UpdatedAt.ToUniversalTime() != command.ExpectedUpdatedAtUtc.Value.ToUniversalTime())
        {
            throw new ConflictException("Flashcard has been modified by another operation");
        }
        entity.FrontContent = command.FrontContent.Trim();
        entity.BackContent = command.BackContent.Trim();
        entity.Tags = command.Tags?.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid flashcardId, CancellationToken ct = default)
    {
        var entity = await _db.Flashcards.FirstOrDefaultAsync(f => f.Id == flashcardId && f.UserId == userId, ct);
        if (entity == null)
        {
            return; // idempotent
        }
        _db.Flashcards.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}

