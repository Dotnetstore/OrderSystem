namespace OrderSystem.Worker.Processor.Messages;

public sealed record OrderCreatedMessage(Guid Id, string Customer, decimal Amount, DateTime CreatedAt);