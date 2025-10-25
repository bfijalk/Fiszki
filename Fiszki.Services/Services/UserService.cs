using BCrypt.Net;
using FluentValidation;
using Fiszki.Database;
using Fiszki.Services.Commands;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Mapping;
using Fiszki.Services.Models;
using Microsoft.EntityFrameworkCore;
using UserEntity = Fiszki.Database.Entities.User;

namespace Fiszki.Services.Services;

public class UserService : IUserService
{
    private readonly FiszkiDbContext _db;
    private readonly IValidator<RegisterUserCommand> _registerValidator;
    private readonly IValidator<LoginCommand> _loginValidator;

    public UserService(FiszkiDbContext db, IValidator<RegisterUserCommand> registerValidator, IValidator<LoginCommand> loginValidator)
    {
        _db = db;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<UserDto> RegisterAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        var vr = await _registerValidator.ValidateAsync(new ValidationContext<RegisterUserCommand>(command), ct);
        if (!vr.IsValid)
        {
            throw new ValidationException("Validation failed", vr.Errors);
        }
        var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (existing != null)
        {
            throw new ConflictException($"User with email {command.Email} already exists");
        }
        var hash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 12);
        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            PasswordHash = hash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task<UserDto> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        var vr = await _loginValidator.ValidateAsync(new ValidationContext<LoginCommand>(command), ct);
        if (!vr.IsValid)
        {
            throw new ValidationException("Validation failed", vr.Errors);
        }
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedDomainException("Invalid credentials");
        }
        if (!user.IsActive)
        {
            throw new UnauthorizedDomainException("Account inactive");
        }
        return user.ToDto();
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync(new object[] { userId }, ct);
        if (user == null)
        {
            throw new DomainNotFoundException("User not found");
        }
        return user.ToDto();
    }
}
