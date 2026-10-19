using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paperless.Domain;

namespace Paperless.Dal;
public static class DalAssembly
{
    public static void AddDatabase(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaperlessDbContext>(options => options.UseNpgsql(connectionString));
        // Innerhalb einer Anfrage teilen sich Service und Repository denselben DbContext.
        services.AddScoped<IDocumentRepository, DocumentRepository>();
    }
}
