using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OTT.Lib.Metrics.Extensions;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler
{
    public class HttpHostedService : IHostedService
    {
        private readonly int _port;
        private readonly ILogger<HttpHostedService> _logger;
        private readonly IWebHost _host;
        private Task _hostTask;

        public HttpHostedService(int port, ILogger<HttpHostedService> logger)
        {
            _port = port;
            _logger = logger;
            _host = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://*:{_port}")
                .ConfigureAppConfiguration((ctx, builder) => builder.Sources.Clear())
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks();  // TODO real health check
                })
                .Configure(app =>
                {
                    app
                        .UseMetrics()
                        .UseHealthChecks("/health");
                })
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder
                        .ClearProviders()
                        .AddProvider(new KLoggerProvider())
                        .SetMinimumLevel(LogLevel.Warning);
                })
                .Build();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting http service {Port}", _port);
            _hostTask = _host.RunAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping http service {Port}", _port);
            if (_hostTask != null)
            {
                await _host.StopAsync(cancellationToken);
                await _host.WaitForShutdownAsync(cancellationToken);
            }
        }
    }
}