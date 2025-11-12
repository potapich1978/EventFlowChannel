using ChannelReader.Abstract;
using EventLogger;
using NSubstitute;

namespace ChannelReader.tests
{
    public class EventDispatcerTests
    {
        [Fact]
        public async Task DispatchEventAsync_Should_Invoke_Handler_When_Registered()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var handler = Substitute.For<IGenericEventHandler<string>>();
            var @event = Substitute.For<IGenericEvent<string>>();
            @event.EventType.Returns("Test");
            handler.EventType.Returns("Test");

            var handlers = new Dictionary<string, IGenericEventHandler<string>>
            {
                { "Test", handler }
            };

            var dispatcher = new EventDispatcher<string>(handlers, logger);

            // Act
            await dispatcher.DispatchEventAsync(@event);

            // Assert
            await handler.Received(1).HandleAsync(@event);
            logger.DidNotReceiveWithAnyArgs().LogWarning(default!);
            logger.DidNotReceiveWithAnyArgs().LogError(default!);
        }

        [Fact]
        public async Task DispatchEventAsync_Should_LogError_When_Handler_Throws()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var handler = Substitute.For<IGenericEventHandler<string>>();
            var @event = Substitute.For<IGenericEvent<string>>();
            @event.EventType.Returns("Fail");
            handler.EventType.Returns("Fail");

            handler.HandleAsync(@event).Returns<Task>(_ => throw new InvalidOperationException("boom"));

            var handlers = new Dictionary<string, IGenericEventHandler<string>>
            {
                { "Fail", handler }
            };

            var dispatcher = new EventDispatcher<string>(handlers, logger);

            // Act
            try
            {
                await dispatcher.DispatchEventAsync(@event);
            }
            catch 
            {
            }

            // Assert
            await handler.Received(1).HandleAsync(@event);
            logger.ReceivedWithAnyArgs(1).LogError(default!);
        }

        [Fact]
        public async Task DispatchEventAsync_Should_LogWarning_When_Handler_Not_Found()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var @event = Substitute.For<IGenericEvent<string>>();
            @event.EventType.Returns("Unknown");

            var handlers = new Dictionary<string, IGenericEventHandler<string>>();

            var dispatcher = new EventDispatcher<string>(handlers, logger);

            // Act
            await dispatcher.DispatchEventAsync(@event);

            // Assert
            logger.ReceivedWithAnyArgs(1).LogWarning(default!);
        }
    }
}
