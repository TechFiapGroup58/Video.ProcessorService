using Microsoft.EntityFrameworkCore;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.DataSource.Configurations;

namespace Video.ProcessorService.DataSource;

public sealed class ProcessorDbContext : DbContext
{
    public DbSet<ProcessingJob> ProcessingJobs => Set<ProcessingJob>();

    public ProcessorDbContext(DbContextOptions<ProcessorDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProcessingJobConfiguration());
    }
}
