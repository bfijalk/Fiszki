using Microsoft.EntityFrameworkCore;

namespace Fiszki.Database;

/// <summary>
/// Minimal application DbContext: only entity sets and constructor.
/// </summary>
public class FiszkiDbContext : DbContext
{
    public FiszkiDbContext(DbContextOptions<FiszkiDbContext> options) : base(options) { }
}
