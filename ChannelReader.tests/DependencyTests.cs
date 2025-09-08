using ChannelReader.Abstract;
using EventLogger;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;

namespace ChannelReader.tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="Dependency"/> class.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Verifies that AddChannelReader method returns the same service collection instance for method chaining.
        /// </summary>
        [Fact]
        public void AddChannelReader_ShouldReturnSameServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = services.AddChannelReader<int>(assembly);

            // Assert
            Assert.Same(services, result);
        }

        /// <summary>
        /// Verifies that AddChannelReader method registers IChannelReader service as singleton.
        /// </summary>
        [Fact]
        public void AddChannelReader_ShouldRegisterIChannelReaderAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            services.AddChannelReader<int>(assembly);

            // Assert
            var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IChannelReader<int>));
            Assert.NotNull(descriptor);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        /// <summary>
        /// Verifies that the service provider can resolve all registered dependencies without errors.
        /// </summary>
        [Fact]
        public void ServiceProvider_ShouldResolveAllDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();

            // Mock the logger dependency
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            services.AddSingleton(logger);

            // Register channel reader services
            services.AddChannelReader<int>(assembly);

            // Act & Assert
            var serviceProvider = services.BuildServiceProvider();

            // Should not throw when resolving
            var channelReader = serviceProvider.GetService<IChannelReader<int>>();
            var eventDispatcher = serviceProvider.GetService<IEventDispatcher<int>>();
            var handlers = serviceProvider.GetService<Dictionary<int, IGenericEventHandler<int>>>();

            Assert.NotNull(channelReader);
            Assert.NotNull(eventDispatcher);
            Assert.NotNull(handlers);
        }

        /// <summary>
        /// Verifies that the handlers dictionary is populated with event handlers from the assembly.
        /// </summary>
        [Fact]
        public void HandlersDictionary_ShouldContainEventHandlersFromAssembly()
        {
            // This test requires at least one test event handler in the test assembly
            // Arrange
            var services = new ServiceCollection();
            var assembly = Assembly.GetExecutingAssembly();
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            services.AddSingleton(logger);

            //Action
            var provider = services.AddChannelReader<int>(assembly).BuildServiceProvider();

            // Assert
            var handlers = provider.GetService<Dictionary<int, IGenericEventHandler<int>>>();
            Assert.IsType<TestEventHandler>(handlers[1]);
            
        }
    }

    // Test event handler implementation for testing
    internal class TestEventHandler : IGenericEventHandler<int>
    {
        public int EventType => 1;

        public Task HandleAsync(IGenericEvent<int> @event, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}