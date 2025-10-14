namespace Fiszki.Database.Entities;

public class LearningProgress
{
    public Guid Id { get; set; }
    public Guid FlashcardId { get; set; }
    public Guid UserId { get; set; }
    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; }
    public int Repetitions { get; set; }
    public DateTime NextReviewDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Flashcard? Flashcard { get; set; }
    public User? User { get; set; }
}
