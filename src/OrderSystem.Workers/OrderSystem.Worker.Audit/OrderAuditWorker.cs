using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace OrderSystem.Worker.Audit;

public class OrderAuditWorker : BackgroundService
{
    private readonly IConnection _connection;
    
    private static readonly ActivitySource ActivitySource = new("OrderSystem");

    public OrderAuditWorker(IConfiguration configuration)
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
        channel.QueueDeclare("orders.audit", durable: true, exclusive: false, autoDelete: false);

        // Bind ALL events
        channel.QueueBind("orders.audit", "orders", "orders.*");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) =>
        {
            using var activity = ActivitySource.StartActivity();

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.routing_key", ea.RoutingKey);

            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            Log.Information("[Processor] Received event {RoutingKey}", ea.RoutingKey);
            Console.WriteLine($"[Audit] Event '{ea.RoutingKey}': {json}");
        };

        channel.BasicConsume("orders.audit", autoAck: true, consumer);

        return Task.CompletedTask;
    }
}