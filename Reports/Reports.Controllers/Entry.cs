using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Controllers;

public static class Entry
{
    /// <summary>
    /// Добавление API приложения
    /// </summary>
    public static IMvcBuilder AddApi(this IMvcBuilder builder)
    {
        builder.Services.AddValidation();
        builder.Services.AddApiVersioning();
        builder.AddApplicationPart(typeof(Api.v1.ReportsController).Assembly);

        return builder;
    }

    private static void AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}