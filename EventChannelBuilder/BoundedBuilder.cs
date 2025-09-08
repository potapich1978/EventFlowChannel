using ChannelFactory;
using ChannelFactory.Abstract;
using EventChannelBuilder.Abstract;
using System;
using System.Threading.Channels;

namespace EventChannelBuilder
{
    /// <summary>
    /// Internal implementation of <see cref="IBoundedChannelBuilder{T}"/> used to configure and build bounded channels.
    /// </summary>
    /// <typeparam name="T">The type of items transmitted through the channel.</typeparam>
    internal sealed class BoundedBuilder<T> : IBoundedChannelBuilder<T>
        where T : IComparable
    {
        private readonly IChannelFactory<T> _factory;
        private readonly BoundedOptions _options = new BoundedOptions();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedBuilder{T}"/> class.
        /// </summary>
        /// <param name="factory">The channel factory responsible for producing the channel.</param>
        public BoundedBuilder(IChannelFactory<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Sets the maximum capacity of the bounded channel.
        /// </summary>
        /// <param name="capacity">The maximum number of items allowed in the channel.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IBoundedChannelBuilder<T> WithCapacity(int capacity = 1000)
        {
            _options.Capacity = capacity;
            return this;
        }

        /// <summary>
        /// Sets the number of readers allowed to consume from the channel.
        /// </summary>
        /// <param name="count">The number of concurrent readers.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IBoundedChannelBuilder<T> WithReadersCount(int count = 1)
        {
            _options.ReadersCount = count;
            return this;
        }

        /// <summary>
        /// Configures whether the channel allows multiple writers.
        /// </summary>
        /// <param name="multi">True to allow multiple writers; false for a single writer.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IBoundedChannelBuilder<T> WithMultipleWriters(bool multi = true)
        {
            _options.SingleWriter = !multi;
            return this;
        }

        /// <summary>
        /// Configures the behavior when the channel reaches full capacity.
        /// </summary>
        /// <param name="mode">The <see cref="BoundedChannelFullMode"/> to apply when the channel is full.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        public IBoundedChannelBuilder<T> WithFullMode(BoundedChannelFullMode mode = BoundedChannelFullMode.Wait)
        {
            _options.FullMode = mode;
            return this;
        }

        /// <summary>
        /// Builds and returns a fully configured bounded channel.
        /// </summary>
        /// <returns>An instance of <see cref="IChannel{T}"/>.</returns>
        public IChannel<T> Build()
        {
            return _factory.ProduceChannel(_options);
        }
    }
}
