using System.Net;
using System.Net.Http.Json;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Events;

namespace OrderSystem.Tests;

public sealed class CancelOrderTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task CancelOrder_ShouldUpdateOrderStatus_AndPublishCancelledEvent()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var orderId = await client.CreateOrderAsync("Charlie", 50.00m);

        var response = await client.PostAsJsonAsync($"/orders/{orderId}/cancel", new
        {
            Reason = "Customer request"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var order = await factory.GetOrderAsync(orderId);
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatus.Cancelled);
        order.AuditEntryCount.ShouldBe(2);

        factory.PublishedMessages.ShouldMatch(
            ExpectedPublishedMessage.For<OrderCreated>("orders.created"),
            ExpectedPublishedMessage.For<OrderCancelled>("orders.cancelled"));
    }
}
