namespace Fiszki.Services.Models;

public record UserDto(
    Guid Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalCardsGenerated,
    int TotalCardsAccepted
);

