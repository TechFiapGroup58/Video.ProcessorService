using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Video.ProcessorService.Core.Domain.Exceptions;
using Video.ProcessorService.Core.Gateways;

namespace Video.ProcessorService.DataSource.FrameExtraction;

public sealed class FfmpegFrameExtractor : IFrameExtractor
{
    private readonly FfmpegOptions _opts;
    public FfmpegFrameExtractor(IOptions<FfmpegOptions> opts) => _opts = opts.Value;

    public async IAsyncEnumerable<(int FrameNumber, Stream FrameStream)> ExtractFramesAsync(
        Stream videoStream,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var tempInput = Path.GetTempFileName() + ".video";
        var tempDir   = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            Directory.CreateDirectory(tempDir);
            await using (var fs = File.Create(tempInput))
                await videoStream.CopyToAsync(fs, ct);

            var pattern = Path.Combine(tempDir, "frame_%04d.png");
            await RunFfmpegAsync(tempInput, pattern, ct);

            var frames = Directory.GetFiles(tempDir, "*.png").OrderBy(x => x).ToList();
            for (var i = 0; i < frames.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var bytes = await File.ReadAllBytesAsync(frames[i], ct);
                yield return (i + 1, new MemoryStream(bytes));
            }
        }
        finally
        {
            if (File.Exists(tempInput))    File.Delete(tempInput);
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }

    private async Task RunFfmpegAsync(string input, string pattern, CancellationToken ct)
    {
        var args = "-i \"" + input + "\" -vf fps=1 -y \"" + pattern + "\"";
        using var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName              = _opts.ExecutablePath,
                Arguments             = args,
                RedirectStandardError = true,
                UseShellExecute       = false,
                CreateNoWindow        = true
            }
        };
        proc.Start();
        var stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        if (proc.ExitCode != 0)
            throw new FrameExtractionException(
                "FFmpeg exited with code " + proc.ExitCode + ": " + stderr);
    }
}
