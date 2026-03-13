using FluentAssertions;
using Moq;
using System.Runtime.CompilerServices;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Domain.Exceptions;
using Video.ProcessorService.Core.Domain.Services;
using Video.ProcessorService.Core.Domain.ValueObjects;
using Video.ProcessorService.Core.Gateways;
using Xunit;

namespace Video.ProcessorService.UnitTests.Gateways;

public sealed class VideoProcessingServiceTests
{
    private readonly Mock<IProcessingJobRepository> _repo      = new();
    private readonly Mock<IVideoStorageGateway>     _storage   = new();
    private readonly Mock<IFrameExtractor>          _extractor = new();
    private readonly Mock<IZipBuilder>              _zip       = new();
    private readonly VideoProcessingService         _sut;

    public VideoProcessingServiceTests()
        => _sut = new VideoProcessingService(
               _repo.Object, _storage.Object, _extractor.Object, _zip.Object);

    [Fact]
    public async Task ProcessAsync_ValidInput_CompletesAndUploadsZip()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.FindByUploadJobIdAsync(id, default))
             .ReturnsAsync((ProcessingJob?)null);
        _storage.Setup(s => s.DownloadAsync("k", default))
                .ReturnsAsync(new MemoryStream(new byte[] { 1 }));
        _extractor.Setup(e => e.ExtractFramesAsync(It.IsAny<Stream>(), default))
                  .Returns(FakeFrames(3));
        _zip.Setup(z => z.BuildAsync(It.IsAny<IAsyncEnumerable<(string, Stream)>>(), default))
            .ReturnsAsync(new MemoryStream(new byte[] { 0x50, 0x4B }));
        _storage.Setup(s => s.UploadAsync(
                    It.IsAny<string>(), It.IsAny<Stream>(), "application/zip", default))
                .ReturnsAsync("zips/x/frames.zip");

        var result = await _sut.ProcessAsync(id, "u", "k", "v.mp4");

        result.Should().NotBeNull();
        _repo.Verify(r => r.AddAsync(It.IsAny<ProcessingJob>(), default), Times.Once);
        _storage.Verify(s => s.UploadAsync(
            It.IsAny<string>(), It.IsAny<Stream>(), "application/zip", default), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ExistingJob_ReturnsWithoutProcessing()
    {
        var id  = Guid.NewGuid();
        var job = ProcessingJob.Create(id, new OwnerId("u"), new StorageKey("k"), "f.mp4");
        _repo.Setup(r => r.FindByUploadJobIdAsync(id, default)).ReturnsAsync(job);

        var result = await _sut.ProcessAsync(id, "u", "k", "f.mp4");

        result.Should().Be(job);
        _repo.Verify(r => r.AddAsync(It.IsAny<ProcessingJob>(), default), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_NoFrames_ThrowsFrameExtractionException()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.FindByUploadJobIdAsync(id, default))
             .ReturnsAsync((ProcessingJob?)null);
        _storage.Setup(s => s.DownloadAsync(It.IsAny<string>(), default))
                .ReturnsAsync(new MemoryStream());
        _extractor.Setup(e => e.ExtractFramesAsync(It.IsAny<Stream>(), default))
                  .Returns(FakeFrames(0));
        _zip.Setup(z => z.BuildAsync(It.IsAny<IAsyncEnumerable<(string, Stream)>>(), default))
            .ReturnsAsync(new MemoryStream());

        Func<Task> act = () => _sut.ProcessAsync(id, "u", "k", "f.mp4");
        await act.Should().ThrowAsync<FrameExtractionException>();
    }

    private static async IAsyncEnumerable<(int, Stream)> FakeFrames(
        int count, [EnumeratorCancellation] CancellationToken _ = default)
    {
        for (var i = 1; i <= count; i++)
            yield return (i, new MemoryStream(new byte[] { (byte)i }));
        await Task.CompletedTask;
    }
}
