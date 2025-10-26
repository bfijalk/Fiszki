using Microsoft.EntityFrameworkCore;
using Fiszki.Database;
using Fiszki.Database.Entities;

namespace Fiszki.FunctionalTests.Services;

public class DatabaseCleanupService
{
    private readonly string _connectionString;
    private const string TestUserEmail = "test@test.pl";

    public DatabaseCleanupService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Removes all flashcards for the test user while keeping the user account intact.
    /// This ensures a clean state for each test run.
    /// </summary>
    public async Task ClearFlashcardsForTestUserAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            // Find the test user
            var testUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == TestUserEmail);

            if (testUser == null)
            {
                Console.WriteLine("[Database Cleanup] Test user '" + TestUserEmail + "' not found. Skipping flashcard cleanup.");
                return;
            }

            // Get all flashcards for the test user
            var flashcards = await context.Flashcards
                .Where(f => f.UserId == testUser.Id)
                .ToListAsync();

            if (flashcards.Count == 0)
            {
                Console.WriteLine("[Database Cleanup] No flashcards found for test user '" + TestUserEmail + "'.");
                return;
            }

            // Remove learning progress associated with these flashcards
            var flashcardIds = flashcards.Select(f => f.Id).ToList();
            var learningProgress = await context.LearningProgress
                .Where(lp => flashcardIds.Contains(lp.FlashcardId))
                .ToListAsync();

            if (learningProgress.Count > 0)
            {
                context.LearningProgress.RemoveRange(learningProgress);
                Console.WriteLine("[Database Cleanup] Removing " + learningProgress.Count + " learning progress records.");
            }

            // Remove the flashcards
            context.Flashcards.RemoveRange(flashcards);
            Console.WriteLine("[Database Cleanup] Removing " + flashcards.Count + " flashcards for test user '" + TestUserEmail + "'.");

            // Save changes
            await context.SaveChangesAsync();
            Console.WriteLine("[Database Cleanup] Successfully cleaned up " + flashcards.Count + " flashcards and " + learningProgress.Count + " learning progress records.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Database Cleanup] Error cleaning up flashcards: " + ex.Message);
            // Don't throw - we don't want cleanup failures to fail the tests
        }
    }

    /// <summary>
    /// Ensures the test user exists in the database. This is a safety method
    /// to verify the test user is available for testing.
    /// </summary>
    public async Task<bool> EnsureTestUserExistsAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            var testUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == TestUserEmail);

            if (testUser != null)
            {
                Console.WriteLine("[Database Cleanup] Test user '" + TestUserEmail + "' found with ID: " + testUser.Id);
                return true;
            }

            Console.WriteLine("[Database Cleanup] Test user '" + TestUserEmail + "' not found in database.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Database Cleanup] Error checking for test user: " + ex.Message);
            return false;
        }
    }
}
