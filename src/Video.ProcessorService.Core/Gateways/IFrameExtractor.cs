namespace Video.ProcessorService.Core.Gateways;

public interface IFrameExtractor
{
    IAsyncEnumerable<(int FrameNumber, Stream FrameStream)> ExtractFramesAsync(
        Stream videoStream,
        CancellationToken ct = default);
}
