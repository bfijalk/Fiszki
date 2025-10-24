namespace Fiszki.Components.Pages.Generate;

public record SelectionStats(
    int Total,
    int SelectedCount,
    int AcceptedCount,
    int RejectedCount,
    int EditableCount
);
