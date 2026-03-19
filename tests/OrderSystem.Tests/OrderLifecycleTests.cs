using System.Net;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Events;

namespace OrderSystem.Tests;

public sealed class OrderLifecycleTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task StartProcessing_ThenComplete_ShouldUpdateOrderStatus_AndPublishEvents()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var orderId = await client.CreateOrderAsync("Bob", 200.00m);

        var startResponse = await client.PostAsync($"/orders/{orderId}/start-processing", content: null);
        var completeResponse = await client.PostAsync($"/orders/{orderId}/complete", content: null);

        startResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        completeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var order = await factory.GetOrderAsync(orderId);
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatus.Completed);
        order.AuditEntryCount.ShouldBe(3);

        factory.PublishedMessages.ShouldMatch(
            ExpectedPublishedMessage.For<OrderCreated>("orders.created"),
            ExpectedPublishedMessage.For<OrderProcessingStarted>("orders.processing"),
            ExpectedPublishedMessage.For<OrderCompleted>("orders.completed"));
    }
}
