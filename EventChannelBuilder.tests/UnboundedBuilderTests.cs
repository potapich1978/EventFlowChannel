using ChannelFactory.Abstract;
using NSubstitute;
using ChannelFactory;
using Xunit;

namespace EventChannelBuilder.tests;

/// <summary>
/// Contains unit tests for the UnboundedBuilder class.
/// </summary>
public class UnboundedBuilderTests
{
    private readonly IChannelFactory<int> _channelFactory;
    private readonly UnboundedBuilder<int> _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnboundedBuilderTests"/> class.
    /// Sets up the test dependencies and creates the system under test.
    /// </summary>
    public UnboundedBuilderTests()
    {
        _channelFactory = Substitute.For<IChannelFactory<int>>();
        _sut = new UnboundedBuilder<int>(_channelFactory);
    }

    /// <summary>
    /// Tests that the WithReadersCount method sets the readers count and returns the builder instance.
    /// Verifies that the fluent interface pattern is maintained.
    /// </summary>
    [Fact]
    public void WithReadersCount_SetsReadersCount_ReturnsBuilderInstance()
    {
        // Arrange
        var readersCount = 2;

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
    /// Tests that the Build method calls the factory to produce a channel.
    /// Verifies that the builder properly delegates channel creation to the factory.
    /// </summary>
    [Fact]
    public void Build_CallsFactoryToProduceChannel()
    {
        // Arrange
        var channel = Substitute.For<IChannel<int>>();
        _channelFactory.ProduceChannel(Arg.Any<UnboundedOptions>()).Returns(channel);

        // Act
        var result = _sut.Build();

        // Assert
        Assert.Same(channel, result);
        _channelFactory.Received(1).ProduceChannel(Arg.Any<UnboundedOptions>());
    }
}    
