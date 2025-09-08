using System;

namespace EventChannelBuilder.Abstract
{

    /// <summary>
    /// Defines a builder interface for creating channels with different configurations.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    public interface IChannelBuilder<T>
        where T : IComparable
    {
        /// <summary>
        /// Creates a bounded channel builder that allows configuration of capacity, readers, and writers.
        /// </summary>
        /// <returns>An instance of <see cref="IBoundedChannelBuilder{T}"/> for further configuration.</returns>
        IBoundedChannelBuilder<T> Bounded();

        /// <summary>
        /// Creates an unbounded channel builder that allows configuration of readers and writers.
        /// </summary>
        /// <returns>An instance of <see cref="IUnboundedChannelBuilder{T}"/> for further configuration.</returns>
        IUnboundedChannelBuilder<T> Unbounded();
    }
}
