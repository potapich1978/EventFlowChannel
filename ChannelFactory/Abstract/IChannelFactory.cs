using System;

namespace ChannelFactory.Abstract
{
    /// <summary>
    /// Provides a factory abstraction for creating channels based on configuration options.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
    public interface IChannelFactory<T>
        where T : IComparable
    {
        /// <summary>
        /// Produces a new channel instance based on the provided options.
        /// </summary>
        /// <param name="channelOptions">The configuration options that define the channel behavior.</param>
        /// <returns>An instance of <see cref="IChannel{T}"/> created according to the specified options.</returns>
        IChannel<T> ProduceChannel(IChannelOptions channelOptions);
    }
}
