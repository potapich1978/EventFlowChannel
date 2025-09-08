using ChannelReader.Abstract;
using System;
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

        /// <summary>
        /// Enqueues a new event into the channel for asynchronous processing.
        /// </summary>
        /// <param name="message">The event to enqueue.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> representing the asynchronous operation.
        /// <para>If the channel is not ready, the task will complete with an <see cref="ChannelClosedException"/></para>
        /// <para>If channel is closing, the task will complete with an <see cref="TaskCanceledException"/></para>
        /// </returns>
        ValueTask Enqueue(IGenericEvent<T> message);
    }
}
