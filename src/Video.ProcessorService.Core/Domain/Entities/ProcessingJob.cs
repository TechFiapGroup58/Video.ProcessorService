using Video.ProcessorService.Core.Domain.Enums;
using Video.ProcessorService.Core.Domain.Exceptions;
using Video.ProcessorService.Core.Domain.ValueObjects;

namespace Video.ProcessorService.Core.Domain.Entities;

public sealed class ProcessingJob
{
    public Guid   Id               { get; private set; }
    public Guid   UploadJobId      { get; private set; }
    public OwnerId OwnerId         { get; private set; }
    public StorageKey VideoStorageKey { get; private set; }
    public string OriginalFileName { get; private set; }
    public ProcessingStatus Status { get; private set; }
    public string? ZipStorageKey   { get; private set; }
    public int    FrameCount       { get; private set; }
    public string? ErrorMessage    { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ProcessingJob()
    {
        OwnerId          = null!;
        VideoStorageKey  = null!;
        OriginalFileName = null!;
    }

    public static ProcessingJob Create(
        Guid uploadJobId,
        OwnerId ownerId,
        StorageKey videoStorageKey,
        string originalFileName)
    {
        if (ownerId is null)         throw new ArgumentNullException(nameof(ownerId));
        if (videoStorageKey is null) throw new ArgumentNullException(nameof(videoStorageKey));
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("File name cannot be empty.", nameof(originalFileName));
        var now = DateTimeOffset.UtcNow;
        return new ProcessingJob
        {
            Id               = Guid.NewGuid(),
            UploadJobId      = uploadJobId,
            OwnerId          = ownerId,
            VideoStorageKey  = videoStorageKey,
            OriginalFileName = originalFileName,
            Status           = ProcessingStatus.Pending,
            CreatedAt        = now,
            UpdatedAt        = now
        };
    }

    public void MarkAsProcessing()
    {
        if (Status != ProcessingStatus.Pending)
            throw new InvalidProcessingJobStateException(Id, Status, ProcessingStatus.Processing);
        Status    = ProcessingStatus.Processing;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsCompleted(string zipStorageKey, int frameCount)
    {
        if (Status != ProcessingStatus.Processing)
            throw new InvalidProcessingJobStateException(Id, Status, ProcessingStatus.Completed);
        if (string.IsNullOrWhiteSpace(zipStorageKey))
            throw new ArgumentException("Zip key cannot be empty.", nameof(zipStorageKey));
        if (frameCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameCount), "Must be greater than zero.");
        Status        = ProcessingStatus.Completed;
        ZipStorageKey = zipStorageKey;
        FrameCount    = frameCount;
        UpdatedAt     = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));
        Status       = ProcessingStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt    = DateTimeOffset.UtcNow;
    }
}
