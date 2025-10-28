using Fiszki.Database;
using Fiszki.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fiszki.Services.Services;

public class TestDataSeeder
{
    private readonly FiszkiDbContext _context;

    public TestDataSeeder(FiszkiDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists to avoid duplicate seeding
        if (await _context.Users.AnyAsync())
        {
            return; // Data already seeded
        }

        // Create test users
        var testUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 5,
            TotalCardsAccepted = 4,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        var adminUser = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            IsActive = true,
            TotalCardsGenerated = 10,
            TotalCardsAccepted = 8,
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            UpdatedAt = DateTime.UtcNow
        };

        var demoUser = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Email = "demo@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo123"),
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 15,
            TotalCardsAccepted = 12,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            UpdatedAt = DateTime.UtcNow
        };

        // Add a user without any flashcards for testing empty page scenarios
        var emptyUser = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Email = "empty@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("empty123"),
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 0,
            TotalCardsAccepted = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        // Add a second empty user for additional empty state testing
        var empty2User = new User
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Email = "empty2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("empty2123"),
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 0,
            TotalCardsAccepted = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow
        };

        // Add a third empty user to guarantee empty state for problematic tests
        var empty3User = new User
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Email = "empty3@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("empty3123"),
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 0,
            TotalCardsAccepted = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.AddRange(testUser, adminUser, demoUser, emptyUser, empty2User, empty3User);
        await _context.SaveChangesAsync();

        // Create test flashcards
        var flashcards = new List<Flashcard>
        {
            new Flashcard
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                UserId = testUser.Id,
                FrontContent = "Hello",
                BackContent = "Cześć",
                CreationSource = CreationSource.Ai,
                AiModel = "gpt-4",
                OriginalTextHash = "hash1",
                Tags = new List<string> { "greetings", "basic" },
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Flashcard
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                UserId = testUser.Id,
                FrontContent = "Thank you",
                BackContent = "Dziękuję",
                CreationSource = CreationSource.Ai,
                AiModel = "gpt-4",
                OriginalTextHash = "hash2",
                Tags = new List<string> { "greetings", "politeness" },
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new Flashcard
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                UserId = testUser.Id,
                FrontContent = "Good morning",
                BackContent = "Dzień dobry",
                CreationSource = CreationSource.Manual,
                Tags = new List<string> { "greetings", "time" },
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new Flashcard
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                UserId = adminUser.Id,
                FrontContent = "Computer",
                BackContent = "Komputer",
                CreationSource = CreationSource.Ai,
                AiModel = "gpt-3.5-turbo",
                OriginalTextHash = "hash3",
                Tags = new List<string> { "technology", "nouns" },
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Flashcard
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                UserId = demoUser.Id,
                FrontContent = "Book",
                BackContent = "Książka",
                CreationSource = CreationSource.Manual,
                Tags = new List<string> { "objects", "education" },
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Flashcard
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                UserId = demoUser.Id,
                FrontContent = "Water",
                BackContent = "Woda",
                CreationSource = CreationSource.Ai,
                AiModel = "gpt-4",
                OriginalTextHash = "hash4",
                Tags = new List<string> { "nature", "basic" },
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.Flashcards.AddRange(flashcards);
        await _context.SaveChangesAsync();

        // Create learning progress entries
        var learningProgressEntries = new List<LearningProgress>
        {
            new LearningProgress
            {
                Id = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"),
                FlashcardId = flashcards[0].Id, // Hello
                UserId = testUser.Id,
                EaseFactor = 2.5,
                Interval = 1,
                Repetitions = 1,
                NextReviewDate = DateTime.UtcNow.AddDays(1),
                LastReviewDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new LearningProgress
            {
                Id = Guid.Parse("22222222-bbbb-bbbb-bbbb-222222222222"),
                FlashcardId = flashcards[1].Id, // Thank you
                UserId = testUser.Id,
                EaseFactor = 2.8,
                Interval = 6,
                Repetitions = 3,
                NextReviewDate = DateTime.UtcNow.AddDays(3),
                LastReviewDate = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new LearningProgress
            {
                Id = Guid.Parse("33333333-cccc-cccc-cccc-333333333333"),
                FlashcardId = flashcards[2].Id, // Good morning
                UserId = testUser.Id,
                EaseFactor = 2.2,
                Interval = 1,
                Repetitions = 0,
                NextReviewDate = DateTime.UtcNow,
                LastReviewDate = null,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                UpdatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new LearningProgress
            {
                Id = Guid.Parse("44444444-dddd-dddd-dddd-444444444444"),
                FlashcardId = flashcards[3].Id, // Computer
                UserId = adminUser.Id,
                EaseFactor = 3.0,
                Interval = 15,
                Repetitions = 5,
                NextReviewDate = DateTime.UtcNow.AddDays(10),
                LastReviewDate = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new LearningProgress
            {
                Id = Guid.Parse("55555555-eeee-eeee-eeee-555555555555"),
                FlashcardId = flashcards[4].Id, // Book
                UserId = demoUser.Id,
                EaseFactor = 2.5,
                Interval = 1,
                Repetitions = 1,
                NextReviewDate = DateTime.UtcNow.AddDays(1),
                LastReviewDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _context.LearningProgress.AddRange(learningProgressEntries);
        await _context.SaveChangesAsync();

        Console.WriteLine("[TestDataSeeder] Test data seeded successfully:");
        Console.WriteLine($"  - Created {await _context.Users.CountAsync()} users");
        Console.WriteLine($"  - Created {await _context.Flashcards.CountAsync()} flashcards");
        Console.WriteLine($"  - Created {await _context.LearningProgress.CountAsync()} learning progress entries");
    }
}
