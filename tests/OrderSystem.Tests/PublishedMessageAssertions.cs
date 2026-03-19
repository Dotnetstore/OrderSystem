namespace OrderSystem.Tests;

internal static class PublishedMessageAssertions
{
    public static void ShouldMatch(this PublishedMessageCollector collector, params ExpectedPublishedMessage[] expectedMessages)
    {
        var actualMessages = collector.Messages.ToArray();

        actualMessages.Length.ShouldBe(expectedMessages.Length);

        for (var index = 0; index < expectedMessages.Length; index++)
        {
            actualMessages[index].RoutingKey.ShouldBe(expectedMessages[index].RoutingKey);
            actualMessages[index].Payload.GetType().ShouldBe(expectedMessages[index].PayloadType);
        }
    }
}

internal sealed record ExpectedPublishedMessage(string RoutingKey, Type PayloadType)
{
    public static ExpectedPublishedMessage For<TPayload>(string routingKey) => new(routingKey, typeof(TPayload));
}

