using System.Net;
using Azure;
using Azure.Core;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FluentAssertions;
using Infrastructure.Messaging.Azure.Queues;
using Infrastructure.Messaging.Azure.Queues.Operations;
using Infrastructure.Messaging.Azure.Queues.Settings;
using Microsoft.Extensions.Azure;
using Moq;
using static BunsenBurner.Aaa;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Queues;

public class DummySendReceiptResponse<T> : Response<T>
{
    private readonly Response _response;

    private DummySendReceiptResponse(Response response) => _response = response;

    public override T Value { get; }

    public override Response GetRawResponse() => _response;

    public static DummySendReceiptResponse<T> New(Response response) => new(response);
}

public class DummyResponse : Response
{
    private DummyResponse(HttpStatusCode statusCode) => Status = (int)statusCode;

    private DummyResponse(HttpStatusCode errorStatusCode, string reason)
    {
        Status = (int)errorStatusCode;
        ReasonPhrase = reason;
    }

    public override int Status { get; }
    public override string ReasonPhrase { get; }
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; }

    public override bool IsError => !string.IsNullOrEmpty(ReasonPhrase);

    public override void Dispose() { }

    protected override bool TryGetHeader(string name, out string? value)
    {
        value = "";
        return true;
    }

    protected override bool TryGetHeaderValues(string name, out IEnumerable<string>? values)
    {
        values = Array.Empty<string>();
        return true;
    }

    protected override bool ContainsHeader(string name) => true;

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => Array.Empty<HttpHeader>();

    public static DummyResponse New(HttpStatusCode statusCode) => new(statusCode);

    public static DummyResponse New(HttpStatusCode errorStatusCode, string reason) =>
        new(errorStatusCode, reason);
}

public class AzureStorageQueueOperationsTests
{
    [Fact]
    public async Task UnregisteredQueueServiceClientMustFail() =>
        await Arrange(() =>
            {
                var factory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Throws(new Exception("queue service client error"));

                var operations = new AzureStorageQueueOperations(factory.Object);
                return (factory, operations);
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.DefaultSettings,
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    fin.IsFail.Should().BeTrue();
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.QueueServiceClientNotFound);
                        error.Message.Should().Be(ErrorMessages.QueueServiceClientNotFound);
                    });
                }
            );

    [Fact]
    public async Task QueueDoesNotExistMustFail() =>
        await Arrange(() =>
            {
                var mockedQueueClient = new Mock<QueueClient>();
                mockedQueueClient
                    .Setup(x => x.GetPropertiesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        DummySendReceiptResponse<QueueProperties>.New(
                            DummyResponse.New(HttpStatusCode.NotFound,"queue not found")
                        )
                    );
                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient
                    .Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Returns(mockedQueueClient.Object);

                var factory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);

                var operations = new AzureStorageQueueOperations(factory.Object);
                return (factory, operations);
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.DefaultSettings,
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    fin.IsFail.Should().BeTrue();
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.QueueClientNotFound);
                        error.Message.Should().Be(ErrorMessages.QueueClientNotFound);
                    });
                }
            );

    [Fact]
    public async Task ExceptionOccursWhenPublishToQueueWithDefaultMessageSettingsMustFail() =>
        await Arrange(() =>
            {
                var mockedQueueClient = new Mock<QueueClient>();
                mockedQueueClient.Setup(x => x.GetPropertiesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DummySendReceiptResponse<QueueProperties>.New(DummyResponse.New(HttpStatusCode.OK)));
                mockedQueueClient
                    .Setup(x => x.SendMessageAsync(It.IsAny<string>()))
                    .Throws(new Exception("unauthorized access"));

                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient
                    .Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Returns(mockedQueueClient.Object);

                var mockedFactory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                mockedFactory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);

                var operations = new AzureStorageQueueOperations(mockedFactory.Object);
                return (
                    factory: mockedFactory,
                    serviceClient: mockedQueueServiceClient,
                    QueueClient: mockedQueueClient,
                    operations
                );
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.DefaultSettings,
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    tuple.serviceClient.Verify(
                        x => x.GetQueueClient(It.IsAny<string>()),
                        Times.Once
                    );
                    tuple.QueueClient.Verify(
                        x => x.SendMessageAsync(It.IsAny<string>()),
                        Times.Once
                    );

                    fin.IsFail.Should().BeTrue();
                    fin.IfFail(error =>
                    {
                        error.Code
                            .Should()
                            .Be(ErrorCodes.UnableToPublishWithDefaultMessageSettings);
                        error.Message
                            .Should()
                            .Be(ErrorMessages.UnableToPublishWithDefaultMessageSettings);
                    });
                }
            );

    [Fact]
    public async Task ExceptionOccursWhenPublishToQueueWithSpecificMessageSettingsMustFail() =>
        await Arrange(() =>
            {
                var mockedQueueClient = new Mock<QueueClient>();
                mockedQueueClient.Setup(x => x.GetPropertiesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DummySendReceiptResponse<QueueProperties>.New(DummyResponse.New(HttpStatusCode.OK)));
                mockedQueueClient
                    .Setup(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .Throws(new Exception("unauthorized access"));

                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient
                    .Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Returns(mockedQueueClient.Object);

                var mockedFactory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                mockedFactory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);

                var operations = new AzureStorageQueueOperations(mockedFactory.Object);
                return (
                    factory: mockedFactory,
                    serviceClient: mockedQueueServiceClient,
                    QueueClient: mockedQueueClient,
                    operations
                );
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.New(10, 30),
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    tuple.serviceClient.Verify(
                        x => x.GetQueueClient(It.IsAny<string>()),
                        Times.Once
                    );
                    tuple.QueueClient.Verify(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            ),
                        Times.Once
                    );

                    fin.IsFail.Should().BeTrue();
                    fin.IfFail(error =>
                    {
                        error.Code
                            .Should()
                            .Be(ErrorCodes.UnableToPublishWithProvidedMessageSettings);
                        error.Message
                            .Should()
                            .Be(ErrorMessages.UnableToPublishWithProvidedMessageSettings);
                    });
                }
            );

    [Fact]
    public async Task PublishToQueueValidConditionsMustSucceed() =>
        await Arrange(() =>
            {
                var mockedQueueClient = new Mock<QueueClient>();
                mockedQueueClient.Setup(x => x.GetPropertiesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DummySendReceiptResponse<QueueProperties>.New(DummyResponse.New(HttpStatusCode.OK)));
                mockedQueueClient
                    .Setup(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .ReturnsAsync(
                        DummySendReceiptResponse<SendReceipt>.New(
                            DummyResponse.New(HttpStatusCode.Accepted)
                        )
                    );

                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient
                    .Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Returns(mockedQueueClient.Object);

                var mockedFactory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                mockedFactory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);

                var operations = new AzureStorageQueueOperations(mockedFactory.Object);
                return (
                    factory: mockedFactory,
                    serviceClient: mockedQueueServiceClient,
                    QueueClient: mockedQueueClient,
                    operations
                );
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.New(10, 30),
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    tuple.serviceClient.Verify(
                        x => x.GetQueueClient(It.IsAny<string>()),
                        Times.Once
                    );
                    tuple.QueueClient.Verify(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            ),
                        Times.Once
                    );

                    fin.IsSucc.Should().BeTrue();
                }
            );

    [Fact]
    public async Task PublishMessageToUnauthorizedQueueMustFail() =>
        await Arrange(() =>
            {
                var mockedQueueClient = new Mock<QueueClient>();
                mockedQueueClient.Setup(x => x.GetPropertiesAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DummySendReceiptResponse<QueueProperties>.New(DummyResponse.New(HttpStatusCode.OK)));
                mockedQueueClient
                    .Setup(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .ReturnsAsync(
                        DummySendReceiptResponse<SendReceipt>.New(
                            DummyResponse.New(HttpStatusCode.Unauthorized, "unauthorized")
                        )
                    );

                var mockedQueueServiceClient = new Mock<QueueServiceClient>();
                mockedQueueServiceClient
                    .Setup(x => x.GetQueueClient(It.IsAny<string>()))
                    .Returns(mockedQueueClient.Object);

                var mockedFactory = new Mock<IAzureClientFactory<QueueServiceClient>>();
                mockedFactory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(mockedQueueServiceClient.Object);

                var operations = new AzureStorageQueueOperations(mockedFactory.Object);
                return (
                    factory: mockedFactory,
                    serviceClient: mockedQueueServiceClient,
                    QueueClient: mockedQueueClient,
                    operations
                );
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Publish(
                            new MessageOperation(
                                "666",
                                "inventory",
                                "inventory-changes",
                                MessageSettings.New(10, 30),
                                () => "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    tuple.factory.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
                    tuple.serviceClient.Verify(
                        x => x.GetQueueClient(It.IsAny<string>()),
                        Times.Once
                    );
                    tuple.QueueClient.Verify(
                        x =>
                            x.SendMessageAsync(
                                It.IsAny<string>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<TimeSpan>(),
                                It.IsAny<CancellationToken>()
                            ),
                        Times.Once
                    );

                    fin.IsFail.Should().BeTrue();
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.PublishFailResponse);
                        error.Message.Should().Be(ErrorMessages.PublishFailResponse);
                    });
                }
            );
}
