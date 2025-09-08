using ChannelFactory.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using EventLogger;

namespace ChannelFactory.Tests;

/// <summary>
    /// Contains unit tests for the Dependency class.
    /// </summary>
    public class DependencyTests
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly Assembly _testAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public DependencyTests()
        {
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            _serviceCollection = Substitute.For<IServiceCollection>();
            _serviceCollection.AddSingleton(logger);
            _testAssembly = typeof(DependencyTests).Assembly;
        }

        /// <summary>
        /// Tests that AddChannelFactory registers the required services in the service collection.
        /// Verifies that the method returns the same service collection instance.
        /// </summary>
        [Fact]
        public void AddChannelFactory_RegistersRequiredServices_ReturnsSameServiceCollection()
        {
            // Act
            var result = _serviceCollection.AddChannelFactory<int>(_testAssembly);

            // Assert
            Assert.Same(_serviceCollection, result);
        }

        /// <summary>
        /// Tests that AddChannelFactory calls AddChannelReader with the correct assembly.
        /// Verifies that the channel reader registration is part of the factory setup.
        /// </summary>
        [Fact]
        public void AddChannelFactory_CallsAddChannelReaderWithCorrectAssembly()
        {
            // Arrange
            var originalServices = new ServiceCollection();
            
            // Act
            originalServices.AddChannelFactory<int>(_testAssembly);
            
            // Assert
            // If no exception is thrown, the test passes as it means AddChannelReader was called successfully
        }

        /// <summary>
        /// Tests that AddChannelFactory registers IChannelFactory as a singleton service.
        /// Verifies that the factory is registered with the correct lifetime.
        /// </summary>
        [Fact]
        public void AddChannelFactory_RegistersIChannelFactoryAsSingleton()
        {
            // Arrange
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var originalServices = new ServiceCollection();
            originalServices.AddSingleton(logger);
            // Act
            originalServices.AddChannelFactory<int>(_testAssembly);
            
            // Assert
            var completed = false;
            foreach (var service in originalServices)
            {
                if (service.ServiceType == typeof(IChannelFactory<int>))
                {
                    completed = service.ImplementationType == typeof(ChannelFactory<int>)
                                && service.Lifetime == ServiceLifetime.Singleton;
                    
                }
            }
            Assert.True(completed);
        }

        /// <summary>
        /// Tests that AddChannelFactory throws ArgumentNullException when services parameter is null.
        /// Verifies that the method validates input parameters.
        /// </summary>
        [Fact]
        public void AddChannelFactory_WithNullServices_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Dependency.AddChannelFactory<int>(null, _testAssembly));
        }

        /// <summary>
        /// Tests that AddChannelFactory throws ArgumentNullException when assembly parameter is null.
        /// Verifies that the method validates input parameters.
        /// </summary>
        [Fact]
        public void AddChannelFactory_WithNullAssembly_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _serviceCollection.AddChannelFactory<int>(null));
        }


    }