using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reports.Cache.Redis;
using Reports.Controllers;
using Reports.Data.Minio;
using Reports.Data.PostgreSql;
using Reports.Exceptions;
using Reports.MessageBroker.RabbitMq;
using Reports.Web.Configuration;

namespace Reports.Web;

public class Startup
{
    private static string _basePath = null!;
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        _basePath = Configuration["GlobalPrefix"]?.Trim('/') is { Length: > 0 } prefix 
            ? $"/{prefix}" 
            : "";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddPostgreSqlStorage(cfg =>
        {
            var npgsqlDataSource = new NpgsqlDataSourceBuilder(Configuration["App:DbConnectionString"])
                .EnableDynamicJson()
                .Build();

            cfg.UseNpgsql(npgsqlDataSource);
        });

        services.AddControllers(opt =>
            {
                opt.Filters.Add<DomainExceptionFilter<AppException>>();
            })
            .AddApi()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddSwaggerGen(c =>
        {
            c.UseOneOfForPolymorphism();
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Contracts.xml"));
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Controllers.xml"));
        });

        services.AddDomain();
        services.AddCache(Configuration);
        services.AddFileStorage();
        services.AddMessageBroker(Configuration);
    }

    [UsedImplicitly]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
        ILogger logger, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(_basePath))
        {
            app.UsePathBase(new PathString(_basePath));
        }
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            
            foreach (var description in provider.ApiVersionDescriptions)
            {
                var url = $"{_basePath}/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}