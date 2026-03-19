using OrderSystem.Domain.Events;

namespace OrderSystem.Application.Interfaces;

public interface IEventDispatcher
{
    ValueTask DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}