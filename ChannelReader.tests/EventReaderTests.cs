using ChannelReader.Abstract;
using EventLogger;
using NSubstitute;
using System.Threading.Channels;

namespace ChannelReader.tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="EventReader{T}"/> class.
    /// </summary>
    public class EventReaderTests
    {
        private readonly IEventDispatcher<int> _dispatcher;
        private readonly IGenericEventDispatcherLogger _logger;
        private readonly EventReader<int> _eventReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventReaderTests"/> class.
        /// Sets up substitutes and creates the test instance.
        /// </summary>
        public EventReaderTests()
        {
            _dispatcher = Substitute.For<IEventDispatcher<int>>();
            _logger = Substitute.For<IGenericEventDispatcherLogger>();
            _eventReader = new EventReader<int>(_dispatcher, _logger);
        }

        /// <summary>
        /// Verifies that the number of reader tasks produced matches the specified readers count.
        /// </summary>
        [Fact]
        public void ProduceReaderTask_ShouldReturnCorrectNumberOfTasks()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            const int readersCount = 3;
            var token = CancellationToken.None;

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, readersCount, token);

            // Assert
            Assert.Equal(readersCount, new List<Task>(tasks).Count);
        }

        /// <summary>
        /// Verifies that events are properly dispatched when read from the channel.
        /// </summary>
        [Fact]
        public async Task ReadChannelItem_ShouldDispatchEvents()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            var eventMock = Substitute.For<IGenericEvent<int>>();
            var token = new CancellationTokenSource().Token;

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, 1, token);
            await channel.Writer.WriteAsync(eventMock, token);
            channel.Writer.Complete();

            // Assert
            await Task.WhenAll(tasks);
            await _dispatcher.Received(1).DispatchEventAsync(eventMock, Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Verifies that the reader logs a warning when operation is canceled.
        /// </summary>
        [Fact]
        public async Task ReadChannelItem_ShouldLogWarning_WhenCancelled()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, 1, token);
            await cts.CancelAsync();

            // Assert
            await Task.WhenAll(tasks);
            _logger.Received().LogWarning(Arg.Is<string>(s => s.Contains("service stoped")));
        }

        /// <summary>
        /// Verifies that the reader logs an error when an exception occurs during event processing.
        /// </summary>
        [Fact]
        public async Task ReadChannelItem_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            var eventMock = Substitute.For<IGenericEvent<int>>();
            var token = new CancellationTokenSource().Token;
            var exception = new InvalidOperationException("Test exception");

            _dispatcher
                .When(d => d.DispatchEventAsync(Arg.Any<IGenericEvent<int>>(), Arg.Any<CancellationToken>()))
                .Do(_ => throw exception);

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, 1, token);
            await channel.Writer.WriteAsync(eventMock, token);
            channel.Writer.Complete();

            // Assert
            await Task.WhenAll(tasks);
            _logger.Received().LogError(Arg.Is<string>(s => s.Contains("unhandled exception")), exception);
        }

        /// <summary>
        /// Verifies that multiple events are processed when multiple readers are active.
        /// </summary>
        [Fact]
        public async Task MultipleReaders_ShouldProcessAllEvents()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            var events = new List<IGenericEvent<int>>();
            var token = new CancellationTokenSource().Token;

            for (var i = 0; i < 5; i++)
            {
                var eventMock = Substitute.For<IGenericEvent<int>>();
                events.Add(eventMock);
            }

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, 2, token);

            foreach (var eventItem in events)
            {
                await channel.Writer.WriteAsync(eventItem, token);
            }

            channel.Writer.Complete();

            // Assert
            await Task.WhenAll(tasks);
            foreach (var eventItem in events)
            {
                await _dispatcher.Received().DispatchEventAsync(eventItem, Arg.Any<CancellationToken>());
            }
        }

        /// <summary>
        /// Verifies that the reader continues processing after handling an exception.
        /// </summary>
        [Fact]
        public async Task ReadChannelItem_ShouldContinue_AfterException()
        {
            // Arrange
            var channel = Channel.CreateUnbounded<IGenericEvent<int>>();
            var failingEvent = Substitute.For<IGenericEvent<int>>();
            var successfulEvent = Substitute.For<IGenericEvent<int>>();
            var token = new CancellationTokenSource().Token;
            var exception = new InvalidOperationException("Test exception");

            _dispatcher
                .When(d => d.DispatchEventAsync(failingEvent, Arg.Any<CancellationToken>()))
                .Do(_ => throw exception);

            // Act
            var tasks = _eventReader.ProduceReaderTask(channel, 1, token);
            await channel.Writer.WriteAsync(failingEvent, token);
            await channel.Writer.WriteAsync(successfulEvent, token);
            channel.Writer.Complete();

            // Assert
            await Task.WhenAll(tasks);
            
            _logger.Received().LogError(Arg.Is<string>(s => s.Contains("unhandled exception")), exception);
            await _dispatcher.Received().DispatchEventAsync(successfulEvent, Arg.Any<CancellationToken>());
        }
    }
}