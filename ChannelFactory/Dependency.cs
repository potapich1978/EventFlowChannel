using ChannelFactory.Abstract;
using ChannelReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace ChannelFactory
{
    /// <summary>
    /// Provides extension methods for registering channel factory and related services in the dependency injection container.
    /// </summary>
    public static class Dependency
    {
        /// <summary>
        /// Registers the channel factory, channel reader, and associated event handlers in the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of the event identifier, which must implement <see cref="IComparable"/>.</typeparam>
        /// <param name="services">The service collection to which services will be added.</param>
        /// <param name="handlersImplementationAssembly">
        /// The assembly that contains the implementations of event handlers.
        /// Used to automatically register available event handlers.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection"/> with channel factory services registered.</returns>
        public static IServiceCollection AddChannelFactory<T>(this IServiceCollection services, Assembly handlersImplementationAssembly)
            where T : IComparable
        {
            if(handlersImplementationAssembly == null)
                throw new ArgumentNullException(nameof(handlersImplementationAssembly));
            
            return services
                .AddChannelReader<T>(handlersImplementationAssembly)
                .AddSingleton<IChannelFactory<T>, ChannelFactory<T>>();
        }
    }
}
