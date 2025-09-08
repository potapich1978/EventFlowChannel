using ChannelReader.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChannelReader
{
    /// <summary>
    /// Provides extension methods for registering channel reader and event dispatcher services in the dependency injection container.
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// Registers channel reader services, including the event dispatcher and event reader,
        /// into the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <param name="handlersImplementationAssembly">
        /// The assembly that contains implementations of <see cref="IGenericEventHandler{T}"/>.
        /// Used for automatic discovery and registration of event handlers.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> with channel reader services registered.</returns>
        public static IServiceCollection AddChannelReader<T>(this IServiceCollection services, Assembly handlersImplementationAssembly)
            where T : IComparable
        {
            return services
                .AddEventDispatcher<T>(handlersImplementationAssembly)
                .AddSingleton<IChannelReader<T>, EventReader<T>>();
        }

        /// <summary>
        /// Registers the event dispatcher and discovers all available event handler implementations
        /// in the specified assembly. Event handlers are automatically instantiated and added
        /// to the dispatcher’s handler collection.
        /// </summary>
        /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <param name="handlersImplementationAssembly">
        /// The assembly containing all event handler implementations for the given event type.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> with event dispatcher services registered.</returns>
        private static IServiceCollection AddEventDispatcher<T>(this IServiceCollection services, Assembly handlersImplementationAssembly)
             where T : IComparable
        {
            return services
                    .AddSingleton(provider =>
                    {
                        var handlers = new Dictionary<T, IGenericEventHandler<T>>();
                        var handlerTypes = handlersImplementationAssembly
                            .GetTypes()
                            .Where(t =>
                                typeof(IGenericEventHandler<T>).IsAssignableFrom(t) &&
                                t.IsClass &&
                                !t.IsAbstract);

                        foreach (var type in handlerTypes)
                        {
                            var instance = (IGenericEventHandler<T>)ActivatorUtilities.CreateInstance(provider, type);
                            handlers[instance.EventType] = instance;
                        }

                        return handlers;
                    })
                    .AddSingleton<IEventDispatcher<T>, EventDispatcher<T>>();
        }
    }
}
