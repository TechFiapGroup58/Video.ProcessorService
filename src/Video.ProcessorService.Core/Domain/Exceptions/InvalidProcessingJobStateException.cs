using Video.ProcessorService.Core.Domain.Enums;

namespace Video.ProcessorService.Core.Domain.Exceptions;

public sealed class InvalidProcessingJobStateException : Exception
{
    public InvalidProcessingJobStateException(
        Guid jobId, ProcessingStatus current, ProcessingStatus attempted)
        : base($"Cannot transition job '{jobId}' from '{current}' to '{attempted}'.") { }
}
