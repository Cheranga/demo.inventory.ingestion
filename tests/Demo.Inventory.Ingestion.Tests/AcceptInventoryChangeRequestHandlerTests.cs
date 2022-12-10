using BunsenBurner;
using Demo.Inventory.Ingestion.Functions.Extensions;
using Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges;
using Demo.Inventory.Ingestion.Tests.Extensions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Messaging.Azure.Queues;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Moq;
using static BunsenBurner.Aaa;
using static LanguageExt.Prelude;
using ErrorCodes = Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges.ErrorCodes;
using ErrorMessages = Demo.Inventory.Ingestion.Functions.Features.AcceptInventoryChanges.ErrorMessages;

namespace Demo.Inventory.Ingestion.Tests;

public static class AcceptInventoryChangeRequestHandlerTests
{
    [Fact(DisplayName = "executing with invalid input data must fail")]
    public static async Task InvalidRequestMustFail()
    {
        await Arrange(() =>
            {
                var mockedPublisher = new Mock<IMessagePublisher>();
                var mockedValidator = new Mock<IValidator<AcceptInventoryChangeRequest>>();
                mockedValidator
                    .Setup(
                        x =>
                            x.ValidateAsync(
                                It.IsAny<AcceptInventoryChangeRequest>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .ReturnsAsync(
                        new ValidationResult(new[] { new ValidationFailure("666", "error") })
                    );
                var mockedLogger = new Mock<ILogger<AcceptInventoryChangeRequestHandler>>();
                var handler = new AcceptInventoryChangeRequestHandler(
                    mockedPublisher.Object,
                    It.IsAny<AcceptInventorySettings>(),
                    mockedValidator.Object,
                    mockedLogger.Object
                );

                return (handler, mockedPublisher, mockedLogger);
            })
            .Act(
                async tuple =>
                    await tuple.handler.Handle(
                        new AcceptInventoryChangeRequest("666", "blah.csv"),
                        CancellationToken.None
                    )
            )
            .Assert(
                (tuple, datas) =>
                {
                    datas.IsLeft.Should().BeTrue();
                    datas.IfLeft(response =>
                    {
                        response.ErrorCode.Should().Be(ErrorCodes.InvalidData);
                        response.ErrorMessage.Should().Be(ErrorMessages.InvalidData);
                    });

                    tuple.mockedLogger.VerifyUsage(LogLevel.Error, Times.Once());
                    
                    tuple.mockedPublisher.Verify(
                        publisher =>
                            publisher.PublishAsync(
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<Func<string>>(),
                                It.IsAny<MessageSettings>(),
                                It.IsAny<ILogger>()
                            ),
                        Times.Never
                    );
                }
            );
    }
    
    [Fact(DisplayName = "when message publisher cannot publish must fail")]
    public static async Task WhenMessagePublisherCannotPublish()
    {
        await Arrange(() =>
            {
                var mockedPublisher = new Mock<IMessagePublisher>();
                mockedPublisher.Setup(x => x.PublishAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Func<string>>(),
                        It.IsAny<MessageSettings>(),
                        It.IsAny<ILogger>()))
                    .Returns(FailAff<Unit>(Error.New(666, "message publish error")));
                
                var mockedValidator = new Mock<IValidator<AcceptInventoryChangeRequest>>();
                mockedValidator
                    .Setup(
                        x =>
                            x.ValidateAsync(
                                It.IsAny<AcceptInventoryChangeRequest>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .ReturnsAsync(new ValidationResult());
                
                var mockedLogger = new Mock<ILogger<AcceptInventoryChangeRequestHandler>>();
                var handler = new AcceptInventoryChangeRequestHandler(
                    mockedPublisher.Object,
                    It.IsAny<AcceptInventorySettings>(),
                    mockedValidator.Object,
                    mockedLogger.Object
                );

                return (handler, mockedPublisher, mockedLogger);
            })
            .Act(
                async tuple =>
                    await tuple.handler.Handle(
                        new AcceptInventoryChangeRequest("666", "blah.csv"),
                        CancellationToken.None
                    )
            )
            .Assert(
                (tuple, datas) =>
                {
                    datas.IsLeft.Should().BeTrue();
                    datas.IfLeft(response =>
                    {
                        response.ErrorCode.Should().Be(666);
                        response.ErrorMessage.Should().Be("message publish error");
                    });

                    tuple.mockedPublisher.Verify(
                        publisher =>
                            publisher.PublishAsync(
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<Func<string>>(),
                                It.IsAny<MessageSettings>(),
                                It.IsAny<ILogger>()
                            ),
                        Times.Once
                    );

                    tuple.mockedLogger.VerifyUsage(LogLevel.Error, Times.Once());
                }
            );
    }
    
    [Fact(DisplayName = "all's well that ends well")]
    public static async Task AllIsWellThatEndsWell()
    {
        await Arrange(() =>
            {
                var mockedPublisher = new Mock<IMessagePublisher>();
                mockedPublisher.Setup(x => x.PublishAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Func<string>>(),
                        It.IsAny<MessageSettings>(),
                        It.IsAny<ILogger>()))
                    .Returns(SuccessAff(unit));
                
                var mockedValidator = new Mock<IValidator<AcceptInventoryChangeRequest>>();
                mockedValidator
                    .Setup(
                        x =>
                            x.ValidateAsync(
                                It.IsAny<AcceptInventoryChangeRequest>(),
                                It.IsAny<CancellationToken>()
                            )
                    )
                    .ReturnsAsync(new ValidationResult());
                
                var mockedLogger = new Mock<ILogger<AcceptInventoryChangeRequestHandler>>();
                var handler = new AcceptInventoryChangeRequestHandler(
                    mockedPublisher.Object,
                    It.IsAny<AcceptInventorySettings>(),
                    mockedValidator.Object,
                    mockedLogger.Object
                );

                return (handler, mockedPublisher, mockedLogger);
            })
            .Act(
                async tuple =>
                    await tuple.handler.Handle(
                        new AcceptInventoryChangeRequest("666", "blah.csv"),
                        CancellationToken.None
                    )
            )
            .Assert(
                (tuple, datas) =>
                {
                    datas.IsRight.Should().BeTrue();

                    tuple.mockedPublisher.Verify(
                        publisher =>
                            publisher.PublishAsync(
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<string>(),
                                It.IsAny<Func<string>>(),
                                It.IsAny<MessageSettings>(),
                                It.IsAny<ILogger>()
                            ),
                        Times.Once
                    );

                    tuple.mockedLogger.VerifyUsage(LogLevel.Information, Times.Once());
                }
            );
    }
}
