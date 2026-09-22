// REST-Einstieg. Fachlogik gehört nach Paperless.Business, DB nach Paperless.Dal.

var builder = WebApplication.CreateBuilder(args);

// Services (DbContext, Queue, Speicher) werden hier registriert, sobald sie existieren.

var app = builder.Build();

// Erstmal nur ein Ping, damit klar ist dass der Server steht. CRUD kommt später.
app.MapGet("/health", () => "ok");

app.Run();
