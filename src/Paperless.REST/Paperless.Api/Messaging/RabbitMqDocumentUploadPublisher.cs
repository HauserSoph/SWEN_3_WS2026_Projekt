using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Paperless.Api.Messaging;

public sealed class RabbitMqDocumentUploadPublisher(IOptions<RabbitMqOptions> options) : IDocumentUploadPublisher
{
    private readonly RabbitMqOptions options = options.Value;

    public Task PublishAsync(DocumentUploadJob job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionFactory factory = CreateFactory();
        using IConnection connection = factory.CreateConnection();
        using IModel channel = connection.CreateModel();
        channel.QueueDeclare(options.Queue, durable: true, exclusive: false, autoDelete: false);
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job));
        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        channel.BasicPublish(string.Empty, options.Queue, properties, body);
        return Task.CompletedTask;
    }

    internal ConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            DispatchConsumersAsync = false
        };
    }
}
