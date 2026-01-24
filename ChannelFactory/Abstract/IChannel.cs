using ChannelReader.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace ChannelFactory.Abstract
{
    /// <summary>
    /// Represents a communication channel abstraction used for event-driven processing.
    /// Provides lifecycle management methods and the ability to enqueue events for handling.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
    public interface IChannel<T> : IDisposable
        where T : IComparable
    {
        /// <summary>
        /// Stops the channel and cancels all running operations.
        /// Ensures that no further events will be enqueued or processed.
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts the channel, initializing required resources and readers.
        /// Must be called before enqueuing events.
        /// </summary>
        void Start();
        
        [Obsolete("Use EnqueueAsync instead. Will be removed in v2.0.")]
        ValueTask Enqueue(IGenericEvent<T> messageReadAsync);

        /// <summary>
        /// Enqueues a message to the channel.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="token">
        /// A cancellation token that can be used to cancel the write operation.
        /// When using BoundedChannelFullMode.Wait, consider providing a timeout
        /// to prevent uncontrolled memory growth with slow consumers and fast producers.
        /// </param>
        /// <returns>A ValueTask representing the asynchronous operation.</returns>
        /// <exception cref="ChannelClosedException">Thrown when the channel is not running.</exception>
        /// <exception cref="CancelEnqueueMessageException">Thrown when the operation is cancelled by channel stop.</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled by the provided token.</exception>
        ValueTask EnqueueAsync(IGenericEvent<T> message, CancellationToken token);
    }
}
