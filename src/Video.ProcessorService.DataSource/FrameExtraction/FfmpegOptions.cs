namespace Video.ProcessorService.DataSource.FrameExtraction;

public sealed class FfmpegOptions
{
    public const string SectionName = "Ffmpeg";
    public string ExecutablePath { get; set; } = "ffmpeg";
}
