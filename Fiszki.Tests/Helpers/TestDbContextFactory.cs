using Microsoft.EntityFrameworkCore;
using Fiszki.Database;

namespace Fiszki.Tests.Helpers;

public static class TestDbContextFactory
{
    public static FiszkiDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<FiszkiDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new FiszkiDbContext(options);
    }
}
