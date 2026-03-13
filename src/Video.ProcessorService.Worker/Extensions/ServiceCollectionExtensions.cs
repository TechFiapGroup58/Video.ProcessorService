using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Video.ProcessorService.Core.Domain.Services;
using Video.ProcessorService.Core.Gateways;
using Video.ProcessorService.DataSource;
using Video.ProcessorService.DataSource.FrameExtraction;
using Video.ProcessorService.DataSource.Repositories;
using Video.ProcessorService.DataSource.Storage;
using Video.ProcessorService.DataSource.Zip;
using Video.ProcessorService.Messaging.Consumers;

namespace Video.ProcessorService.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProcessorDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);
        return services;
    }

    public static IServiceCollection AddS3Storage(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(
            configuration.GetSection(S3StorageOptions.SectionName));
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var o = configuration
                .GetSection(S3StorageOptions.SectionName)
                .Get<S3StorageOptions>()!;
            return new AmazonS3Client(
                new BasicAWSCredentials(o.AccessKey, o.SecretKey),
                new AmazonS3Config { ServiceURL = o.ServiceUrl, ForcePathStyle = o.ForcePathStyle });
        });
        services.AddScoped<IVideoStorageGateway, S3VideoStorageGateway>();
        return services;
    }

    public static IServiceCollection AddFfmpeg(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FfmpegOptions>(
            configuration.GetSection(FfmpegOptions.SectionName));
        services.AddScoped<IFrameExtractor, FfmpegFrameExtractor>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IProcessingJobRepository, ProcessingJobRepository>();
        services.AddScoped<IZipBuilder, InMemoryZipBuilder>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();
        return services;
    }

    public static IServiceCollection AddMessaging(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddHostedService<VideoJobConsumer>();
        return services;
    }
}
