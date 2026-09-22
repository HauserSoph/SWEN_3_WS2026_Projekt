using MapsterMapper;
using Paperless.Api.Errors;
using Paperless.Api.Mapping;
using Paperless.Business;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();

// IoC: ASP.NET Core builds the controller, business service and mapper for each request.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(MappingConfiguration.Create());
builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// TODO (integration with DAL teammate, Sprint 1): add the API -> DAL project reference
// and register the PostgreSQL DbContext and the implementation of IDocumentRepository here.
// Example once her class exists: AddScoped<IDocumentRepository, DocumentRepository>().
// The repository must persist documents and notes; deleting a document must also delete its notes.
// Until registration is added, normal Development startup fails dependency validation.
// The HTTP tests supply a mocked repository; they do not verify database persistence.
// After integration: create a document, restart the API and read it again to verify persistence.

var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/health", () => "ok");
app.MapControllers();
app.Run();

// Allows HTTP tests to host this application in memory.
public partial class Program;
