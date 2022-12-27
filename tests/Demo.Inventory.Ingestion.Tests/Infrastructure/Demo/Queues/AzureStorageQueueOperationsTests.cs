using Azure.Storage.Queues;
using BunsenBurner.FunctionApp;
using FluentAssertions;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Operations;
using Infrastructure.Messaging.Azure.Queues.Settings;
using Microsoft.Extensions.Azure;
using Moq;
using static BunsenBurner.Aaa;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.Queues;

public class AzureStorageQueueOperationsTests
{
    [Fact]
    public async Task UnregisteredQueueServiceClientMustFail()
    {
        await Arrange(() =>
            {
                var factory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Throws(new Exception("queue service client error"));

                var operations = new AzureStorageQueueOperations(factory.Object);
                return (factory, operations);
            })

            .Act(async tuple => await tuple.operations.Publish(new MessageOperation(
                "666",
                "inventory",
                "inventory-changes",
                MessageSettings.DefaultSettings,
                () => "Yo!"
            )).Run())
            .Assert((tuple, fin) =>
            {
                tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                fin.IsFail.Should().BeTrue();
                fin.IfFail(error =>
                {
                    error.Code.Should().Be(ErrorCodes.QueueServiceClientNotFound);
                    error.Message.Should().Be(ErrorMessages.QueueServiceClientNotFound);
                });
                
            });
    }
    
    [Fact]
    public async Task QueueClientDoesNotExistMustFail()
    {
        await Arrange(() =>
            {
                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient.Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Throws(new Exception("some error"));
                
                var factory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                factory.Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);
                

                var operations = new AzureStorageQueueOperations(factory.Object);
                return (factory, operations);
            })
            .Act(async tuple => await tuple.operations.Publish(new MessageOperation(
                "666",
                "inventory",
                "inventory-changes",
                MessageSettings.DefaultSettings,
                () => "Yo!"
            )).Run())
            .Assert((tuple, fin) =>
            {
                tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                fin.IsFail.Should().BeTrue();
                fin.IfFail(error =>
                {
                    error.Code.Should().Be(ErrorCodes.QueueClientNotFound);
                    error.Message.Should().Be(ErrorMessages.QueueClientNotFound);
                });
                
            });
    }
}
