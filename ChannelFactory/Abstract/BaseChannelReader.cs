using ChannelReader.Abstract;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelFactory.Abstract
{
    /// <summary>
    /// Provides a base implementation for channel readers that process events from a channel.
    /// Manages channel lifecycle, readers, and event dispatching.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
    internal abstract class BaseChannelReader<T> : IChannel<T>
        where T : IComparable
    {
        private readonly IChannelReader<T> _readerTaskBuilder;
        private readonly IChannelOptions _channelOptions;
        internal object _locker;

        internal CancellationTokenSource _cts;
        internal int _isRunning = 0;

        protected readonly List<Task> Readers;
        protected internal Channel<IGenericEvent<T>> InternalChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseChannelReader{T}"/> class.
        /// </summary>
        /// <param name="channelOptions">The configuration options for the channel.</param>
        /// <param name="readerTaskBuilder">The reader task builder responsible for creating reader tasks.</param>
        protected BaseChannelReader(IChannelOptions channelOptions, IChannelReader<T> readerTaskBuilder)
        {
            _channelOptions = channelOptions;
            _readerTaskBuilder = readerTaskBuilder;
            _locker = new object();
            Readers = new List<Task>();
        }
        
        /// <inheritdoc/>
        public ValueTask Enqueue(IGenericEvent<T> message)
        {
            if (Volatile.Read(ref _isRunning) == 0)
                return new ValueTask(Task.FromException(new ChannelClosedException("channel not ready")));

            try
            {
                // Intentionally not synchronized with Stop():
                // - Stop() may complete the channel or cancel the token while we write.
                // - In such cases WriteAsync will throw (ChannelClosedException/TaskCanceledException),
                //   which is expected and safely handled below.
                // - Using _cts.Token without synchronization is safe here:
                //   CancellationToken is a struct captured by value; once obtained,
                //   it remains valid even if the underlying CTS is later disposed.
                return InternalChannel.Writer.WriteAsync(message, _cts.Token);
            }
            catch (Exception e)
            {
                return new ValueTask(Task.FromException(new ChannelClosedException("channel not ready",e)));
            }
        }

        ///<inheritdoc/>
        public void Stop()
        {
            if (Volatile.Read(ref _isRunning) == 0)
                return;

            lock (_locker)
            {
                if (Volatile.Read(ref _isRunning) == 0)
                    return;

                Interlocked.Exchange(ref _isRunning, 0);
                InternalChannel?.Writer.Complete();
                _cts?.Cancel();
                
                // Readers will naturally stop once the channel is completed 
                // and the cancellation token is observed.
                // No need to wait for their completion here.
                Readers.Clear();
                
                // Disposing CTS is safe because each reader already captured
                // its own CancellationToken struct by value. They do not rely
                // on this reference anymore, and WaitToReadAsync(token) will 
                // still correctly throw OperationCanceledException when needed.
                _cts?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            lock (_locker)
            {
                Stop();
                _cts = new CancellationTokenSource();
                InternalChannel = ProduceChannel(_channelOptions);
                Interlocked.Exchange(ref _isRunning, 1);
                foreach (var readerTask in _readerTaskBuilder.ProduceReaderTask(InternalChannel, _channelOptions.ReadersCount, _cts.Token))
                    Readers.Add(readerTask);
            }
        }

        /// <inheritdoc/>
        protected internal abstract Channel<IGenericEvent<T>> ProduceChannel(IChannelOptions channelOptions);

        public void Dispose()
        {
            Stop();
        }
    }
}
