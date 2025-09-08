using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChannelReader.Abstract
{
    /// <summary>
    /// Defines a dispatcher that routes events to their corresponding handlers.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    public interface IEventDispatcher<T>
        where T : IComparable
    {
        /// <summary>
        /// Dispatches the specified event to its associated handler asynchronously.
        /// </summary>
        /// <param name="event">The event to be dispatched.</param>
        /// <param name="token">The cancellation token used for cancel task.</param>
        /// <returns>A task representing the asynchronous dispatch operation.</returns>
        Task DispatchEventAsync(IGenericEvent<T> @event, CancellationToken token = default);
    }
}
