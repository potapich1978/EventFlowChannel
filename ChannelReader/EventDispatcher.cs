using ChannelReader.Abstract;
using EventLogger;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChannelReader
{
    /// <summary>
    /// Default implementation of <see cref="IEventDispatcher{T}"/> that dispatches events
    /// to registered handlers based on their event type.
    /// </summary>
    /// <typeparam name="T">The type of the event identifier.</typeparam>
    internal sealed class EventDispatcher<T> : IEventDispatcher<T>
        where T : IComparable
    {
        private readonly Dictionary<T, IGenericEventHandler<T>> _handlers;

        private readonly IGenericEventDispatcherLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDispatcher{T}"/> class.
        /// </summary>
        /// <param name="handlers">A dictionary of registered event handlers keyed by event type.</param>
        /// <param name="logger">The logger used to record dispatcher activity.</param>
        public EventDispatcher(Dictionary<T, IGenericEventHandler<T>> handlers, IGenericEventDispatcherLogger logger)
        {
            _logger = logger;
            _handlers = handlers;
        }

        /// <summary>
        /// Dispatches the specified event to its associated handler asynchronously.
        /// </summary>
        /// <param name="event">The event to be dispatched.</param>
        /// <param name="token">The cancellation token used for cancel task.</param>
        /// <returns>A task representing the asynchronous dispatch operation.</returns>
        public async Task DispatchEventAsync(IGenericEvent<T> @event, CancellationToken token = default)
        {
            if (_handlers.TryGetValue(@event.EventType, out var handler))
            {
                try
                {
                    await handler.HandleAsync(@event, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[{DateTime.Now:dd:MM:yy HH:mm:ss} EventDispatcher] " +
                                      $"unhandled exception was throw in handler for event with type {@event.EventType}: " +
                                      $"{ex}");

                    //await Task.CompletedTask;
                }
            }
            else
            {
                _logger.LogWarning($"[{DateTime.Now:dd:MM:yy HH:mm:ss} EventDispatcher] " +
                                  $"can't dispatch event with type {@event.EventType}, " +
                                  $"handler not registered");
            }
        }
    }
}
