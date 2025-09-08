using ChannelReader.Abstract;
using EventLogger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChannelReader
{
    /// <summary>
    /// Provides functionality to continuously read events from a channel and dispatch them for handling.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    internal class EventReader<T> : IChannelReader<T>
        where T : IComparable
    {
        private readonly IEventDispatcher<T> _dispatcher;

        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventReader{T}"/> class.
        /// </summary>
        /// <param name="dispatcher">The dispatcher responsible for routing events to handlers.</param>
        /// <param name="logger">The logger used to record reader activity.</param>
        public EventReader(IEventDispatcher<T> dispatcher, IGenericEventDispatcherLogger logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }


        /// <summary>
        /// Produces a collection of reader tasks that continuously read events from the specified channel.
        /// </summary>
        /// <param name="channel">The channel instance from which events will be read.</param>
        /// <param name="readersCount">The number of concurrent readers to be started.</param>
        /// <param name="token">The cancellation token used to stop the reading process.</param>
        /// <returns>A collection of <see cref="Task"/> instances representing the readers.</returns>
        public IEnumerable<Task> ProduceReaderTask(Channel<IGenericEvent<T>> channel, int readersCount, CancellationToken token)
        {
            for (int i = 0; i < readersCount; i++)
                yield return ReadChannelItem(channel, token);
        }

        /// <summary>
        /// Continuously reads events from the channel and dispatches them until cancellation is requested.
        /// </summary>
        /// <param name="channel">The channel instance to read from.</param>
        /// <param name="token">The cancellation token used to stop the reading process.</param>
        private async Task ReadChannelItem(Channel<IGenericEvent<T>> channel, CancellationToken token)
        {
            try
            {
                while (await channel.Reader.WaitToReadAsync(token))
                {
                    while (channel.Reader.TryRead(out var @event))
                    {
                        try
                        {
                            await _dispatcher.DispatchEventAsync(@event, token);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"[{DateTime.Now:HH:mm:ss} EventReader]: unhandled exception while process event " +
                                             $"{@event.GetType().FullName} {e.Message}", e);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"[{DateTime.Now:HH:mm:ss} EventReader]: service stoped");
            }
        }
    }
}