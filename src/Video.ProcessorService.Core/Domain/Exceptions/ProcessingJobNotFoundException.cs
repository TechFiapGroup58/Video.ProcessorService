namespace Video.ProcessorService.Core.Domain.Exceptions;

public sealed class ProcessingJobNotFoundException : Exception
{
    public ProcessingJobNotFoundException(Guid jobId)
        : base($"ProcessingJob '{jobId}' was not found.") { }
}
