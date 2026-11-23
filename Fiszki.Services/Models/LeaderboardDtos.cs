namespace Fiszki.Services.Models;

/// <summary>One row of the leaderboard. Carries only a masked email, never the full address.</summary>
public record LeaderboardRowDto(int Position, string MaskedEmail, int Points, bool IsCurrentUser);

/// <summary>
/// Motivation banner data for the current user. It is computed from the full ranking and does not
/// depend on the page being viewed.
/// </summary>
/// <param name="Position">Current user's unique position (1-based).</param>
/// <param name="Points">Current user's points.</param>
/// <param name="IsLeader">True when the current user is at position 1.</param>
/// <param name="RivalMaskedEmail">Masked email of the user directly above, null for the leader.</param>
/// <param name="PointsGap">Points the rival has over the current user (0 when the rival won the tie-break), null for the leader.</param>
/// <param name="SamePointsCount">Number of other users with exactly the same points.</param>
public record LeaderboardBannerDto(
    int Position,
    int Points,
    bool IsLeader,
    string? RivalMaskedEmail,
    int? PointsGap,
    int SamePointsCount);

public record LeaderboardPageDto(
    IReadOnlyList<LeaderboardRowDto> Rows,
    int Page,
    int PageSize,
    int TotalUsers,
    int TotalPages,
    LeaderboardBannerDto Banner);
