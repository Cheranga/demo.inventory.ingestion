using FluentAssertions;
using LanguageExt;

namespace Demo.Inventory.Ingestion.Tests.Infrastructure.Demo.FileIO;

public static class FileIOTests
{
    [Fact]
    public static async Task LiveEnvironment()
    {
        var runTime = LiveFileRunTime.New();

        var inputFilePath = @"c:\Cheranga\test.txt";

        var operation = await (
            from op in FileOperationsSchema<LiveFileRunTime>.ReadAllText(inputFilePath)
            select op
        ).Run(runTime);

        operation.IsSucc.Should().BeTrue();
        operation.IfSucc(content => content.Should().Be("Cheranga Hatangala"));
    }

    [Fact]
    public static async Task TestEnvironment()
    {
        var demoFiles = new Dictionary<string, string> { { "test.txt", "Cheranga Hatangala" } };

        var runTime = TestFileRunTime.New(demoFiles);

        var existingFile = "test.txt";

        var operation = await (
            from op in FileOperationsSchema<TestFileRunTime>.ReadAllText(existingFile)
            select op
        ).Run(runTime);

        operation.IsSucc.Should().BeTrue();
        operation.IfSucc(content => content.Should().Be("Cheranga Hatangala"));

        var nonExistingFile = "blah.txt";
        operation = await (
            from op in FileOperationsSchema<TestFileRunTime>.ReadAllText(nonExistingFile)
            select op
        ).Run(runTime);

        operation.IsFail.Should().BeTrue();
        operation.IfFail(error =>
        {
            var exception = error.ToException();
            exception.Should().BeOfType<FileNotFoundException>();
            exception.Message.Should().Be("blah.txt not found");
        });
    }
}
