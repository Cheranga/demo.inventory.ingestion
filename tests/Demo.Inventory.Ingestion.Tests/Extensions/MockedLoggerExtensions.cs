using Microsoft.Extensions.Logging;
using Moq;

namespace Demo.Inventory.Ingestion.Tests.Extensions;

public static class MockedLoggerExtensions
{
    public static void VerifyUsage<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
    {
        logger.Verify(x => x.Log(level,
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()!), times);
    }
}