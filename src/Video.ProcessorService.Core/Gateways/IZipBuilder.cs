namespace Video.ProcessorService.Core.Gateways;

public interface IZipBuilder
{
    Task<Stream> BuildAsync(
        IAsyncEnumerable<(string EntryName, Stream Content)> entries,
        CancellationToken ct = default);
}
