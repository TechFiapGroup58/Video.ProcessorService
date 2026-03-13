using Microsoft.EntityFrameworkCore;
using Video.ProcessorService.DataSource;
using Video.ProcessorService.Worker.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;
        services
            .AddDatabase(cfg)
            .AddS3Storage(cfg)
            .AddFfmpeg(cfg)
            .AddApplicationServices()
            .AddMessaging(cfg);
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProcessorDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();

public partial class Program { }
