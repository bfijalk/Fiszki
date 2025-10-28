using Microsoft.EntityFrameworkCore;
using Fiszki.Database;
using Fiszki.Database.Entities;

namespace Fiszki.FunctionalTests.Services;

public class TestFlashcardSeedingService
{
    private readonly string _connectionString;
    
    public TestFlashcardSeedingService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Seeds dummy flashcard data for a test user to avoid going through the full generation process
    /// for tests that focus on UI behavior rather than flashcard generation.
    /// </summary>
    public async Task SeedDummyFlashcardsAsync(string userEmail)
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            // Find the test user
            var testUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (testUser == null)
            {
                throw new InvalidOperationException($"Test user '{userEmail}' not found. Cannot seed flashcards.");
            }

            // Check if user already has flashcards
            var existingFlashcards = await context.Flashcards
                .Where(f => f.UserId == testUser.Id)
                .CountAsync();

            if (existingFlashcards > 0)
            {
                Console.WriteLine($"[Test Flashcard Seeding] User '{userEmail}' already has {existingFlashcards} flashcards. Skipping seeding.");
                return;
            }

            var dummyFlashcards = new List<Flashcard>
            {
                // AI-generated flashcards for testing filters and interactions
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    FrontContent = "What is Heliora?",
                    BackContent = "Heliora is a mystical realm where ancient magic flows through crystalline structures.",
                    CreationSource = CreationSource.Ai,
                    AiModel = "gpt-4",
                    Tags = new List<string> { "fantasy", "magic", "realm" },
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    FrontContent = "Who rules the Shadowlands?",
                    BackContent = "The Shadowlands are ruled by the Council of Whispers, three ancient entities that communicate through dreams.",
                    CreationSource = CreationSource.Ai,
                    AiModel = "gpt-4",
                    Tags = new List<string> { "fantasy", "shadowlands", "rulers" },
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    FrontContent = "What powers the Crystal Gates?",
                    BackContent = "The Crystal Gates are powered by starlight captured in ethereal prisms during celestial alignments.",
                    CreationSource = CreationSource.Ai,
                    AiModel = "gpt-4",
                    Tags = new List<string> { "fantasy", "crystals", "magic" },
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    UpdatedAt = DateTime.UtcNow.AddHours(-12)
                },
                // Manual flashcards for testing filters
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    FrontContent = "Test manual flashcard question",
                    BackContent = "Test manual flashcard answer",
                    CreationSource = CreationSource.Manual,
                    Tags = new List<string> { "test", "manual" },
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    UpdatedAt = DateTime.UtcNow.AddHours(-6)
                },
                new Flashcard
                {
                    Id = Guid.NewGuid(),
                    UserId = testUser.Id,
                    FrontContent = "Another manual card for testing",
                    BackContent = "This is another manually created flashcard for testing purposes",
                    CreationSource = CreationSource.Manual,
                    Tags = new List<string> { "test", "manual", "ui" },
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    UpdatedAt = DateTime.UtcNow.AddHours(-3)
                }
            };

            context.Flashcards.AddRange(dummyFlashcards);
            await context.SaveChangesAsync();

            Console.WriteLine($"[Test Flashcard Seeding] Successfully seeded {dummyFlashcards.Count} dummy flashcards for user '{userEmail}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test Flashcard Seeding] Error seeding flashcards for user '{userEmail}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Ensures the user has dummy flashcards, seeding them if they don't exist
    /// </summary>
    public async Task EnsureDummyFlashcardsExistAsync(string userEmail)
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            // Find the test user
            var testUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (testUser == null)
            {
                throw new InvalidOperationException($"Test user '{userEmail}' not found. Cannot ensure flashcards exist.");
            }

            // Check if user has any flashcards
            var existingCount = await context.Flashcards
                .Where(f => f.UserId == testUser.Id)
                .CountAsync();

            if (existingCount == 0)
            {
                await SeedDummyFlashcardsAsync(userEmail);
            }
            else
            {
                Console.WriteLine($"[Test Flashcard Seeding] User '{userEmail}' already has {existingCount} flashcards.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test Flashcard Seeding] Error ensuring flashcards exist for user '{userEmail}': {ex.Message}");
            throw;
        }
    }
}
