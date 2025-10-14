namespace Fiszki.Database.Entities;

public enum CreationSource
{
    Ai = 0,
    Manual = 1
}

public class Flashcard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FrontContent { get; set; } = string.Empty;
    public string BackContent { get; set; } = string.Empty;
    public CreationSource CreationSource { get; set; }
    public string? AiModel { get; set; }
    public string? OriginalTextHash { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public LearningProgress? LearningProgress { get; set; }
}
