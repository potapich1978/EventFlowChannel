using ChannelReader.Abstract;
using NSubstitute;
using System.Threading.Channels;

namespace ChannelFactory.Tests;

/// <summary>
/// Contains unit tests for the UnboundedChannelReader class.
/// </summary>
public class UnboundedChannelReaderTests
{
    /// <summary>
    /// Tests that ProduceChannel creates an unbounded channel with correct options.
    /// Verifies that the channel is properly configured based on the provided options.
    /// </summary>
    [Fact]
    public void ProduceChannel_WithUnboundedOptions_ReturnsUnboundedChannel()
    {
        // Arrange
        var options = new UnboundedOptions
        {
            SingleWriter = true,
            ReadersCount = 1
        };
        var reader = Substitute.For<IChannelReader<int>>();
        var sut = new UnboundedChannelReader<int>(options, reader);

        // Act
        var channel = sut.ProduceChannel(options);

        // Assert
        Assert.NotNull(channel);
    }

    /// <summary>
    /// Tests that ProduceChannel returns default when options are not UnboundedOptions.
    /// Verifies that the method handles incorrect option types gracefully.
    /// </summary>
    [Fact]
    public void ProduceChannel_WithNonUnboundedOptions_ReturnsDefault()
    {
        // Arrange
        var options = new BoundedOptions
        {
            Capacity = 10,
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            ReadersCount = 1
        };
        var reader = Substitute.For<IChannelReader<int>>();
        var sut = new UnboundedChannelReader<int>(options, reader);

        // Act
        var channel = sut.ProduceChannel(options);

        // Assert
        Assert.Null(channel);
    }
}