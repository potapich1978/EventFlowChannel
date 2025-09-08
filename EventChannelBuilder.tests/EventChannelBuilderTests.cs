using ChannelFactory.Abstract;
using EventChannelBuilder.Abstract;
using NSubstitute;
using Xunit;

namespace EventChannelBuilder.tests;

    /// <summary>
    /// Contains unit tests for the EventChannelBuilder class.
    /// </summary>
    public class EventChannelBuilderTests
    {
        private readonly EventChannelBuilder<int> _sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventChannelBuilderTests"/> class.
        /// Sets up the test dependencies and creates the system under test.
        /// </summary>
        public EventChannelBuilderTests()
        {
            var channelFactory = Substitute.For<IChannelFactory<int>>();
            _sut = new EventChannelBuilder<int>(channelFactory);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when factory parameter is null.
        /// Verifies that the constructor validates input parameters.
        /// </summary>
        [Fact]
        public void Constructor_WithNullFactory_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EventChannelBuilder<int>(null));
        }

        /// <summary>
        /// Tests that the Bounded method returns an instance of IBoundedChannelBuilder.
        /// Verifies that the method creates the correct builder type.
        /// </summary>
        [Fact]
        public void Bounded_ReturnsBoundedChannelBuilderInstance()
        {
            // Act
            var result = _sut.Bounded();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IBoundedChannelBuilder<int>>(result);
        }

        /// <summary>
        /// Tests that the Unbounded method returns an instance of IUnboundedChannelBuilder.
        /// Verifies that the method creates the correct builder type.
        /// </summary>
        [Fact]
        public void Unbounded_ReturnsUnboundedChannelBuilderInstance()
        {
            // Act
            var result = _sut.Unbounded();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IUnboundedChannelBuilder<int>>(result);
        }
    }