namespace OrderSystem.Application.Interfaces;

public interface IMessagePublisher
{
    ValueTask PublishAsync(string routingKey, object message, CancellationToken cancellationToken = default);
}