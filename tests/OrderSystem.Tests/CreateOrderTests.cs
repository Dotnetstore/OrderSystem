using System.Net;
using System.Net.Http.Json;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Events;

namespace OrderSystem.Tests;

public sealed class CreateOrderTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task CreateOrder_ShouldReturnCreated_AndPersistOrder()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new
        {
            Customer = "Alice",
            Amount = 149.95m
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.OriginalString.ShouldStartWith("/orders/");

        var payload = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        payload.ShouldNotBeNull();
        payload.Id.ShouldNotBe(Guid.Empty);

        var order = await factory.GetOrderAsync(payload.Id);
        order.ShouldNotBeNull();
        order.Customer.ShouldBe("Alice");
        order.Amount.ShouldBe(149.95m);
        order.Status.ShouldBe(OrderStatus.Created);
        order.AuditEntryCount.ShouldBe(1);

        factory.PublishedMessages.ShouldMatch(
            ExpectedPublishedMessage.For<OrderCreated>("orders.created"));
    }

    private sealed record CreateOrderResponse(Guid Id);
}
