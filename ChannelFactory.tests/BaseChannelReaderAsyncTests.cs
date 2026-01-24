using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using NSubstitute;
using System.Threading.Channels;

namespace ChannelFactory.Tests
{
    /// <summary>
    /// Unit tests for BaseChannelReader focusing on EnqueueAsync
    /// and channel lifecycle behavior.
    /// </summary>
    public class BaseChannelReaderAsyncTests
    {
        private readonly TestChannelReader _sut;

        public BaseChannelReaderAsyncTests()
        {
            var readerTaskBuilder = Substitute.For<IChannelReader<int>>();
            var channelOptions = Substitute.For<IChannelOptions>();
            _sut = new TestChannelReader(channelOptions, readerTaskBuilder);

            channelOptions.ReadersCount.Returns(1);
            readerTaskBuilder
                .ProduceReaderTask(
                    Arg.Any<Channel<IGenericEvent<int>>>(),
                    Arg.Any<int>(),
                    Arg.Any<CancellationToken>())
                .Returns([Task.CompletedTask]);
        }

        [Fact]
        public void Start_InitializesChannelAndReaders()
        {
            _sut.Start();

            Assert.NotNull(_sut.Channel);
            Assert.Single(_sut.TaskReaders);
            Assert.Equal(1, _sut.IsRunning);
            Assert.False(_sut.Cts.Token.IsCancellationRequested);
        }

        [Fact]
        public void Stop_WhenCalled_MakesChannelUnavailable()
        {
            _sut.Start();
            _sut.Stop();

            Assert.Empty(_sut.TaskReaders);
            Assert.Equal(0, _sut.IsRunning);
            Assert.True(_sut.Cts.IsCancellationRequested);
        }

        [Fact]
        public async Task EnqueueAsync_WhenChannelReady_CompletesSuccessfully()
        {
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();

            var task = _sut.EnqueueAsync(message, CancellationToken.None);

            await task;
            Assert.True(task.IsCompletedSuccessfully);
        }
        
        [Fact]
        public async Task EnqueueAsync_WhenInternalChannelIsNull_ReturnsChannelClosedException()
        {
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();

            _sut.InternalChannel = null;

            var task = _sut.EnqueueAsync(message, CancellationToken.None).AsTask();

            var ex = await Assert.ThrowsAsync<ChannelClosedException>(() => task);
            Assert.Contains("channel not ready", ex.Message);
        }

        [Fact]
        public async Task EnqueueAsync_WhenChannelNotStarted_ThrowsChannelClosedException()
        {
            var message = Substitute.For<IGenericEvent<int>>();

            var task = _sut.EnqueueAsync(message, CancellationToken.None).AsTask();

            var ex = await Assert.ThrowsAsync<ChannelClosedException>(() => task);
            Assert.Equal("channel not ready", ex.Message);
        }

        [Fact]
        public async Task EnqueueAsync_WhenUserTokenCancelled_Throws_CancelEnqueueMessageException()
        {
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var task = _sut.EnqueueAsync(message, cts.Token).AsTask();

            await Assert.ThrowsAsync<CancelEnqueueMessageException>(() => task);
        }

        [Fact]
        public async Task EnqueueAsync_WhenChannelStoppedDuringWrite_throws_CancelEnqueueMessageException()
        {
            _sut.Start();
            var message = Substitute.For<IGenericEvent<int>>();
            
            await _sut.Cts.CancelAsync();
            var task = _sut.EnqueueAsync(message, CancellationToken.None).AsTask();

            await Assert.ThrowsAsync<CancelEnqueueMessageException>(() => task);
        }

        [Fact]
        public async Task EnqueueAsync_WhenInternalChannelIsNull_ThrowsChannelClosedException()
        {
            _sut.Start();
            _sut.InternalChannel = null;

            var message = Substitute.For<IGenericEvent<int>>();
            var task = _sut.EnqueueAsync(message, CancellationToken.None).AsTask();

            await Assert.ThrowsAsync<ChannelClosedException>(() => task);
        }

        [Fact]
        public void Dispose_CallsStopAndCleansUp()
        {
            _sut.Start();

            _sut.Dispose();

            Assert.Empty(_sut.TaskReaders);
        }

        // Test implementation
        private class TestChannelReader(
            IChannelOptions channelOptions,
            IChannelReader<int> readerTaskBuilder)
            : BaseChannelReader<int>(channelOptions, readerTaskBuilder)
        {
            public Channel<IGenericEvent<int>> Channel => InternalChannel;
            public List<Task> TaskReaders => Readers;

            protected internal override Channel<IGenericEvent<int>> ProduceChannel(IChannelOptions channelOptions)
            {
                return System.Threading.Channels.Channel.CreateBounded<IGenericEvent<int>>(1);
            }
        }
    }
}
