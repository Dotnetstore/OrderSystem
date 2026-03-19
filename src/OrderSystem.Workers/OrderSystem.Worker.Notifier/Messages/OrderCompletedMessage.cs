namespace OrderSystem.Worker.Notifier.Messages;

public record OrderCompletedMessage(Guid OrderId);