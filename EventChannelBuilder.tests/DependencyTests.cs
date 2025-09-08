using EventChannelBuilder.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using EventLogger;
using Xunit;

namespace EventChannelBuilder.tests;

    /// <summary>
    /// Contains unit tests for the Dependency class.
    /// </summary>
    public class DependencyTests
    {
        /// <summary>
        /// Tests that AddEventChannelBuilder registers IChannelBuilder as a singleton service.
        /// Verifies that the service is registered with the correct lifetime and implementation type.
        /// </summary>
        [Fact]
        public void AddEventChannelBuilder_RegistersIChannelBuilderAsSingleton()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var services = new ServiceCollection();
            services.AddSingleton(logger);
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = services.AddEventChannelBuilder<int>(assembly);

            // Assert
            Assert.Same(services, result);
            
            var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IChannelBuilder<int>));
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
            Assert.Equal(typeof(EventChannelBuilder<int>), serviceDescriptor.ImplementationType);
        }

        /// <summary>
        /// Tests that AddEventChannelBuilder returns the same service collection instance.
        /// </summary>
        [Fact]
        public void AddEventChannelBuilder_ReturnsSameServiceCollection()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var services = new ServiceCollection();
            services.AddSingleton(logger);
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = services.AddEventChannelBuilder<int>(assembly);

            // Assert
            Assert.Same(services, result);
        }

        /// <summary>
        /// Tests that AddEventChannelBuilder can resolve the registered IChannelBuilder service.
        /// Verifies that the service registration is valid and can be used for dependency injection.
        /// </summary>
        [Fact]
        public void AddEventChannelBuilder_CanResolveIChannelBuilder()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var services = new ServiceCollection();
            services.AddSingleton(logger);
            var assembly = Assembly.GetExecutingAssembly();
            services.AddEventChannelBuilder<int>(assembly);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var channelBuilder = serviceProvider.GetService<IChannelBuilder<int>>();

            // Assert
            Assert.NotNull(channelBuilder);
            Assert.IsType<EventChannelBuilder<int>>(channelBuilder);
        }
    }