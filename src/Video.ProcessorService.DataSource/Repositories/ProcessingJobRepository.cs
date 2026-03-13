using Microsoft.EntityFrameworkCore;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Gateways;

namespace Video.ProcessorService.DataSource.Repositories;

public sealed class ProcessingJobRepository : IProcessingJobRepository
{
    private readonly ProcessorDbContext _db;
    public ProcessingJobRepository(ProcessorDbContext db) => _db = db;

    public async Task AddAsync(ProcessingJob job, CancellationToken ct = default)
    {
        await _db.ProcessingJobs.AddAsync(job, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ProcessingJob?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.ProcessingJobs.AsNoTracking()
               .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<ProcessingJob?> FindByUploadJobIdAsync(Guid uploadJobId, CancellationToken ct = default)
        => await _db.ProcessingJobs.AsNoTracking()
               .FirstOrDefaultAsync(x => x.UploadJobId == uploadJobId, ct);

    public async Task UpdateAsync(ProcessingJob job, CancellationToken ct = default)
    {
        _db.ProcessingJobs.Update(job);
        await _db.SaveChangesAsync(ct);
    }
}
