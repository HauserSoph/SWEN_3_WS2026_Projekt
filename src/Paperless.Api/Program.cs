using MapsterMapper;
using Paperless.Api.Errors;
using Paperless.Api.Mapping;
using Paperless.Business;
using Paperless.Dal;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();

// IoC: ASP.NET Core builds the controller, business service and mapper for each request.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(MappingConfiguration.Create());
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// Datenbank anbinden.
var connectionString = builder.Configuration.GetConnectionString("Paperless")
    ?? throw new InvalidOperationException("Connection string 'Paperless' fehlt.");
DalAssembly.AddDatabase(builder.Services, connectionString);

var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/health", () => "ok");
app.MapControllers();
app.Run();

// Allows HTTP tests to host this application in memory.
public partial class Program;
