using OrderSystem.Domain.Events;
using OrderSystem.Domain.Exceptions;

namespace OrderSystem.Domain.Entities;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<AuditEntry> _auditLog = [];


    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Customer { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public OrderStatus Status { get; private set; } = OrderStatus.Created;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<AuditEntry> AuditLog => _auditLog.AsReadOnly();

    private Order() { }

    public Order(string customer, decimal amount)
    {
        Customer = customer;
        Amount = amount;

        AddAudit($"Order created for {customer} with amount {amount}");
        AddEvent(new OrderCreated(Id, customer, amount));
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOrderStateException("Order must be in Created state to start processing.");

        Status = OrderStatus.Processing;

        AddAudit("Order processing started");
        AddEvent(new OrderProcessingStarted(Id));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOrderStateException("Order must be Processing to complete.");

        Status = OrderStatus.Completed;

        AddAudit("Order completed");
        AddEvent(new OrderCompleted(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOrderStateException("Completed orders cannot be cancelled.");

        Status = OrderStatus.Cancelled;

        AddAudit($"Order cancelled: {reason}");
        AddEvent(new OrderCancelled(Id, reason));
    }

    private void AddEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    private void AddAudit(string message)
        => _auditLog.Add(new AuditEntry(message));

    public void ClearDomainEvents() => _domainEvents.Clear();
}