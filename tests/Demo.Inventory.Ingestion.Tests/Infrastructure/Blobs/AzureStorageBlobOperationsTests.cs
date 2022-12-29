using Azure.Storage.Blobs;
using FluentAssertions;
using Infrastructure.Messaging.Azure.Blobs;
using Infrastructure.Messaging.Azure.Blobs.Operations;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using Microsoft.Extensions.Azure;
using Moq;
using static BunsenBurner.Aaa;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Blobs;

public class AzureStorageBlobOperationsTests
{
    [Fact]
    public static async Task UnregisteredBlobServiceClientMustFail()
    {
        await Arrange(() =>
            {
                var factory = new Mock<IAzureClientFactory<BlobServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Throws(new Exception("error"));

                var operations = AzureStorageBlobOperations.New(factory.Object);
                return (factory, operations);
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Upload(
                            new FileUploadRequest(
                                "666",
                                "inventory",
                                "inventory",
                                "blah.csv",
                                "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    fin.IsFail.Should().BeTrue();
                    tuple.factory.Verify(x => x.CreateClient("inventory"), Times.Once);
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.UnregisteredBlobServiceClient);
                        error.Message.Should().Be(ErrorMessages.UnregisteredBlobServiceClient);
                    });
                }
            );
    }

    [Fact]
    public static async Task UnavailableBlobContainerMustFail()
    {
        await Arrange(() =>
            {
                var blobServiceClient = new Mock<BlobServiceClient>();
                blobServiceClient
                    .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                    .Throws(new Exception("error"));
                var factory = new Mock<IAzureClientFactory<BlobServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(blobServiceClient.Object);

                var operations = AzureStorageBlobOperations.New(factory.Object);
                return (factory, blobServiceClient, operations);
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Upload(
                            new FileUploadRequest(
                                "666",
                                "inventory",
                                "inventory",
                                "blah.csv",
                                "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    fin.IsFail.Should().BeTrue();
                    tuple.factory.Verify(x => x.CreateClient("inventory"), Times.Once);
                    tuple.blobServiceClient.Verify(
                        x => x.GetBlobContainerClient("inventory"),
                        Times.Once
                    );
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.CannotGetBlobContainerClient);
                        error.Message.Should().Be(ErrorMessages.CannotGetBlobContainerClient);
                    });
                }
            );
    }
    
    [Fact]
    public static async Task CannotCreateBlobClientForFileMustFail()
    {
        await Arrange(() =>
            {
                var blobContainerClient = new Mock<BlobContainerClient>();
                blobContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
                    .Throws(new Exception("error"));
                
                var blobServiceClient = new Mock<BlobServiceClient>();
                blobServiceClient
                    .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                    .Returns(blobContainerClient.Object);
                
                var factory = new Mock<IAzureClientFactory<BlobServiceClient>>();
                factory
                    .Setup(x => x.CreateClient(It.IsAny<string>()))
                    .Returns(blobServiceClient.Object);

                var operations = AzureStorageBlobOperations.New(factory.Object);
                return (factory, blobServiceClient, blobContainerClient, operations);
            })
            .Act(
                async tuple =>
                    await tuple.operations
                        .Upload(
                            new FileUploadRequest(
                                "666",
                                "inventory",
                                "inventory",
                                "blah.csv",
                                "Yo!"
                            )
                        )
                        .Run()
            )
            .Assert(
                (tuple, fin) =>
                {
                    fin.IsFail.Should().BeTrue();
                    tuple.factory.Verify(x => x.CreateClient("inventory"), Times.Once);
                    tuple.blobServiceClient.Verify(
                        x => x.GetBlobContainerClient("inventory"),
                        Times.Once
                    );
                    tuple.blobContainerClient.Verify(x=>x.GetBlobClient("blah.csv"), Times.Once);
                    fin.IfFail(error =>
                    {
                        error.Code.Should().Be(ErrorCodes.CannotGetBlobClient);
                        error.Message.Should().Be(ErrorMessages.CannotGetBlobClient);
                    });
                }
            );
    }
}
