using AutoMapper;
using System.Reflection;
using Paperless.Api.Errors;
using Paperless.Api.Mapping;
using Paperless.Api.Messaging;
using Paperless.Business;
using Paperless.Dal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
public class Program
{
    public static void Main(string[] args)
    {
        // Dieser Standard-Logger kann auch Fehler vor dem Aufbau der Anwendung ausgeben.
        using ILoggerFactory loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            // ILogger schreibt über den Console-Provider ins Terminal bzw. Docker-Log.
            builder.Logging.AddConsole();
            builder.Services.AddControllers();
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                var defaultResponse = options.InvalidModelStateResponseFactory;
                options.InvalidModelStateResponseFactory = context =>
                {
                    logger.LogWarning("Warning: API request rejected because the input is invalid.");
                    return defaultResponse(context);
                };
            });
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddAutoMapper(ConfigureMapping);
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
            builder.Services.AddSingleton<IDocumentUploadPublisher, RabbitMqDocumentUploadPublisher>();
            // Scoped bedeutet: innerhalb einer Anfrage dieselbe Instanz.
            // Der DbContext und das Repository haben dieselbe Lebensdauer.
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            string? connectionString = builder.Configuration.GetConnectionString("Paperless");
            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'Paperless' missing.");
            }
            DalAssembly.AddDatabase(builder.Services, connectionString);
            WebApplication app = builder.Build();
            IMapper mapper = app.Services.GetRequiredService<IMapper>();
            // Fehlende Mapping-Regeln sollen schon beim Start auffallen, nicht erst bei einer Anfrage.
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            app.UseExceptionHandler();
            app.MapGet("/health", GetHealth);
            app.MapControllers();
            logger.LogInformation("API: Starting web server.");
            app.Run();
            logger.LogInformation("API: Web server stopped.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error: API could not start or stopped unexpectedly");
            throw;
        }
    }
    private static void ConfigureMapping(IMapperConfigurationExpression configuration)
    {
        configuration.LicenseKey = Environment.GetEnvironmentVariable("AUTOMAPPER_KEY");
        // Unsere DTOs und Commands bekommen ihre Werte über öffentliche Konstruktoren.
        configuration.ShouldUseConstructor = IsPublicConstructor;
        configuration.AddProfile<MappingConfiguration>();
    }
    private static bool IsPublicConstructor(ConstructorInfo constructor)
    {
        return constructor.IsPublic;
    }
    private static string GetHealth()
    {
        return "ok";
    }
}
