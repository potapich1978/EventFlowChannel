using ChannelFactory.Abstract;
using EventChannelBuilder.Abstract;
using System;

namespace EventChannelBuilder
{
    /// <summary>
    /// Provides a fluent API for building bounded and unbounded channels.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    public sealed class EventChannelBuilder<T> : IChannelBuilder<T>
        where T : IComparable
    {
        private readonly IChannelFactory<T> _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventChannelBuilder{T}"/> class.
        /// </summary>
        /// <param name="factory">The channel factory used to create channel instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        public EventChannelBuilder(IChannelFactory<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates a bounded channel builder that allows configuration of capacity, readers, and writers.
        /// </summary>
        /// <returns>An instance of <see cref="IBoundedChannelBuilder{T}"/> for further configuration.</returns>
        public IBoundedChannelBuilder<T> Bounded()
        {
            return new BoundedBuilder<T>(_factory);
        }

        /// <summary>
        /// Creates an unbounded channel builder that allows configuration of readers and writers.
        /// </summary>
        /// <returns>An instance of <see cref="IUnboundedChannelBuilder{T}"/> for further configuration.</returns>
        public IUnboundedChannelBuilder<T> Unbounded()
        {
            return new UnboundedBuilder<T>(_factory);
        }
    }
}
