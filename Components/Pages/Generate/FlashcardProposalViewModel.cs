using Fiszki.Services.Models.Generation;

namespace Fiszki.Components.Pages.Generate;

public record FlashcardProposalViewModel
{
    public Guid Id { get; init; }
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? Notes { get; set; }
    public bool Selected { get; set; }
    public bool Accepted { get; set; }
    public bool Rejected { get; set; }

    public static FlashcardProposalViewModel FromDto(FlashcardProposalDto dto) => new()
    {
        Id = dto.Id,
        Front = dto.Front,
        Back = dto.Back,
        Example = dto.Example,
        Notes = dto.Notes,
        Selected = dto.IsAccepted,
        Accepted = dto.IsAccepted,
        Rejected = dto.IsRejected
    };

    public FlashcardProposalDto ToDto() => new(
        Id,
        Front,
        Back,
        Example,
        Notes,
        Accepted,
        Rejected
    );
}
