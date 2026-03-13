using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Domain.Exceptions;
using Video.ProcessorService.Core.Domain.ValueObjects;
using Video.ProcessorService.Core.Gateways;

namespace Video.ProcessorService.Core.Domain.Services;

public sealed class VideoProcessingService : IVideoProcessingService
{
    private readonly IProcessingJobRepository _repository;
    private readonly IVideoStorageGateway     _storage;
    private readonly IFrameExtractor          _extractor;
    private readonly IZipBuilder              _zipBuilder;

    public VideoProcessingService(
        IProcessingJobRepository repository,
        IVideoStorageGateway     storage,
        IFrameExtractor          extractor,
        IZipBuilder              zipBuilder)
    {
        _repository = repository;
        _storage    = storage;
        _extractor  = extractor;
        _zipBuilder = zipBuilder;
    }

    public async Task<ProcessingJob> ProcessAsync(
        Guid   uploadJobId,
        string ownerId,
        string videoStorageKey,
        string originalFileName,
        CancellationToken ct = default)
    {
        var existing = await _repository.FindByUploadJobIdAsync(uploadJobId, ct);
        if (existing is not null) return existing;

        var job = ProcessingJob.Create(
            uploadJobId, new OwnerId(ownerId),
            new StorageKey(videoStorageKey), originalFileName);

        await _repository.AddAsync(job, ct);
        job.MarkAsProcessing();
        await _repository.UpdateAsync(job, ct);

        try
        {
            await using var videoStream = await _storage.DownloadAsync(videoStorageKey, ct);
            var frameCount = 0;

            async IAsyncEnumerable<(string, Stream)> FrameEntries()
            {
                await foreach (var (n, s) in _extractor.ExtractFramesAsync(videoStream, ct))
                {
                    frameCount++;
                    yield return ($"frame_{n:D4}.png", s);
                }
            }

            await using var zipStream = await _zipBuilder.BuildAsync(FrameEntries(), ct);

            if (frameCount == 0)
                throw new FrameExtractionException("No frames were extracted from the video.");

            var zipKey = StorageKey.ForZip(job.Id).Value;
            await _storage.UploadAsync(zipKey, zipStream, "application/zip", ct);
            job.MarkAsCompleted(zipKey, frameCount);
            await _repository.UpdateAsync(job, ct);
        }
        catch (FrameExtractionException)
        {
            job.MarkAsFailed("Frame extraction failed.");
            await _repository.UpdateAsync(job, ct);
            throw;
        }
        catch (Exception ex)
        {
            job.MarkAsFailed(ex.Message);
            await _repository.UpdateAsync(job, ct);
            throw;
        }

        return job;
    }
}
