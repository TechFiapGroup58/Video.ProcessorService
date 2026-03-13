using Video.ProcessorService.Core.Domain.Entities;

namespace Video.ProcessorService.Core.Gateways;

public interface IProcessingJobRepository
{
    Task AddAsync(ProcessingJob job, CancellationToken ct = default);
    Task<ProcessingJob?> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProcessingJob?> FindByUploadJobIdAsync(Guid uploadJobId, CancellationToken ct = default);
    Task UpdateAsync(ProcessingJob job, CancellationToken ct = default);
}
