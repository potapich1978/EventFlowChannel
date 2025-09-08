using ChannelFactory.Abstract;
using System;

namespace EventChannelBuilder.Abstract
{
    /// <summary>
    /// Provides configuration options for building an unbounded channel.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    public interface IUnboundedChannelBuilder<T>
        where T : IComparable
    {

        /// <summary>
        /// Sets the number of readers allowed to consume from the channel.
        /// </summary>
        /// <param name="count">The number of concurrent readers.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IUnboundedChannelBuilder<T> WithReadersCount(int count);

        /// <summary>
        /// Configures whether the channel allows multiple writers.
        /// </summary>
        /// <param name="multy">True to allow multiple writers; false for a single writer.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        IUnboundedChannelBuilder<T> WithMultipleWriters(bool multy = true);


        /// <summary>
        /// Builds and returns a fully configured unbounded channel.
        /// </summary>
        /// <returns>An instance of <see cref="IChannel{T}"/>.</returns>
        IChannel<T> Build();
    }
}
