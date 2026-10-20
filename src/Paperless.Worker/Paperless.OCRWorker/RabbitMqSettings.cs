namespace Paperless.OCRWorker;

internal sealed class RabbitMqSettings
{
    public string HostName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    public int Port { get; } = ReadPort();
    public string UserName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "rabbitmq";
    public string Password { get; } = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "rabbitmq";
    public string QueueName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE") ?? "paperless.document-upload";

    private static int ReadPort()
    {
        string? value = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        if (value is null)
        {
            return 5672;
        }

        if (int.TryParse(value, out int port) && port is > 0 and <= 65535)
        {
            return port;
        }

        throw new InvalidOperationException("RABBITMQ_PORT must be a valid TCP port number.");
    }
}
