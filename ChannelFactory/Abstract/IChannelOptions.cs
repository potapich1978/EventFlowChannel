namespace ChannelFactory.Abstract
{
    /// <summary>
    /// Defines the configuration options for creating a channel.
    /// </summary>
    public interface IChannelOptions
    {
        /// <summary>
        /// Gets a value indicating whether the channel supports a single writer optimization.
        /// </summary>
        bool SingleWriter { get; }

        /// <summary>
        /// Gets the number of reader tasks to be created for this channel.
        /// </summary>
        int ReadersCount { get; }
    }
}
