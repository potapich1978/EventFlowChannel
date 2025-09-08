using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChannelReader.Abstract
{
    /// <summary>
    /// Defines a handler that processes a specific type of event.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    public interface IGenericEventHandler<T>
        where T : IComparable
    {
        /// <summary>
        /// Gets the type of event that this handler processes.
        /// </summary>
        T EventType { get; }

        /// <summary>
        /// Handles the specified event asynchronously.
        /// </summary>
        /// <param name="event">The event instance to handle.</param>
        /// <param name="token">cancellation token</param>
        /// <returns>A task representing the asynchronous handling operation.</returns>
        Task HandleAsync(IGenericEvent<T> @event, CancellationToken token = default);
    }
}
