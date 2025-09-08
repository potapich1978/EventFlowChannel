using ChannelFactory;
using EventChannelBuilder.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace EventChannelBuilder
{
    /// <summary>
    /// Provides extension methods for registering the event channel builder
    /// and its dependencies into the dependency injection container.
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// Registers the event channel builder, along with channel factory and reader services,
        /// into the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
        /// <param name="services">The service collection used for dependency injection.</param>
        /// <param name="handlersImplementationAssembly">
        /// The assembly that contains implementations of event handlers.
        /// Used to automatically register all available event handlers for the given event type.
        /// </param>
        /// <returns>
        /// The updated <see cref="IServiceCollection"/> with event channel builder services registered.
        /// </returns>
        public static IServiceCollection AddEventChannelBuilder<T>(this IServiceCollection services, Assembly handlersImplementationAssembly)
            where T : IComparable
        {
            return services
                .AddChannelFactory<T>(handlersImplementationAssembly)
                .AddSingleton<IChannelBuilder<T>, EventChannelBuilder<T>>();
        }
    }
}
