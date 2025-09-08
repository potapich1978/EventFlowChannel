using ChannelFactory.Abstract;
using System;
using System.Threading.Channels;

namespace EventChannelBuilder.Abstract
{
    /// <summary>
    /// Provides configuration options for building a bounded channel.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    public interface IBoundedChannelBuilder<T>
        where T : IComparable
    {
        /// <summary>
        /// Sets the maximum capacity of the bounded channel.
        /// </summary>
        /// <param name="capacity">The maximum number of items allowed in the channel.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IBoundedChannelBuilder<T> WithCapacity(int capacity);

        /// <summary>
        /// Sets the number of readers allowed to consume from the channel.
        /// </summary>
        /// <param name="count">The number of concurrent readers.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IBoundedChannelBuilder<T> WithReadersCount(int count);

        /// <summary>
        /// Configures whether the channel allows multiple writers.
        /// </summary>
        /// <param name="multy">True to allow multiple writers; false for a single writer.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IBoundedChannelBuilder<T> WithMultipleWriters(bool multy = true);

        /// <summary>
        /// Configures the behavior when the channel reaches full capacity.
        /// </summary>
        /// <param name="mode">The <see cref="BoundedChannelFullMode"/> to apply when the channel is full.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IBoundedChannelBuilder<T> WithFullMode(BoundedChannelFullMode mode);

        /// <summary>
        /// Builds and returns a fully configured bounded channel.
        /// </summary>
        /// <returns>An instance of <see cref="IChannel{T}"/>.</returns>
        IChannel<T> Build();
    }
}
