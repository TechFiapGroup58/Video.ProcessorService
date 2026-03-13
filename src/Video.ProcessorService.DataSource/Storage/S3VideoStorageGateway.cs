using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Video.ProcessorService.Core.Gateways;

namespace Video.ProcessorService.DataSource.Storage;

public sealed class S3VideoStorageGateway : IVideoStorageGateway
{
    private readonly IAmazonS3        _s3;
    private readonly S3StorageOptions _opts;

    public S3VideoStorageGateway(IAmazonS3 s3, IOptions<S3StorageOptions> opts)
    { _s3 = s3; _opts = opts.Value; }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var resp = await _s3.GetObjectAsync(
            new GetObjectRequest { BucketName = _opts.BucketName, Key = storageKey }, ct);
        return resp.ResponseStream;
    }

    public async Task<string> UploadAsync(
        string storageKey, Stream content, string contentType, CancellationToken ct = default)
    {
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName      = _opts.BucketName,
            Key             = storageKey,
            InputStream     = content,
            ContentType     = contentType,
            AutoCloseStream = false
        }, ct);
        return storageKey;
    }
}
