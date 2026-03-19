namespace OrderSystem.Domain.Events;

public record OrderCancelled(Guid OrderId, string Reason) : IDomainEvent;