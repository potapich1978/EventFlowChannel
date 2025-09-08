using ChannelFactory.Abstract;
using NSubstitute;
using System.Threading.Channels;
using ChannelFactory;
using Xunit;

namespace EventChannelBuilder.tests;

/// <summary>
    /// Contains unit tests for the BoundedBuilder class.
    /// </summary>
    public class BoundedBuilderTests
    {
        private readonly IChannelFactory<int> _channelFactory;
        private readonly BoundedBuilder<int> _sut;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedBuilderTests"/> class.
        /// Sets up the test dependencies and creates the system under test.
        /// </summary>
        public BoundedBuilderTests()
        {
            _channelFactory = Substitute.For<IChannelFactory<int>>();
            _sut = new BoundedBuilder<int>(_channelFactory);
        }

        /// <summary>
        /// Tests that the Capacity method sets the capacity and returns the builder instance.
        /// Verifies that the fluent interface pattern is maintained.
        /// </summary>
        [Fact]
        public void Capacity_SetsCapacity_ReturnsBuilderInstance()
        {
            // Arrange
            var capacity = 100;

            // Act
            var result = _sut.WithCapacity(capacity);

            // Assert
            Assert.Same(_sut, result);
        }

        /// <summary>
        /// Tests that the WithReadersCount method sets the readers count and returns the builder instance.
        /// Verifies that the fluent interface pattern is maintained.
        /// </summary>
        [Fact]
        public void WithReadersCount_SetsReadersCount_ReturnsBuilderInstance()
        {
            // Arrange
            const int readersCount = 2;

            // Act
            var result = _sut.WithReadersCount(readersCount);

            // Assert
            Assert.Same(_sut, result);
        }

        /// <summary>
        /// Tests that the WithMultipleWriters method sets the writer mode and returns the builder instance.
        /// Verifies that the fluent interface pattern is maintained.
        /// </summary>
        [Fact]
        public void WithMultipleWriters_SetsWriterMode_ReturnsBuilderInstance()
        {
            // Act
            var result = _sut.WithMultipleWriters();

            // Assert
            Assert.Same(_sut, result);
        }

        /// <summary>
        /// Tests that the WithFullMode method sets the full mode and returns the builder instance.
        /// Verifies that the fluent interface pattern is maintained.
        /// </summary>
        [Fact]
        public void WithFullMode_SetsFullMode_ReturnsBuilderInstance()
        {
            // Arrange
            var fullMode = BoundedChannelFullMode.DropWrite;

            // Act
            var result = _sut.WithFullMode(fullMode);

            // Assert
            Assert.Same(_sut, result);
        }

        /// <summary>
        /// Tests that the Build method calls the factory to produce a channel.
        /// Verifies that the builder properly delegates channel creation to the factory.
        /// </summary>
        [Fact]
        public void Build_CallsFactoryToProduceChannel()
        {
            // Arrange
            var channel = Substitute.For<IChannel<int>>();
            _channelFactory.ProduceChannel(Arg.Any<BoundedOptions>()).Returns(channel);

            // Act
            var result = _sut.Build();

            // Assert
            Assert.Same(channel, result);
            _channelFactory.Received(1).ProduceChannel(Arg.Any<BoundedOptions>());
        }
    }