using System.Collections.Concurrent;
using OrderSystem.Application.Interfaces;

namespace OrderSystem.Tests;

public sealed class PublishedMessageCollector : IMessagePublisher
{
    private readonly ConcurrentQueue<PublishedMessage> _messages = new();

    public IReadOnlyCollection<PublishedMessage> Messages => _messages.ToArray();

    public ValueTask PublishAsync(string routingKey, object message, CancellationToken cancellationToken = default)
    {
        _messages.Enqueue(new PublishedMessage(routingKey, message));
        return ValueTask.CompletedTask;
    }

    public void Clear()
    {
        while (_messages.TryDequeue(out _))
        {
        }
    }
}

public sealed record PublishedMessage(string RoutingKey, object Payload);
