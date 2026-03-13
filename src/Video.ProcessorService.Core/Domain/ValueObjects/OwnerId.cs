namespace Video.ProcessorService.Core.Domain.ValueObjects;

public sealed record OwnerId
{
    public string Value { get; }

    public OwnerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("OwnerId cannot be empty.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
