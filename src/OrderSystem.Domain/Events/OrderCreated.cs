namespace OrderSystem.Domain.Events;

public sealed record OrderCreated(Guid OrderId, string Customer, decimal Amount) : IDomainEvent;