using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using NSubstitute;

namespace ChannelFactory.Tests;

    /// <summary>
    /// Contains unit tests for the ChannelFactory class.
    /// </summary>
    public class ChannelFactoryTests
    {
        private readonly ChannelFactory<int> _sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelFactoryTests"/> class.
        /// Sets up the test dependencies and creates the system under test.
        /// </summary>
        public ChannelFactoryTests()
        {
            var channelReader = Substitute.For<IChannelReader<int>>();
            _sut = new ChannelFactory<int>(channelReader);
        }

        /// <summary>
        /// Tests that ProduceChannel returns a BoundedChannelReader when provided with BoundedOptions.
        /// Verifies that the factory creates the correct channel type based on the options.
        /// </summary>
        [Fact]
        public void ProduceChannel_WithBoundedOptions_ReturnsBoundedChannelReader()
        {
            // Arrange
            var options = new BoundedOptions
            {
                Capacity = 10,
                FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait,
                SingleWriter = true,
                ReadersCount = 1
            };

            // Act
            var result = _sut.ProduceChannel(options);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BoundedChannelReader<int>>(result);
        }

        /// <summary>
        /// Tests that ProduceChannel returns an UnboundedChannelReader when provided with UnboundedOptions.
        /// Verifies that the factory creates the correct channel type based on the options.
        /// </summary>
        [Fact]
        public void ProduceChannel_WithUnboundedOptions_ReturnsUnboundedChannelReader()
        {
            // Arrange
            var options = new UnboundedOptions
            {
                SingleWriter = true,
                ReadersCount = 1
            };

            // Act
            var result = _sut.ProduceChannel(options);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UnboundedChannelReader<int>>(result);
        }

        /// <summary>
        /// Tests that ProduceChannel throws an ArgumentException when provided with unsupported options.
        /// Verifies that the factory properly validates input options.
        /// </summary>
        [Fact]
        public void ProduceChannel_WithUnsupportedOptions_ThrowsArgumentException()
        {
            // Arrange
            var unsupportedOptions = Substitute.For<IChannelOptions>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _sut.ProduceChannel(unsupportedOptions));
            Assert.Contains("unsupported implementation of IChannelOptions", exception.Message);
        }

        /// <summary>
        /// Tests that ProduceChannel throws an ArgumentNullException when provided with null options.
        /// Verifies that the factory properly validates input parameters.
        /// </summary>
        [Fact]
        public void ProduceChannel_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _sut.ProduceChannel(null));
        }
   }