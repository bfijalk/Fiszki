// Auto-generated DTOs and Command Models based on database entities and planned API.
// Date: 2025-10-14
// Location: .ai/Types.cs (not currently compiled unless included in a .csproj)

using System;
using System.Collections.Generic;

namespace Fiszki.Services.DTOs
{
    /// <summary>
    /// Represents the user data transfer object.
    /// Maps to the User entity.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Mirrors UserRole enum as string for transport simplicity.
        public bool IsActive { get; set; }
        public int TotalCardsGenerated { get; set; }
        public int TotalCardsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the flashcard data transfer object.
    /// Maps to the Flashcard entity.
    /// </summary>
    public class FlashcardDto
    {
        public Guid Id { get; set; }
        public string FrontContent { get; set; } = string.Empty;
        public string BackContent { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string CreationSource { get; set; } = string.Empty; // Mirrors CreationSource enum as string.
        public string? AiModel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the learning progress data transfer object.
    /// Maps to the LearningProgress entity.
    /// </summary>
    public class LearningProgressDto
    {
        public Guid FlashcardId { get; set; }
        public double EaseFactor { get; set; }
        public int Interval { get; set; }
        public int Repetitions { get; set; }
        public DateTime NextReviewDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
    }

    /// <summary>
    /// Represents the result from submitting a review (spaced repetition algorithm output).
    /// </summary>
    public class ReviewResultDto
    {
        public Guid FlashcardId { get; set; }
        public int NewInterval { get; set; }
        public double NewEaseFactor { get; set; }
        public int NewRepetitions { get; set; }
        public DateTime NextReviewDate { get; set; }
    }

    /// <summary>
    /// Aggregated flashcard statistics.
    /// </summary>
    public class StatsFlashcardsDto
    {
        public int TotalManual { get; set; }
        public int TotalAi { get; set; }
        public int GeneratedProposalsTotal { get; set; }
        public int AcceptedAi { get; set; }
        public double AcceptanceRate { get; set; }
        public double AiUsageRatio { get; set; }
    }

    /// <summary>
    /// Aggregated spaced repetition statistics.
    /// </summary>
    public class StatsLearningDto
    {
        public int DueNow { get; set; }
        public int DueToday { get; set; }
        public int ScheduledFuture { get; set; }
        public DateTime? LastReviewedAt { get; set; }
    }
}

namespace Fiszki.Services.Commands
{
    /// <summary>
    /// Command to register a new user.
    /// </summary>
    public class RegisterUserCommand
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to authenticate a user.
    /// </summary>
    public class LoginCommand
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to create a manual flashcard.
    /// </summary>
    public class CreateFlashcardCommand
    {
        public string FrontContent { get; set; } = string.Empty;
        public string BackContent { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Command to request AI generation of flashcards.
    /// </summary>
    public class GenerateAIFlashcardsCommand
    {
        public string SourceText { get; set; } = string.Empty;
        public int MaxCards { get; set; }
        public string Model { get; set; } = string.Empty;
    }

    /// <summary>
    /// A single accepted AI-generated flashcard proposal.
    /// </summary>
    public class AcceptedFlashcardProposal
    {
        public string TempId { get; set; } = string.Empty; // Temporary client-side identifier
        public string FrontContent { get; set; } = string.Empty;
        public string BackContent { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string AiModel { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to accept AI-generated flashcards from a prior generation session.
    /// </summary>
    public class AcceptAIFlashcardsCommand
    {
        public Guid SessionId { get; set; }
        public List<AcceptedFlashcardProposal> Accepted { get; set; } = new();
    }

    /// <summary>
    /// Command to update an existing flashcard (supports PATCH semantics by allowing nulls).
    /// </summary>
    public class UpdateFlashcardCommand
    {
        public string? FrontContent { get; set; }
        public string? BackContent { get; set; }
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// Command to delete a single flashcard.
    /// </summary>
    public class DeleteFlashcardCommand
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Command to batch delete flashcards.
    /// </summary>
    public class DeleteFlashcardsBatchCommand
    {
        public List<Guid> Ids { get; set; } = new();
    }

    /// <summary>
    /// Command to submit a spaced repetition review.
    /// </summary>
    public class SubmitReviewCommand
    {
        public Guid FlashcardId { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewedAt { get; set; }
    }
}

