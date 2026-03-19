using System.Net;
using System.Net.Http.Json;

namespace OrderSystem.Tests;

internal static class TestHttpClientExtensions
{
    public static async Task<Guid> CreateOrderAsync(this HttpClient client, string customer, decimal amount)
    {
        var response = await client.PostAsJsonAsync("/orders", new
        {
            Customer = customer,
            Amount = amount
        });

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        payload.ShouldNotBeNull();

        return payload.Id;
    }

    public static async Task ShouldHaveProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        string expectedTitle,
        string expectedDetailFragment)
    {
        response.StatusCode.ShouldBe(expectedStatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe(expectedTitle);
        problem.Detail.ShouldContain(expectedDetailFragment);
        problem.Status.ShouldBe((int)expectedStatusCode);
    }

    private sealed record CreateOrderResponse(Guid Id);

    private sealed record ProblemResponse(string Title, string Detail, int Status);
}

