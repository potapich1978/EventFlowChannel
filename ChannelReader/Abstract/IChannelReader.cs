using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelReader.Abstract
{
    /// <summary>
    /// Defines an abstraction for reading events from a channel.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    public interface IChannelReader<T>
        where T : IComparable

    {
        /// <summary>
        /// Produces a collection of reader tasks that continuously read events from the specified channel.
        /// </summary>
        /// <param name="channel">The channel instance from which events will be read.</param>
        /// <param name="readersCount">The number of concurrent readers to be started.</param>
        /// <param name="token">The cancellation token used to stop the reading process.</param>
        /// <returns>A collection of <see cref="Task"/> instances representing the readers.</returns>
        IEnumerable<Task> ProduceReaderTask(Channel<IGenericEvent<T>> channel, int readersCount, CancellationToken token);
    }
}
