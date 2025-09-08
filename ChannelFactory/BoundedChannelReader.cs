using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using System;
using System.Threading.Channels;

namespace ChannelFactory
{
    /// <summary>
    /// Provides a bounded channel reader implementation that restricts the number of items in the channel.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
    internal sealed class BoundedChannelReader<T> : BaseChannelReader<T>
        where T : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedChannelReader{T}"/> class.
        /// </summary>
        /// <param name="channelOptions">The bounded channel options that configure this instance.</param>
        /// <param name="readerTaskBuilder">The reader task builder for processing channel events.</param>
        public BoundedChannelReader(IChannelOptions channelOptions, IChannelReader<T> readerTaskBuilder)
            : base(channelOptions, readerTaskBuilder) { }

        /// <summary>
        /// Produces a channel instance based on the provided configuration options.
        /// Must be implemented by derived classes to define specific channel creation logic.
        /// </summary>
        /// <param name="channelOptions">The configuration options for the channel.</param>
        /// <returns>A configured <see cref="Channel{T}"/> instance.</returns>
        protected internal override Channel<IGenericEvent<T>> ProduceChannel(IChannelOptions channelOptions)
        {
            return channelOptions is BoundedOptions boundedMultiReader
                ? Channel.CreateBounded<IGenericEvent<T>>(new BoundedChannelOptions(boundedMultiReader.Capacity)
                {
                    FullMode = boundedMultiReader.FullMode,
                    SingleReader = channelOptions.ReadersCount == 1,
                    SingleWriter = boundedMultiReader.SingleWriter
                })
                : null;
        }
    }
}
