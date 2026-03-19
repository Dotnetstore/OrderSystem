namespace OrderSystem.Application.Exceptions;

public sealed class OrderNotFoundException(Guid orderId)
    : Exception($"Order {orderId} not found.");

