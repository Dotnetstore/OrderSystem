namespace OrderSystem.Domain.Events;

public sealed record OrderProcessingStarted(Guid OrderId) : IDomainEvent;