namespace Video.ProcessorService.Core.Domain.ValueObjects;

public sealed record StorageKey
{
    public string Value { get; }

    public StorageKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("StorageKey cannot be empty.", nameof(value));
        Value = value;
    }

    public static StorageKey ForFrames(Guid jobId, int frameNumber)
        => new($"frames/{jobId}/frame_{frameNumber:D4}.png");

    public static StorageKey ForZip(Guid jobId)
        => new($"zips/{jobId}/frames.zip");

    public override string ToString() => Value;
}
