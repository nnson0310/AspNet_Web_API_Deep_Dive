﻿using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;

namespace CourseLibrary.API;

internal static class StartupHelperExtensions
{
    // Add services to the container
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
            configure.CacheProfiles.Add("longCache", new CacheProfile
            {
                Duration = 240,
                Location = ResponseCacheLocation.Client,
            });
        }).AddNewtonsoftJson(setupAction =>
        {
            setupAction.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }).AddXmlDataContractSerializerFormatters()
        .ConfigureApiBehaviorOptions(setupAction =>
        {
            setupAction.InvalidModelStateResponseFactory = context =>
            {
                var problemsDetailFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                var validationProblemDetails = problemsDetailFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

                validationProblemDetails.Detail = "See error field for detail";
                validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                validationProblemDetails.Title = "One or more field can not pass validations";
                validationProblemDetails.Instance = context.HttpContext.Request.Path;

                return new UnprocessableEntityObjectResult(validationProblemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        builder.Services.Configure<MvcOptions>(configure =>
        {
            var newtonJsonFormatter = configure.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();
            if (newtonJsonFormatter != null)
            {
                newtonJsonFormatter.SupportedMediaTypes.Add("application/api.son.hateoas+json");
            }
        });

        builder.Services.AddTransient<IPropMappingService, PropMappingService>();

        builder.Services.AddTransient<IPropCheckerService, PropCheckerService>();

        builder.Services.AddScoped<ICourseLibraryRepository,
            CourseLibraryRepository>();

        builder.Services.AddDbContext<CourseLibraryContext>(options =>
        {
            options.UseSqlite(@"Data Source=library.db");
        });

        builder.Services.AddLogging();

        builder.Services.AddResponseCaching();

        builder.Services.AddHttpCacheHeaders(
        (expirationModelOptions) =>
        {
            expirationModelOptions.MaxAge = 30;
            expirationModelOptions.CacheLocation = CacheLocation.Private;
        },
        (validationModelOptions) =>
        {
            validationModelOptions.MustRevalidate = true;
        });

        builder.Services.AddAutoMapper(
            AppDomain.CurrentDomain.GetAssemblies());

        return builder.Build();
    }

    // Configure the request/response pipelien
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseResponseCaching();

        app.UseHttpCacheHeaders();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static async Task ResetDatabaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetService<CourseLibraryContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while migrating the database.", ex);
            }
        }
    }
}