namespace OrderSystem.Domain.Events;

public record OrderCompleted(Guid OrderId) : IDomainEvent;