using Video.ProcessorService.Core.Domain.Entities;

namespace Video.ProcessorService.Core.Gateways;

public interface IVideoProcessingService
{
    Task<ProcessingJob> ProcessAsync(
        Guid   uploadJobId,
        string ownerId,
        string videoStorageKey,
        string originalFileName,
        CancellationToken ct = default);
}
