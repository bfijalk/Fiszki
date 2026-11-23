using Fiszki.Database;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Fiszki.Services.Services;

public class LeaderboardService : ILeaderboardService
{
    public const int PageSize = 30;

    private readonly FiszkiDbContext _db;

    public LeaderboardService(FiszkiDbContext db)
    {
        _db = db;
    }

    public async Task<LeaderboardPageDto> GetAsync(Guid currentUserId, int page = 1, CancellationToken ct = default)
    {
        // One small aggregate row per active user; points are the live count of the user's flashcards.
        var aggregates = await _db.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => new Entry(
                u.Id,
                u.Email,
                u.CreatedAt,
                u.Flashcards.Count(),
                u.Flashcards.Max(f => (DateTime?)f.CreatedAt)))
            .ToListAsync(ct);

        // Total order: more points first; on equal points the user who reached the score first
        // (older newest flashcard) first; then stable fallbacks so positions are always unique.
        var ranking = aggregates
            .OrderByDescending(e => e.Points)
            .ThenBy(e => e.LastCardAt ?? DateTime.MaxValue)
            .ThenBy(e => e.UserCreatedAt)
            .ThenBy(e => e.UserId)
            .ToList();

        var currentIndex = ranking.FindIndex(e => e.UserId == currentUserId);
        if (currentIndex < 0)
        {
            throw new DomainNotFoundException("User not found in the leaderboard.");
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling(ranking.Count / (double)PageSize));
        var currentPage = Math.Clamp(page, 1, totalPages);

        var rows = ranking
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .Select((e, i) => new LeaderboardRowDto(
                (currentPage - 1) * PageSize + i + 1,
                EmailMasker.Mask(e.Email),
                e.Points,
                e.UserId == currentUserId))
            .ToList();

        var me = ranking[currentIndex];
        var isLeader = currentIndex == 0;
        var rival = isLeader ? null : ranking[currentIndex - 1];
        var banner = new LeaderboardBannerDto(
            currentIndex + 1,
            me.Points,
            isLeader,
            rival is null ? null : EmailMasker.Mask(rival.Email),
            rival is null ? null : rival.Points - me.Points,
            ranking.Count(e => e.Points == me.Points) - 1);

        return new LeaderboardPageDto(rows, currentPage, PageSize, ranking.Count, totalPages, banner);
    }

    private sealed record Entry(Guid UserId, string Email, DateTime UserCreatedAt, int Points, DateTime? LastCardAt);
}
