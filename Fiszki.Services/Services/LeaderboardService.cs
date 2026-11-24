using Fiszki.Database;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace Fiszki.Services.Services;

public class LeaderboardService : ILeaderboardService
{
    public const int PageSize = 30;

    /// <summary>Extra points an archived (learned) flashcard gives on top of the 1 point every flashcard gives.</summary>
    public const int ArchivedBonus = 3;

    private readonly FiszkiDbContext _db;

    public LeaderboardService(FiszkiDbContext db)
    {
        _db = db;
    }

    public async Task<LeaderboardPageDto> GetAsync(Guid currentUserId, int page = 1, CancellationToken ct = default)
    {
        // One small aggregate row per active user. Points are computed live from the user's flashcards:
        // 1 per flashcard plus ArchivedBonus per archived flashcard.
        var raw = await _db.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => new RawEntry(
                u.Id,
                u.Email,
                u.CreatedAt,
                u.Flashcards.Count(),
                u.Flashcards.Count(f => f.ArchivedAt != null),
                u.Flashcards.Max(f => (DateTime?)f.CreatedAt),
                u.Flashcards.Max(f => f.ArchivedAt)))
            .ToListAsync(ct);

        // The score changes when a flashcard is created or archived, so the moment the user reached
        // the current score is the later of the newest creation and the newest archiving.
        var aggregates = raw.Select(r => new Entry(
            r.UserId,
            r.Email,
            r.UserCreatedAt,
            r.Flashcards + ArchivedBonus * r.Archived,
            r.Flashcards,
            r.Archived,
            LearningProgressCalculator.Percent(r.Archived, r.Flashcards),
            Latest(r.LastCreatedAt, r.LastArchivedAt))).ToList();

        // Total order: more points first; on equal points the user who reached the score first
        // (earlier latest score event) first; then stable fallbacks so positions are always unique.
        // The learning progress is shown but never used for ordering.
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
                e.UserId == currentUserId,
                e.Flashcards,
                e.Archived,
                e.LearningProgress))
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
            ranking.Count(e => e.Points == me.Points) - 1,
            me.Flashcards,
            me.Archived,
            me.LearningProgress);

        return new LeaderboardPageDto(rows, currentPage, PageSize, ranking.Count, totalPages, banner);
    }

    private static DateTime? Latest(DateTime? a, DateTime? b) =>
        a is null ? b : b is null ? a : (a > b ? a : b);

    private sealed record RawEntry(Guid UserId, string Email, DateTime UserCreatedAt, int Flashcards, int Archived, DateTime? LastCreatedAt, DateTime? LastArchivedAt);

    private sealed record Entry(Guid UserId, string Email, DateTime UserCreatedAt, int Points, int Flashcards, int Archived, int LearningProgress, DateTime? LastCardAt);
}
