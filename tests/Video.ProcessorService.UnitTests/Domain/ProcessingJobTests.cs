using FluentAssertions;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Domain.Enums;
using Video.ProcessorService.Core.Domain.Exceptions;
using Video.ProcessorService.Core.Domain.ValueObjects;
using Xunit;

namespace Video.ProcessorService.UnitTests.Domain;

public sealed class ProcessingJobTests
{
    private static ProcessingJob Make() => ProcessingJob.Create(
        Guid.NewGuid(), new OwnerId("u"), new StorageKey("videos/v.mp4"), "v.mp4");

    [Fact]
    public void Create_ValidArgs_ReturnsPending()
    {
        var job = Make();
        job.Status.Should().Be(ProcessingStatus.Pending);
        job.Id.Should().NotBeEmpty();
        job.FrameCount.Should().Be(0);
        job.ZipStorageKey.Should().BeNull();
        job.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Create_NullOwnerId_Throws()
    {
        Action act = () => ProcessingJob.Create(Guid.NewGuid(), null!, new StorageKey("k"), "f");
        act.Should().Throw<ArgumentNullException>().WithParameterName("ownerId");
    }

    [Fact]
    public void Create_NullStorageKey_Throws()
    {
        Action act = () => ProcessingJob.Create(Guid.NewGuid(), new OwnerId("u"), null!, "f");
        act.Should().Throw<ArgumentNullException>().WithParameterName("videoStorageKey");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyFileName_Throws(string name)
    {
        Action act = () => ProcessingJob.Create(Guid.NewGuid(), new OwnerId("u"), new StorageKey("k"), name);
        act.Should().Throw<ArgumentException>().WithParameterName("originalFileName");
    }

    [Fact]
    public void MarkAsProcessing_FromPending_Transitions()
    {
        var job = Make();
        job.MarkAsProcessing();
        job.Status.Should().Be(ProcessingStatus.Processing);
    }

    [Fact]
    public void MarkAsProcessing_FromCompleted_Throws()
    {
        var job = Make();
        job.MarkAsProcessing();
        job.MarkAsCompleted("zips/x/frames.zip", 10);
        Action act = () => job.MarkAsProcessing();
        act.Should().Throw<InvalidProcessingJobStateException>();
    }

    [Fact]
    public void MarkAsCompleted_ValidArgs_SetsFields()
    {
        var job = Make();
        job.MarkAsProcessing();
        job.MarkAsCompleted("zips/x/frames.zip", 42);
        job.Status.Should().Be(ProcessingStatus.Completed);
        job.ZipStorageKey.Should().Be("zips/x/frames.zip");
        job.FrameCount.Should().Be(42);
    }

    [Fact]
    public void MarkAsCompleted_ZeroFrames_Throws()
    {
        var job = Make();
        job.MarkAsProcessing();
        Action act = () => job.MarkAsCompleted("zips/x/frames.zip", 0);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("frameCount");
    }

    [Fact]
    public void MarkAsCompleted_FromPending_Throws()
    {
        var job = Make();
        Action act = () => job.MarkAsCompleted("zips/x/frames.zip", 5);
        act.Should().Throw<InvalidProcessingJobStateException>();
    }

    [Fact]
    public void MarkAsFailed_SetsError()
    {
        var job = Make();
        job.MarkAsFailed("boom");
        job.Status.Should().Be(ProcessingStatus.Failed);
        job.ErrorMessage.Should().Be("boom");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkAsFailed_EmptyMessage_Throws(string msg)
    {
        var job = Make();
        Action act = () => job.MarkAsFailed(msg);
        act.Should().Throw<ArgumentException>().WithParameterName("errorMessage");
    }
}
