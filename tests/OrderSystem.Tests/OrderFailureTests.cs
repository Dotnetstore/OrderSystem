using System.Net;
using System.Net.Http.Json;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Tests;

public sealed class OrderFailureTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task StartProcessing_ForUnknownOrder_ShouldFail_WithoutPublishingEvents()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/orders/{Guid.NewGuid()}/start-processing", content: null);

        await response.ShouldHaveProblemAsync(HttpStatusCode.NotFound, "Order not found", "not found");
        factory.PublishedMessages.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task Complete_ForUnknownOrder_ShouldFail_WithoutPublishingEvents()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsync($"/orders/{Guid.NewGuid()}/complete", content: null);

        await response.ShouldHaveProblemAsync(HttpStatusCode.NotFound, "Order not found", "not found");
        factory.PublishedMessages.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task Cancel_ForUnknownOrder_ShouldFail_WithoutPublishingEvents()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/orders/{Guid.NewGuid()}/cancel", new
        {
            Reason = "Customer request"
        });

        await response.ShouldHaveProblemAsync(HttpStatusCode.NotFound, "Order not found", "not found");
        factory.PublishedMessages.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task Complete_BeforeProcessing_ShouldFail_AndLeaveOrderUnchanged()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var orderId = await client.CreateOrderAsync("Dana", 89.50m);

        var response = await client.PostAsync($"/orders/{orderId}/complete", content: null);

        await response.ShouldHaveProblemAsync(HttpStatusCode.Conflict, "Invalid order state", "Processing");

        var order = await factory.GetOrderAsync(orderId);
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatus.Created);
        order.AuditEntryCount.ShouldBe(1);
        factory.PublishedMessages.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Cancel_AfterCompletion_ShouldFail_AndKeepOrderCompleted()
    {
        await factory.ResetAsync();
        using var client = factory.CreateClient();

        var orderId = await client.CreateOrderAsync("Evan", 120.00m);

        await client.PostAsync($"/orders/{orderId}/start-processing", content: null);
        await client.PostAsync($"/orders/{orderId}/complete", content: null);

        var response = await client.PostAsJsonAsync($"/orders/{orderId}/cancel", new
        {
            Reason = "Too late"
        });

        await response.ShouldHaveProblemAsync(HttpStatusCode.Conflict, "Invalid order state", "cannot be cancelled");

        var order = await factory.GetOrderAsync(orderId);
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatus.Completed);
        order.AuditEntryCount.ShouldBe(3);
        factory.PublishedMessages.Messages.Count.ShouldBe(3);
    }
}
