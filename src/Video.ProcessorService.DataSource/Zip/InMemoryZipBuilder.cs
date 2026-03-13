using System.IO.Compression;
using Video.ProcessorService.Core.Gateways;

namespace Video.ProcessorService.DataSource.Zip;

public sealed class InMemoryZipBuilder : IZipBuilder
{
    public async Task<Stream> BuildAsync(
        IAsyncEnumerable<(string EntryName, Stream Content)> entries,
        CancellationToken ct = default)
    {
        var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            await foreach (var (name, content) in entries.WithCancellation(ct))
            {
                var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
                await using var es = entry.Open();
                await content.CopyToAsync(es, ct);
                await content.DisposeAsync();
            }
        }
        output.Position = 0;
        return output;
    }
}
