using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using System;

namespace ChannelFactory
{
    /// <summary>
    /// Provides a factory implementation for creating bounded or unbounded channels.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
    internal sealed class ChannelFactory<T> : IChannelFactory<T>
        where T : IComparable
    {
        private readonly IChannelReader<T> _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFactory{T}"/> class.
        /// </summary>
        /// <param name="channelReader">The reader responsible for consuming events from the channel.</param>
        public ChannelFactory(IChannelReader<T> channelReader)
        {
            _reader = channelReader;
        }

        /// <summary>
        /// Produces a new channel instance based on the provided options.
        /// </summary>
        /// <param name="channelOptions">The configuration options that define the channel behavior.</param>
        /// <returns>An instance of <see cref="IChannel{T}"/> created according to the specified options.</returns>
        public IChannel<T> ProduceChannel(IChannelOptions channelOptions)
        {
            if(channelOptions == null)
                throw new ArgumentNullException(nameof(channelOptions));
            
            switch (channelOptions)
            {
                case BoundedOptions _:
                    return new BoundedChannelReader<T>(channelOptions, _reader);
                case UnboundedOptions _:
                    return new UnboundedChannelReader<T>(channelOptions, _reader);
                default:
                    throw new ArgumentException($"[{DateTime.Now:dd:MM:yy hh:mm:ss} ChannelFactory] " +
                                                $"unsupported implementation of IChannelOptions " +
                                                $"{channelOptions.GetType().FullName}");
            }
        }
    }
}
