using ChannelFactory.Abstract;
using System.Threading.Channels;

namespace ChannelFactory
{
    /// <summary>
    /// Defines configuration options for creating a bounded channel.
    /// </summary>
    public sealed class BoundedOptions : IChannelOptions
    {
        /// <summary>
        /// Gets or sets the behavior applied when the channel is full.
        /// </summary>
        public BoundedChannelFullMode FullMode { get; set; } = BoundedChannelFullMode.Wait;

        /// <summary>
        /// Gets or sets the maximum capacity of the bounded channel.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets a value indicating whether the channel supports a single writer optimization.
        /// </summary>
        public bool SingleWriter { get; set; }

        /// <summary>
        /// Gets the number of reader tasks to be created for this channel.
        /// </summary>
        public int ReadersCount { get; set; } = 1;
    }
}
