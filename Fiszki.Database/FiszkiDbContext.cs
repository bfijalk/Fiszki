using Microsoft.EntityFrameworkCore;

namespace Fiszki.Database;

/// <summary>
/// Application DbContext with entity sets. Configuration is in separate classes implementing IEntityTypeConfiguration.
/// </summary>
public class FiszkiDbContext : DbContext
{
    public FiszkiDbContext(DbContextOptions<FiszkiDbContext> options) : base(options) { }

    public DbSet<Entities.User> Users => Set<Entities.User>();
    public DbSet<Entities.Flashcard> Flashcards => Set<Entities.Flashcard>();
    public DbSet<Entities.LearningProgress> LearningProgress => Set<Entities.LearningProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pgcrypto"); // enable later if needed for db-generated UUIDs
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiszkiDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
