using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OrderSystem.Worker.Notifier.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace OrderSystem.Worker.Notifier;

public sealed class OrderNotifierWorker : BackgroundService
{
    private readonly IConnection _connection;
    
    private static readonly ActivitySource ActivitySource = new("OrderSystem");

    public OrderNotifierWorker(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = configuration.GetValue<int?>("RabbitMq:Port") ?? AmqpTcpEndpoint.UseDefaultPort
        };

        _connection = factory.CreateConnection();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();

        channel.ExchangeDeclare("orders", ExchangeType.Topic, durable: true);
        channel.QueueDeclare("orders.notifier", durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind("orders.notifier", "orders", "orders.completed");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) =>
        {
            using var activity = ActivitySource.StartActivity();

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.routing_key", ea.RoutingKey);

            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var msg = JsonSerializer.Deserialize<OrderCompletedMessage>(json);

            Log.Information("[Processor] Received event {RoutingKey}", ea.RoutingKey);
            Console.WriteLine($"[Notifier] Sending notification: Order {msg?.OrderId} completed");
        };

        channel.BasicConsume("orders.notifier", autoAck: true, consumer);

        return Task.CompletedTask;
    }
}