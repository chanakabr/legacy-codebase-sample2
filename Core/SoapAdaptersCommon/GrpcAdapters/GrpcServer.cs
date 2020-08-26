using KLogMonitor;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using SoapAdaptersCommon.GrpcAdapters.Implementation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SoapAdaptersCommon.Middleware;
using System.Reflection;

namespace SoapAdaptersCommon.GrpcAdapters
{
    public class GrpcServer : Microsoft.Extensions.Hosting.IHostedService
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private Task _GrpcServerTask;
        private IWebHost _GrpcHost;
        private IServiceCollection _ParentServices;

        private const string GRPC_PORT_ENV_KEY = "GRPC_PORT";
        private const string GRPC_CERT_ENV_KEY = "GRPC_CERT";
        private const string GRPC_CERT_PASSWORD_ENV_KEY = "GRPC_CERT_PASSWORD";

        public GrpcServer(IServiceCollection parentServiceCollection)
        {
            _ParentServices = parentServiceCollection;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var grpcPort = Environment.GetEnvironmentVariable(GRPC_PORT_ENV_KEY);
            var grpcCertFilePath = Environment.GetEnvironmentVariable(GRPC_CERT_ENV_KEY);
            var grpcCertPassword = Environment.GetEnvironmentVariable(GRPC_CERT_PASSWORD_ENV_KEY);
            if (string.IsNullOrEmpty(grpcPort) || string.IsNullOrEmpty(grpcCertFilePath) || string.IsNullOrEmpty(grpcCertPassword))
            {
                _Logger.Warn($"{GRPC_PORT_ENV_KEY} or {GRPC_CERT_ENV_KEY} or {GRPC_CERT_PASSWORD_ENV_KEY} is not specifed, grpc engpint will not start");
                return Task.CompletedTask;
            }
            int intPort;
            if (!int.TryParse(grpcPort, out intPort))
            {
                _Logger.Error($"{GRPC_PORT_ENV_KEY}={grpcPort} is not an integer, grpc server will not start.");
                return Task.CompletedTask;
            }


            GrpcStartup.ParentServiceCollection = _ParentServices;
            _GrpcHost = WebHost.CreateDefaultBuilder()
                .UseUrls()
                .ConfigureAppConfiguration((ctx, builder) => builder.Sources.Clear())
                .ConfigureKestrel(o =>
                {
                    var port = intPort;
                    var certFilePath = grpcCertFilePath;
                    var certPassword = grpcCertPassword;
                    o.Limits.MinRequestBodyDataRate = null;
                    o.ListenAnyIP(port, listenOptions =>
                    {
                        listenOptions.UseHttps(certFilePath, certPassword);
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new KLoggerProvider());
                })
                .UseStartup<GrpcStartup>()
                .Build();

            _GrpcServerTask = _GrpcHost.RunAsync();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_GrpcHost != null)
            {
                await _GrpcHost.StopAsync();
                await _GrpcHost.WaitForShutdownAsync();
            }
        }
    }
}