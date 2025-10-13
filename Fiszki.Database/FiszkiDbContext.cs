using Microsoft.EntityFrameworkCore;

namespace Fiszki.Database;

/// <summary>
/// Minimal application DbContext (entities to be added later once schema is finalized).
/// </summary>
public class FiszkiDbContext : DbContext
{
    public FiszkiDbContext(DbContextOptions<FiszkiDbContext> options) : base(options) { }
}
