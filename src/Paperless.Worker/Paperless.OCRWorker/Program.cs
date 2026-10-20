using System.Text;
using Paperless.OCRWorker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using CancellationTokenSource stopping = new();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    stopping.Cancel();
};

RabbitMqSettings settings = new();
TimeSpan retryDelay = TimeSpan.FromSeconds(1);
TimeSpan maximumRetryDelay = TimeSpan.FromSeconds(5*60);

Console.WriteLine($"OCR worker starting. Queue: {settings.QueueName}");

while (!stopping.IsCancellationRequested)
{
    try
    {
        await ConsumeAsync(settings, stopping.Token);
    }
    catch (OperationCanceledException) when (stopping.IsCancellationRequested)
    {
        break;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"RabbitMQ error: {exception.Message}");
    }

    if (stopping.IsCancellationRequested)
    {
        break;
    }

    Console.WriteLine($"Retrying RabbitMQ connection in {retryDelay.TotalSeconds:0} second(s).");
    try
    {
        await Task.Delay(retryDelay, stopping.Token);
    }
    catch (OperationCanceledException) when (stopping.IsCancellationRequested)
    {
        break;
    }

    retryDelay = TimeSpan.FromSeconds(Math.Min(retryDelay.TotalSeconds * 2, maximumRetryDelay.TotalSeconds));
}

Console.WriteLine("OCR worker stopped.");

static async Task ConsumeAsync(RabbitMqSettings settings, CancellationToken cancellationToken)
{
    ConnectionFactory factory = new()
    {
        HostName = settings.HostName,
        Port = settings.Port,
        UserName = settings.UserName,
        Password = settings.Password,
        AutomaticRecoveryEnabled = true
    };

    await using IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
    await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

    await channel.QueueDeclarePassiveAsync(settings.QueueName, cancellationToken: cancellationToken);

    AsyncEventingBasicConsumer consumer = new(channel);
    consumer.ReceivedAsync += async (_, eventArgs) =>
    {
        string content = Encoding.UTF8.GetString(eventArgs.Body.Span);
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Received message (delivery tag: {eventArgs.DeliveryTag})");
        Console.WriteLine(content);
        Console.WriteLine(new string('-', 80));
        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
    };

    await channel.BasicConsumeAsync(
        queue: settings.QueueName,
        autoAck: false,
        consumer: consumer,
        cancellationToken: cancellationToken);

    Console.WriteLine($"Consuming queue '{settings.QueueName}'.");
    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
}