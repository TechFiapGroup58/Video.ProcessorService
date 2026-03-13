using FluentAssertions;
using Video.ProcessorService.Core.Domain.ValueObjects;
using Xunit;

namespace Video.ProcessorService.UnitTests.Domain;

public sealed class StorageKeyTests
{
    [Fact]
    public void Constructor_ValidValue_SetsValue()
        => new StorageKey("videos/abc/test.mp4").Value.Should().Be("videos/abc/test.mp4");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidValue_Throws(string value)
    {
        Action act = () => _ = new StorageKey(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ForFrames_IncludesPaddedNumber()
    {
        var id  = Guid.NewGuid();
        var key = StorageKey.ForFrames(id, 3);
        key.Value.Should().Be($"frames/{id}/frame_0003.png");
    }

    [Fact]
    public void ForZip_IncludesJobId()
    {
        var id  = Guid.NewGuid();
        var key = StorageKey.ForZip(id);
        key.Value.Should().Be($"zips/{id}/frames.zip");
    }

    [Fact]
    public void Equality_SameValue_Equal()
        => new StorageKey("k").Should().Be(new StorageKey("k"));
}
