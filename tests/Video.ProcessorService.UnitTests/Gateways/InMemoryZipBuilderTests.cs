using FluentAssertions;
using System.IO.Compression;
using Video.ProcessorService.DataSource.Zip;
using Xunit;

namespace Video.ProcessorService.UnitTests.Gateways;

public sealed class InMemoryZipBuilderTests
{
    private readonly InMemoryZipBuilder _sut = new();

    [Fact]
    public async Task BuildAsync_WithEntries_ValidZip()
    {
        async IAsyncEnumerable<(string, Stream)> Entries()
        {
            yield return ("frame_0001.png", new MemoryStream(new byte[] { 1, 2, 3 }));
            yield return ("frame_0002.png", new MemoryStream(new byte[] { 4, 5, 6 }));
            await Task.CompletedTask;
        }
        var result = await _sut.BuildAsync(Entries());
        result.Length.Should().BeGreaterThan(0);
        result.Position = 0;
        using var archive = new ZipArchive(result, ZipArchiveMode.Read);
        archive.Entries.Should().HaveCount(2);
        archive.Entries.Select(e => e.Name).Should().Contain("frame_0001.png", "frame_0002.png");
    }

    [Fact]
    public async Task BuildAsync_NoEntries_EmptyZip()
    {
        async IAsyncEnumerable<(string, Stream)> Empty()
        { await Task.CompletedTask; yield break; }
        var result = await _sut.BuildAsync(Empty());
        result.Position = 0;
        using var archive = new ZipArchive(result, ZipArchiveMode.Read);
        archive.Entries.Should().BeEmpty();
    }
}
