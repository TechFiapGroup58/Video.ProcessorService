namespace Video.ProcessorService.Messaging.Consumers;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string Host          { get; set; } = "localhost";
    public int    Port          { get; set; } = 5672;
    public string Username      { get; set; } = "guest";
    public string Password      { get; set; } = "guest";
    public string VirtualHost   { get; set; } = "/";
    public string ExchangeName  { get; set; } = "upload-service";
    public string QueueName     { get; set; } = "video-jobs";
    public string RoutingKey    { get; set; } = "video.job.created";
    public ushort PrefetchCount { get; set; } = 1;
}
