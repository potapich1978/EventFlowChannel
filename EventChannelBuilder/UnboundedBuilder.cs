using ChannelFactory;
using ChannelFactory.Abstract;
using EventChannelBuilder.Abstract;
using System;

namespace EventChannelBuilder
{
    /// <summary>
    /// Internal implementation of <see cref="IUnboundedChannelBuilder{T}"/> used to configure and build unbounded channels.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    internal sealed class UnboundedBuilder<T> : IUnboundedChannelBuilder<T>
        where T : IComparable
    {
        private readonly IChannelFactory<T> _factory;
        private readonly UnboundedOptions _options = new UnboundedOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundedBuilder{T}"/> class.
        /// </summary>
        /// <param name="factory">The channel factory responsible for producing the channel.</param>
        public UnboundedBuilder(IChannelFactory<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Sets the number of readers allowed to consume from the channel.
        /// </summary>
        /// <param name="count">The number of concurrent readers.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IUnboundedChannelBuilder<T> WithReadersCount(int count = 1)
        {
            _options.ReadersCount = count;
            return this;
        }

        /// <summary>
        /// Configures whether the channel allows multiple writers.
        /// </summary>
        /// <param name="multy">True to allow multiple writers; false for a single writer.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IUnboundedChannelBuilder<T> WithMultipleWriters(bool multy = true)
        {
            _options.SingleWriter = !multy;
            return this;
        }

        /// <summary>
        /// Builds and returns a fully configured unbounded channel.
        /// </summary>
        /// <returns>An instance of <see cref="IChannel{T}"/>.</returns>
        public IChannel<T> Build()
        {
            return _factory.ProduceChannel(_options);
        }
    }
}
