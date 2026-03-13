namespace Video.ProcessorService.Core.Domain.Exceptions;

public sealed class FrameExtractionException : Exception
{
    public FrameExtractionException(string message) : base(message) { }
    public FrameExtractionException(string message, Exception inner) : base(message, inner) { }
}
