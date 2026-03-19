using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OrderSystem.Application.Interfaces;
using RabbitMQ.Client;

namespace OrderSystem.Infrastructure.Messaging;

public class RabbitMqPublisher(IConnection connection) : IMessagePublisher
{
    private static readonly ActivitySource ActivitySource = new("OrderSystem");
    
    public ValueTask PublishAsync(string routingKey, object message, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("RabbitMQ Publish");

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", routingKey);
        activity?.SetTag("messaging.message_payload", JsonSerializer.Serialize(message));

        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: "orders",
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return ValueTask.CompletedTask;
    }
}
