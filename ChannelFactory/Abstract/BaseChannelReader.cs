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
        private readonly object _locker;

        internal CancellationTokenSource Cts;
        internal int IsRunning;

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
            if (Volatile.Read(ref IsRunning) == 0)
                return new ValueTask(Task.FromException(new ChannelClosedException("channel not ready")));
            try
            {
                return InternalChannel.Writer.WriteAsync(message, Cts.Token);
            }
            catch (Exception e)
            {
                return new ValueTask(Task.FromException(new ChannelClosedException("channel not ready",e)));
            }
        }

        /// <inheritdoc/>
        public async ValueTask EnqueueAsync(IGenericEvent<T> message, CancellationToken token)
        {
            if (Volatile.Read(ref IsRunning) == 0 || InternalChannel ==null)
                 throw new ChannelClosedException("channel not ready");

            try
            {
                using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, Cts.Token))
                    // The write operation observes two cancellation sources:
                    // 1. The external token provided by the caller (caller-controlled cancellation).
                    // 2. The internal channel lifetime token (cancelled on Stop()).
                    //
                    // These tokens are linked to ensure that enqueue is cancelled immediately
                    // when either the caller requests cancellation or the channel is stopped.
                    //
                    // No explicit synchronization with Stop() is required:
                    // - If Stop() completes the channel or cancels the internal token concurrently,
                    //   WriteAsync will throw ChannelClosedException or TaskCanceledException.
                    // - Such cases are expected and translated into a domain-specific exception below.
                    //
                    // Using a linked CancellationTokenSource here provides clear cancellation semantics
                    // at the cost of a small allocation, which is acceptable for the current design.
                    await InternalChannel.Writer.WriteAsync(message, linkedToken.Token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw new CancelEnqueueMessageException("Operation cancelled by user token.");
            }
            catch (OperationCanceledException) when (Cts.Token.IsCancellationRequested)
            {
                throw new CancelEnqueueMessageException("Channel close");
            }            
            catch (Exception e)
            {
                throw new ChannelClosedException("channel not ready", e);
            }
        }

        ///<inheritdoc/>
        public void Stop()
        {
            if (Volatile.Read(ref IsRunning) == 0)
                return;

            lock (_locker)
            {
                if (Volatile.Read(ref IsRunning) == 0)
                    return;

                Interlocked.Exchange(ref IsRunning, 0);
                InternalChannel?.Writer.Complete();
                Cts?.Cancel();
                
                // Readers will naturally stop once the channel is completed 
                // and the cancellation token is observed.
                // No need to wait for their completion here.
                Readers.Clear();
                
                // Disposing CTS is safe because each reader already captured
                // its own CancellationToken struct by value. They do not rely
                // on this reference anymore, and WaitToReadAsync(token) will 
                // still correctly throw OperationCanceledException when needed.
                Cts?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            lock (_locker)
            {
                Stop();
                Cts = new CancellationTokenSource();
                InternalChannel = ProduceChannel(_channelOptions);
                Interlocked.Exchange(ref IsRunning, 1);
                foreach (var readerTask in _readerTaskBuilder.ProduceReaderTask(InternalChannel, _channelOptions.ReadersCount, Cts.Token))
                    Readers.Add(readerTask);
            }
        }

        /// <summary>
        /// Initialize a new instance of <see cref="Channel{T}"/>}"/>
        /// </summary>
        /// <param name="channelOptions"></param>
        /// <returns></returns>
        protected internal abstract Channel<IGenericEvent<T>> ProduceChannel(IChannelOptions channelOptions);

        public void Dispose()
        {
            Stop();
        }
    }
}
