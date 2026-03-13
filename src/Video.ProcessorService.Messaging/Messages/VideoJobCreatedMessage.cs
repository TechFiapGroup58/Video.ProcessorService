namespace Video.ProcessorService.Messaging.Messages;

public sealed record VideoJobCreatedMessage
{
    public Guid           VideoJobId       { get; init; }
    public string         OwnerId          { get; init; } = string.Empty;
    public string         StorageKey       { get; init; } = string.Empty;
    public string         OriginalFileName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt        { get; init; }
}
