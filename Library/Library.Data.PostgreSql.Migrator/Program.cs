using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Library.Data.PostgreSql.Migrator;

[UsedImplicitly]
public class Program
{
    private const string AppName = "Library.Data.PostgreSql.Migrator";

    private static IConfiguration Configuration { get; set; } = null!;

    private static readonly ILogger SystemLogger = CreateSystemLogger();

    public static async Task Main()
    {
        try
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build();

            await MigrateDatabase();
        }
        catch (Exception exception)
        {
            SystemLogger.LogCritical(exception, "Critical error in Main");
            throw;
        }
    }

    private static async Task MigrateDatabase()
    {
        var serviceProvider = CreateServices();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        await MigrationsRunner.ApplyMigrations(logger, serviceProvider, AppName);
    }

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();

        return services
            .AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.ClearProviders();
            })
            .AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration["App:DbConnectionString"],
                sqlServerOptions => sqlServerOptions.CommandTimeout(Configuration.GetValue<int>("App:MigrationTimeoutInSeconds"))))
            .BuildServiceProvider(false);
    }

    private static ILogger CreateSystemLogger()
    {
        var logger = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
            })
            .BuildServiceProvider()
            .GetRequiredService<ILogger<Program>>();

        return logger;
    }
}