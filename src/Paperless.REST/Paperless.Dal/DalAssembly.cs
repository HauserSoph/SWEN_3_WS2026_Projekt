using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paperless.Domain;

namespace Paperless.Dal;

/// Registriert DbContext und Repository, damit die API die Datenbank benutzen kann.
public static class DalAssembly
{
    public static void AddDatabase(IServiceCollection services, string connectionString)
    {
        //weenn DB gebraucht wird, verwende Npgsql mit Connection String
        services.AddDbContext<PaperlessDbContext>(options => options.UseNpgsql(connectionString));
        // Wenn IDocumentRepo gebraucht wird, erstelle ein DocumentRepository;  jedes mal ein neues
        services.AddScoped<IDocumentRepository, DocumentRepository>();
    }
}
