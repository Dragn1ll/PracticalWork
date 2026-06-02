using Library.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

AppDomain.CurrentDomain.UnhandledException += (_, e) => 
{
    var ex = e.ExceptionObject as Exception;
    app.Logger.LogCritical(ex, "Fatal error. Terminating: {IsTerminating}", e.IsTerminating);
};

startup.Configure(app, app.Environment, app.Lifetime, app.Logger, app.Services);

await app.RunAsync();