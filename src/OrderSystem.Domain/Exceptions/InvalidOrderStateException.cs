namespace OrderSystem.Domain.Exceptions;

public sealed class InvalidOrderStateException(string message) : Exception(message);

