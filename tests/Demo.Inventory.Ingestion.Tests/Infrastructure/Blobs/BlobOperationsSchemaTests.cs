using AutoFixture;
using Demo.Inventory.Ingestion.Functions.Features.ReadInventoryChanges;
using FluentAssertions;
using Infrastructure.Messaging.Azure.Blobs.Operations;
using Infrastructure.Messaging.Azure.Blobs.Requests;
using Infrastructure.Messaging.Azure.Blobs.Runtimes;
using Newtonsoft.Json;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Blobs;

public class BlobOperationsSchemaTests
{
    private readonly Fixture _fixture;

    public BlobOperationsSchemaTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ReadFile()
    {
        var inventories = JsonConvert.SerializeObject(_fixture.CreateMany<Domain.Inventory>());
        var runTime = TestBlobRunTime.New(new Dictionary<string, string> { { "a", inventories } });

        var operation = await BlobOperationsSchema<TestBlobRunTime>.ReadDataFromCsv<Domain.Inventory, InventoryMap>(
            new ReadFileRequest("1", "inventory", "inv", "a")
        ).Run(runTime);

        operation.IsSucc.Should().BeTrue();
        operation.IfSucc(data => data.Count.Should().Be(3));
    }

    [Fact]
    public async Task UploadBlob()
    {
        var inventories = JsonConvert.SerializeObject(_fixture.CreateMany<Domain.Inventory>());
        var runTime = TestBlobRunTime.New(new Dictionary<string, string>());
        var uploadOperation = await BlobOperationsSchema<TestBlobRunTime>.Upload(
            new FileUploadRequest("1", "inventory", "inv", "a", inventories)
        ).Run(runTime);

        uploadOperation.IsSucc.Should().BeTrue();
        
        var readOperation = await BlobOperationsSchema<TestBlobRunTime>.ReadDataFromCsv<Domain.Inventory, InventoryMap>(
            new ReadFileRequest("1", "inventory", "inv", "a")
        ).Run(runTime);

        readOperation.IsSucc.Should().BeTrue();
        readOperation.IfSucc(data => data.Count.Should().Be(3));
    }
}
