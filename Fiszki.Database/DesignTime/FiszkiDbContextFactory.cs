using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
