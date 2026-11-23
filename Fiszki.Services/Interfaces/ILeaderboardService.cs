using Fiszki.Services.Models;

namespace Fiszki.Services.Interfaces;

public interface ILeaderboardService
{
    /// <summary>
    /// Returns one page of the leaderboard (30 active users per page) plus the motivation banner
    /// data of the current user. The page number is clamped to the valid range.
    /// </summary>
    /// <exception cref="Exceptions.DomainNotFoundException">The current user is not an active user.</exception>
    Task<LeaderboardPageDto> GetAsync(Guid currentUserId, int page = 1, CancellationToken ct = default);
}
