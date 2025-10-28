using Microsoft.EntityFrameworkCore;
using Fiszki.Database;
using Fiszki.Database.Entities;
using BC = BCrypt.Net.BCrypt;

namespace Fiszki.FunctionalTests.Services;

public class TestUserManagementService
{
    private readonly string _connectionString;
    
    public TestUserManagementService(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Ensures a test user exists in the database with the given email and password.
    /// Creates the user if it doesn't exist, or updates the password if it does.
    /// </summary>
    public async Task<User> EnsureTestUserExistsAsync(string email, string password = "test123")
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            // Check if user already exists
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                Console.WriteLine($"[Test User Management] User '{email}' already exists with ID: {existingUser.Id}");
                
                // Update password to ensure it's correct for tests
                existingUser.PasswordHash = BC.HashPassword(password);
                await context.SaveChangesAsync();
                
                return existingUser;
            }

            // Create new test user
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BC.HashPassword(password),
                Role = UserRole.Basic,
                IsActive = true,
                TotalCardsGenerated = 0,
                TotalCardsAccepted = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            Console.WriteLine($"[Test User Management] Created new test user '{email}' with ID: {newUser.Id}");
            return newUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test User Management] Error ensuring test user '{email}' exists: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Removes all flashcards for a specific test user while keeping the user account intact.
    /// This ensures a clean state for each test run.
    /// </summary>
    public async Task ClearFlashcardsForUserAsync(string email)
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            // Find the test user
            var testUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (testUser == null)
            {
                Console.WriteLine($"[Test User Management] User '{email}' not found. Skipping flashcard cleanup.");
                return;
            }

            Console.WriteLine($"[Test User Management] User '{email}' found with ID: {testUser.Id}");

            // Get all flashcards for the test user
            var flashcards = await context.Flashcards
                .Where(f => f.UserId == testUser.Id)
                .ToListAsync();

            if (flashcards.Count == 0)
            {
                Console.WriteLine($"[Test User Management] No flashcards found for user '{email}'.");
                return;
            }

            // Also remove associated learning progress records
            var learningProgressRecords = await context.LearningProgress
                .Where(lp => flashcards.Select(f => f.Id).Contains(lp.FlashcardId))
                .ToListAsync();

            if (learningProgressRecords.Count > 0)
            {
                context.LearningProgress.RemoveRange(learningProgressRecords);
                Console.WriteLine($"[Test User Management] Removing {learningProgressRecords.Count} learning progress records for user '{email}'.");
            }

            // Remove flashcards
            context.Flashcards.RemoveRange(flashcards);
            await context.SaveChangesAsync();

            Console.WriteLine($"[Test User Management] Successfully removed {flashcards.Count} flashcards for user '{email}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test User Management] Error clearing flashcards for user '{email}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Gets a list of all test users (emails starting with "test" and ending with "@test.pl")
    /// </summary>
    public async Task<List<string>> GetAllTestUserEmailsAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<FiszkiDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            using var context = new FiszkiDbContext(options);
            
            var testUserEmails = await context.Users
                .Where(u => u.Email.StartsWith("test") && u.Email.EndsWith("@test.pl"))
                .Select(u => u.Email)
                .ToListAsync();

            return testUserEmails;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test User Management] Error getting test user emails: {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Cleans up all flashcards for all test users
    /// </summary>
    public async Task CleanupAllTestUsersAsync()
    {
        var testUserEmails = await GetAllTestUserEmailsAsync();
        
        foreach (var email in testUserEmails)
        {
            await ClearFlashcardsForUserAsync(email);
        }
        
        Console.WriteLine($"[Test User Management] Completed cleanup for {testUserEmails.Count} test users.");
    }
}
