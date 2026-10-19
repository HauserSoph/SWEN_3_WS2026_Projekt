namespace Paperless.Api.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "rabbitmq";
    public string Password { get; set; } = "rabbitmq";
    public string Queue { get; set; } = "paperless.document-upload";
    public string IncomingPath { get; set; } = "/tmp/paperless/incoming";
    public string ProcessedPath { get; set; } = "/tmp/paperless/processed";
}
