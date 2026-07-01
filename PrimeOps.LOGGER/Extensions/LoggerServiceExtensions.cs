using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using PrimeOps.LOGGER.Configurations;
using PrimeOps.LOGGER.Entities;
using PrimeOps.LOGGER.Interfaces;
using PrimeOps.LOGGER.Services;
using PrimeOps.LOGGER.Static;

namespace PrimeOps.LOGGER.Extensions
{
    public static class LoggerServiceExtensions
    {
        public static IServiceCollection AddFileLogger (this IServiceCollection services, IConfiguration configuration, Action<LoggerOptions>? configure = null)
        {
            if (configure is not null)
                services.Configure<LoggerOptions>(configure);
            else
                services.Configure<LoggerOptions>(configuration.GetSection(LoggerOptions.SectionName));

            services.AddSingleton<LogWriterQueue>();

            services.AddSingleton<IFileLogger>(sp =>
                new FileLogger(sp.GetRequiredService<IOptions<LoggerOptions>>(),
                               sp.GetRequiredService<LogWriterQueue>(), LogLevel.Info));

            // Register named via keyed DI (.NET 8+)
            services.AddKeyedSingleton<IFileLogger>("Info", (sp, _) =>
                new FileLogger(sp.GetRequiredService<IOptions<LoggerOptions>>(),
                               sp.GetRequiredService<LogWriterQueue>(), LogLevel.Info));

            services.AddKeyedSingleton<IFileLogger>("Warn", (sp, _) =>
                new FileLogger(sp.GetRequiredService<IOptions<LoggerOptions>>(),
                               sp.GetRequiredService<LogWriterQueue>(), LogLevel.Warning));

            services.AddKeyedSingleton<IFileLogger>("Error", (sp, _) =>
                new FileLogger(sp.GetRequiredService<IOptions<LoggerOptions>>(),
                               sp.GetRequiredService<LogWriterQueue>(), LogLevel.Error));

            // Wire static façade after DI is built
            services.AddHostedService<LoggerInitializer>();

            return services;
        }
    }

    /// <summary>Bootstraps the static Log façade when the host starts.</summary>
    internal sealed class LoggerInitializer ([FromKeyedServices("Info")] IFileLogger info, [FromKeyedServices("Warn")] IFileLogger warn, [FromKeyedServices("Error")] IFileLogger error) : IHostedService
    {
        public Task StartAsync (CancellationToken _)
        {
            Log.Initialize(info, warn, error);
            return Task.CompletedTask;
        }

        public Task StopAsync (CancellationToken _) => Task.CompletedTask;
    }
}