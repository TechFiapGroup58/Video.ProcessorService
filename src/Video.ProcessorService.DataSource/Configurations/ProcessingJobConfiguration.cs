using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Video.ProcessorService.Core.Domain.Entities;
using Video.ProcessorService.Core.Domain.ValueObjects;

namespace Video.ProcessorService.DataSource.Configurations;

internal sealed class ProcessingJobConfiguration : IEntityTypeConfiguration<ProcessingJob>
{
    public void Configure(EntityTypeBuilder<ProcessingJob> builder)
    {
        builder.ToTable("processing_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.UploadJobId).HasColumnName("upload_job_id").IsRequired();
        builder.Property(x => x.OwnerId)
               .HasColumnName("owner_id")
               .HasConversion(v => v.Value, v => new OwnerId(v))
               .HasMaxLength(256).IsRequired();
        builder.Property(x => x.VideoStorageKey)
               .HasColumnName("video_storage_key")
               .HasConversion(v => v.Value, v => new StorageKey(v))
               .HasMaxLength(1024).IsRequired();
        builder.Property(x => x.OriginalFileName)
               .HasColumnName("original_file_name").HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status)
               .HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.ZipStorageKey).HasColumnName("zip_storage_key").HasMaxLength(1024);
        builder.Property(x => x.FrameCount).HasColumnName("frame_count");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(2048);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.HasIndex(x => x.UploadJobId).IsUnique()
               .HasDatabaseName("ix_processing_jobs_upload_job_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_processing_jobs_status");
    }
}
