namespace OrderSystem.Api.Requests;

public sealed record CreateOrderRequest(string Customer, decimal Amount);