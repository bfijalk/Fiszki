using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Fiszki.Database.Entities;
namespace Fiszki.Database.DesignTime;

public class FiszkiDbContextFactory : IDesignTimeDbContextFactory<FiszkiDbContext>
{
    public FiszkiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FiszkiDbContext>();
        // Fallback connection string for design-time (override via env var if needed)
        var cs = Environment.GetEnvironmentVariable("FISZKI_MIGRATION_CS")
                 ?? "Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(cs);
        return new FiszkiDbContext(optionsBuilder.Options);
    }
}

public enum UserRole
{
    Basic = 0,
    Admin = 1
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Basic;
    public bool IsActive { get; set; } = true;
    public int TotalCardsGenerated { get; set; }
    public int TotalCardsAccepted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
    public ICollection<LearningProgress> LearningProgressEntries { get; set; } = new List<LearningProgress>();
}
