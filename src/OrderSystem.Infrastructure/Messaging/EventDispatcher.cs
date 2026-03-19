using OrderSystem.Application.Interfaces;
using OrderSystem.Domain.Events;

namespace OrderSystem.Infrastructure.Messaging;

public class EventDispatcher(IMessagePublisher publisher) : IEventDispatcher
{
    public ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var routingKey = domainEvent switch
        {
            OrderCreated => "orders.created",
            OrderProcessingStarted => "orders.processing",
            OrderCompleted => "orders.completed",
            OrderCancelled => "orders.cancelled",
            _ => throw new InvalidOperationException($"Unknown event type: {domainEvent.GetType().Name}")
        };

        return publisher.PublishAsync(routingKey, domainEvent, cancellationToken);
    }
}