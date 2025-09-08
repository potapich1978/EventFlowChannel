using ChannelFactory.Abstract;

namespace ChannelFactory
{
    /// <summary>
    /// Defines configuration options for creating an unbounded channel.
    /// </summary>
    public sealed class UnboundedOptions : IChannelOptions
    {
        /// <summary>
        /// Gets a value indicating whether the channel supports a single writer optimization.
        /// </summary>
        public bool SingleWriter { get; set; }

        /// <summary>
        /// Gets the number of reader tasks to be created for this channel.
        /// </summary>
        public int ReadersCount { get; set; }
    }
}
