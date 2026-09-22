using AutoMapper;
using System.Reflection;
using Paperless.Api.Errors;
using Paperless.Api.Mapping;
using Paperless.Business;
using Paperless.Dal;

// Contains the application's starting point.
public partial class Program
{
    // Sets up the API and starts the web server.
    public static void Main(string[] args)
    {
        // Creates the builder and reads the application settings.
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        // Registers support for API controllers.
        builder.Services.AddControllers();
        // Registers support for standard error responses.
        builder.Services.AddProblemDetails();
        // Registers our business error handler.
        builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
        // Shares the system clock across the application.
        builder.Services.AddSingleton(TimeProvider.System);
        // Uses the named method below to configure AutoMapper.
        builder.Services.AddAutoMapper(ConfigureMapping);
        // Creates a business service for each request.
        builder.Services.AddScoped<IDocumentService, DocumentService>();

        // Reads the existing database connection settings.
        string? connectionString = builder.Configuration.GetConnectionString("Paperless");
        // Checks whether the connection settings are missing.
        if (connectionString == null)
        {
            // Stops startup when the required setting is missing.
            throw new InvalidOperationException("Connection string 'Paperless' missing.");
        }
        // Calls the existing database setup without changing it.
        DalAssembly.AddDatabase(builder.Services, connectionString);

        // Builds the application and its dependency container.
        WebApplication app = builder.Build();
        // Gets the registered mapper from the container.
        IMapper mapper = app.Services.GetRequiredService<IMapper>();
        // Checks that all mapping rules are valid.
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        // Enables the error handler for incoming requests.
        app.UseExceptionHandler();
        // Calls GetHealth when a request reaches /health.
        app.MapGet("/health", GetHealth);
        // Connects incoming requests to controller methods.
        app.MapControllers();
        // Starts the web server.
        app.Run();
    }

    // Sets up AutoMapper using the existing mapping profile.
    private static void ConfigureMapping(IMapperConfigurationExpression configuration)
    {
        // Reads the license key from the environment.
        configuration.LicenseKey = Environment.GetEnvironmentVariable("AUTOMAPPER_KEY");
        // Uses the named method below to select public constructors.
        configuration.ShouldUseConstructor = IsPublicConstructor;
        // Adds the existing mapping rules.
        configuration.AddProfile<MappingConfiguration>();
    }

    // Tells AutoMapper whether it may use this constructor.
    private static bool IsPublicConstructor(ConstructorInfo constructor)
    {
        // Allows public constructors.
        return constructor.IsPublic;
    }

    // Confirms the API is running; this does not check the database.
    private static string GetHealth()
    {
        // Returns the health response text.
        return "ok";
    }
}
