using Microsoft.EntityFrameworkCore;

namespace Library.Data.PostgreSql.UnitTests;

internal static class DbFactory
{
    public static AppDbContext Create(string name)
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(opts);
    }
}
