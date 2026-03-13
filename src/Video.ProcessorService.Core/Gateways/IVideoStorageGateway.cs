namespace Video.ProcessorService.Core.Gateways;

public interface IVideoStorageGateway
{
    Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default);
    Task<string> UploadAsync(string storageKey, Stream content, string contentType, CancellationToken ct = default);
}
