using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Domain.Enums;
using Video.ProcessorService.Core.Domain.ValueObjects;
using Video.ProcessorService.DataSource;
using Video.ProcessorService.DataSource.Repositories;
using Xunit;

namespace Video.ProcessorService.IntegrationTests;

public sealed class ProcessingJobRepositoryTests : IDisposable
{
    private readonly ProcessorDbContext      _db;
    private readonly ProcessingJobRepository _sut;

    public ProcessingJobRepositoryTests()
    {
        var opts = new DbContextOptionsBuilder<ProcessorDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        _db  = new ProcessorDbContext(opts);
        _sut = new ProcessingJobRepository(_db);
    }

    [Fact]
    public async Task AddThenFindById_ReturnsJob()
    {
        var job = Make();
        await _sut.AddAsync(job);
        var found = await _sut.FindByIdAsync(job.Id);
        found.Should().NotBeNull();
        found!.OriginalFileName.Should().Be(job.OriginalFileName);
    }

    [Fact]
    public async Task FindByUploadJobId_ReturnsJob()
    {
        var job = Make();
        await _sut.AddAsync(job);
        var found = await _sut.FindByUploadJobIdAsync(job.UploadJobId);
        found.Should().NotBeNull();
        found!.Id.Should().Be(job.Id);
    }

    [Fact]
    public async Task FindById_NotFound_ReturnsNull()
        => (await _sut.FindByIdAsync(Guid.NewGuid())).Should().BeNull();

    [Fact]
    public async Task Update_PersistsStatusChange()
    {
        var job = Make();
        await _sut.AddAsync(job);
        job.MarkAsProcessing();
        await _sut.UpdateAsync(job);
        var updated = await _db.ProcessingJobs.FindAsync(job.Id);
        updated!.Status.Should().Be(ProcessingStatus.Processing);
    }

    private static ProcessingJob Make() => ProcessingJob.Create(
        Guid.NewGuid(), new OwnerId("user-test"),
        new StorageKey("videos/test/v.mp4"), "v.mp4");

    public void Dispose() => _db.Dispose();
}
