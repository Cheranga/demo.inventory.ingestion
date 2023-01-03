using FluentAssertions;
using Infrastructure.Messaging.Azure.Queues.Operations;
using Infrastructure.Messaging.Azure.Queues.Runtimes;
using Infrastructure.Messaging.Azure.Queues.Settings;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Queues;

public class QueueOperationsSchemaTests
{
    [Fact]
    public async Task PublishingWithDefaultSettingsMustPass()
    {
        var runTime = TestQueueRunTime.New(new Queue<string>());
        var operation = await QueueOperationsSchema<TestQueueRunTime>
            .Publish("666", "inventory", "inventory-changes", () => "Yo!")
            .Run(runTime);

        operation.IsSucc.Should().BeTrue();
        runTime.Env.Queue.Peek().Should().Be("Yo!");
    }

    [Fact]
    public async Task PublishingWithSpecificSettingsMustPass()
    {
        var runTime = TestQueueRunTime.New(new Queue<string>());
        var operation = await QueueOperationsSchema<TestQueueRunTime>
            .PublishUsingSettings("666", "inventory", "inventory-changes", () => "Yo!",
                MessageSettings.New(5, 10))
            .Run(runTime);

        operation.IsSucc.Should().BeTrue();
        runTime.Env.Queue.Peek().Should().Be("Yo!");
    }
}
