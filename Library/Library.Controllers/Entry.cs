using System.Globalization;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Library.Controllers.Validations.v1;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Controllers;

public static class Entry
{
    /// <summary>
    /// Добавление API приложения
    /// </summary>
    public static IMvcBuilder AddApi(this IMvcBuilder builder)
    {
        builder.Services.AddValidation();
        builder.Services.AddApiVersioning();
        builder.AddApplicationPart(typeof(Api.v1.BooksController).Assembly);

        return builder;
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateBookRequestValidator>();
        services.AddFluentValidationAutoValidation();

        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member?.Name;
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("ru");
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