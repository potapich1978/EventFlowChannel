using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using NSubstitute;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ChannelFactory.Tests
{
    /// <summary>
    /// Contains unit tests for the BaseChannelReader class.
    /// </summary>
    public class BaseChannelReaderTests
    {
        private readonly IChannelReader<int> _readerTaskBuilder;
        private readonly TestChannelReader _sut;
        private readonly IChannelOptions _channelOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseChannelReaderTests"/> class.
        /// Sets up the test dependencies and creates the system under test.
        /// </summary>
        public BaseChannelReaderTests()
        {
            _readerTaskBuilder = Substitute.For<IChannelReader<int>>();
            _channelOptions = Substitute.For<IChannelOptions>();
            _sut = new TestChannelReader(_channelOptions, _readerTaskBuilder);
        }

        /// <summary>
        /// Tests that the Start method initializes the channel and starts readers.
        /// Verifies that the channel is created and reader tasks are started.
        /// </summary>
        [Fact]
        public void Start_InitializesChannelAndStartsReaders()
        {
            // Arrange
            _channelOptions.ReadersCount.Returns(1);
            _readerTaskBuilder.ProduceReaderTask(
                    Arg.Any<Channel<IGenericEvent<int>>>(),
                    Arg.Any<int>(),
                    Arg.Any<CancellationToken>())
                .Returns([Task.CompletedTask]);

            // Act
            _sut.Start();

            // Assert
            Assert.NotNull(_sut.Channel);
            Assert.Single(_sut.TaskReaders);
        }

        /// <summary>
        /// Tests that the Stop method completes the channel and cancels readers.
        /// Verifies that the channel writer is completed and readers are cleared.
        /// </summary>
        [Fact]
        public void Stop_CompletesChannelAndCancelsReaders()
        {
            // Arrange
            _sut.Start();

            // Act
            _sut.Stop();

            // Assert
            Assert.Empty(_sut.TaskReaders);
        }

        /// <summary>
        /// Tests that the Enqueue method returns a faulted task when the channel is not ready.
        /// Verifies that an exception is thrown when trying to enqueue to a non-ready channel.
        /// </summary>
        [Fact]
        public void Enqueue_WhenChannelNotReady_ReturnsFaultedTask()
        {
            // Act
            var result =  _sut.Enqueue(Substitute.For<IGenericEvent<int>>());

            // Assert
            Assert.True(result.IsFaulted);
        }

        /// <summary>
        /// Tests that the Enqueue method successfully writes to the channel when ready.
        /// Verifies that messages can be enqueued after the channel is started.
        /// </summary>
        [Fact]
        public void Enqueue_WhenChannelReady_WritesMessage()
        {
            // Arrange
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();

            // Act
            var result = _sut.Enqueue(message).AsTask();

            // Assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        /// <summary>
        /// Tests that the Dispose method calls Stop.
        /// Verifies that resources are cleaned up when the object is disposed.
        /// </summary>
        [Fact]
        public void Dispose_CallsStop()
        {
            // Arrange
            _sut.Start();

            // Act
            _sut.Dispose();

            // Assert
            Assert.Empty(_sut.TaskReaders);
        }

        /// <summary>
        /// Tests that the Enqueue method handles exceptions during write operations correctly.
        /// Verifies that when an exception occurs during channel writing, 
        /// it is properly captured and returned as a faulted task.
        /// </summary>

        [Fact]
        public void Enqueue_WhenWriteThrowsException_ReturnsFaultedTask()
        {
            // Arrange
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();
            _sut.InternalChannel=null;

            // Act
            var result = _sut.Enqueue(message).AsTask();

            // Assert
            Assert.True(result.IsFaulted);
            Assert.IsType<ChannelClosedException>(result.Exception.InnerException);
        }

        /// <summary>
        /// Tests that the Stop method handles the scenario where InternalChannel is null.
        /// Verifies that no exception is thrown when stopping a channel that was never properly initialized.
        /// </summary>
        [Fact]
        public void Stop_WhenInternalChannelIsNull_CompletesWithoutError()
        {
            // Arrange
            // Use reflection to set InternalChannel to null
            var channelField = typeof(TestChannelReader).GetField("InternalChannel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            channelField.SetValue(_sut, null);

            // Use reflection to set _isRunning to 1 to bypass the early exit
            var runningField = typeof(TestChannelReader).GetField("_isRunning",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            runningField.SetValue(_sut, 1);

            // Act & Assert
            var exception = Record.Exception(() => _sut.Stop());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the Stop method handles the scenario where CancellationTokenSource is null.
        /// Verifies that no exception is thrown when stopping with a null CTS.
        /// </summary>
        [Fact]
        public void Stop_WhenCancellationTokenSourceIsNull_CompletesWithoutError()
        {
            // Arrange
            _sut.Start();
            _sut._cts=null;
            _sut._isRunning = 1;

            // Act & Assert
            var exception = Record.Exception(() => _sut.Stop());
            Assert.Null(exception);
        }


        // Test implementation of BaseChannelReader for testing
        private class TestChannelReader(IChannelOptions channelOptions, IChannelReader<int> readerTaskBuilder)
            : BaseChannelReader<int>(channelOptions, readerTaskBuilder)
        {
            public Channel<IGenericEvent<int>> Channel => InternalChannel ;
            public List<Task> TaskReaders => Readers;

            protected internal override Channel<IGenericEvent<int>> ProduceChannel(IChannelOptions channelOptions)
            {
                return System.Threading.Channels.Channel.CreateBounded<IGenericEvent<int>>(1);
            }
        }
    }
}