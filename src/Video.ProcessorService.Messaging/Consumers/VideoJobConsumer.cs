using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Video.ProcessorService.Core.Gateways;
using Video.ProcessorService.Messaging.Messages;

namespace Video.ProcessorService.Messaging.Consumers;

public sealed class VideoJobConsumer : BackgroundService
{
    private readonly IServiceScopeFactory      _scopeFactory;
    private readonly RabbitMqOptions           _opts;
    private readonly ILogger<VideoJobConsumer> _logger;
    private IConnection? _connection;
    private IModel?      _channel;

    public VideoJobConsumer(
        IServiceScopeFactory      scopeFactory,
        IOptions<RabbitMqOptions> opts,
        ILogger<VideoJobConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _opts         = opts.Value;
        _logger       = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(Disconnect);
        Connect();
        _channel!.BasicQos(0, _opts.PrefetchCount, false);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceivedAsync;
        _channel.BasicConsume(_opts.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("VideoJobConsumer listening on queue '{Queue}'", _opts.QueueName);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        VideoJobCreatedMessage? msg = null;
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.Span);
            msg = JsonConvert.DeserializeObject<VideoJobCreatedMessage>(json);
            if (msg is null)
            {
                _logger.LogWarning("Null or invalid message received. Dead-lettering.");
                _channel!.BasicNack(ea.DeliveryTag, false, false);
                return;
            }
            _logger.LogInformation("Processing VideoJob {Id}", msg.VideoJobId);
            await using var scope = _scopeFactory.CreateAsyncScope();
            var svc = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();
            await svc.ProcessAsync(msg.VideoJobId, msg.OwnerId, msg.StorageKey, msg.OriginalFileName);
            _channel!.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing VideoJob {Id}", msg?.VideoJobId);
            _channel!.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    private void Connect()
    {
        var factory = new ConnectionFactory
        {
            HostName                 = _opts.Host,
            Port                     = _opts.Port,
            UserName                 = _opts.Username,
            Password                 = _opts.Password,
            VirtualHost              = _opts.VirtualHost,
            DispatchConsumersAsync   = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval  = TimeSpan.FromSeconds(10)
        };
        _connection = factory.CreateConnection("video-processor-service");
        _channel    = _connection.CreateModel();
        _channel.ExchangeDeclare(_opts.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.QueueDeclare(
            _opts.QueueName, durable: true, exclusive: false, autoDelete: false,
            new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = _opts.ExchangeName + ".dlx"
            });
        _channel.QueueBind(_opts.QueueName, _opts.ExchangeName, _opts.RoutingKey);
    }

    private void Disconnect()
    {
        _channel?.Close();    _channel?.Dispose();
        _connection?.Close(); _connection?.Dispose();
    }

    public override void Dispose() { Disconnect(); base.Dispose(); }
}
