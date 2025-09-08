using System;

namespace ChannelReader.Abstract
{
    /// <summary>
    /// Represents a generic event that can be dispatched and handled.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    public interface IGenericEvent<T>
        where T : IComparable
    {
        /// <summary>
        /// Gets the event type identifier associated with this event.
        /// </summary>
        T EventType { get; }
    }
}
