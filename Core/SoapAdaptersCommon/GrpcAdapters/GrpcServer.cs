using Phx.Lib.Log;
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
        private const string GRPC_SSL_CRT_FILE_ENV = "OTT_GRPC_SSL_CRT_FILE";
        private const string GRPC_SSL_CRT_KEY_ENV = "OTT_GRPC_SSL_CRT_KEY";
        private const string GRPC_CERT_PASSWORD_ENV_KEY = "GRPC_CERT_PASSWORD";

        public GrpcServer(IServiceCollection parentServiceCollection)
        {
            _ParentServices = parentServiceCollection;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var grpcPort = Environment.GetEnvironmentVariable(GRPC_PORT_ENV_KEY);
            if (string.IsNullOrEmpty(grpcPort))
            {
                _Logger.Warn($"{GRPC_PORT_ENV_KEY}");
                return Task.CompletedTask;
            }
            
            // using PFX
            var grpcCertFilePath = Environment.GetEnvironmentVariable(GRPC_CERT_ENV_KEY);
            var grpcCertPassword = Environment.GetEnvironmentVariable(GRPC_CERT_PASSWORD_ENV_KEY);
            var isPfxProvided = !string.IsNullOrEmpty(grpcCertFilePath) && !string.IsNullOrEmpty(grpcCertPassword);
            
            // using CRT and PEM
            var grpcSSLCertFilePath = Environment.GetEnvironmentVariable(GRPC_SSL_CRT_FILE_ENV);
            var grpcSSLCertKey = Environment.GetEnvironmentVariable(GRPC_SSL_CRT_KEY_ENV);
            var isCrtPemProvided = !string.IsNullOrEmpty(grpcSSLCertFilePath) && !string.IsNullOrEmpty(grpcSSLCertKey);
            
            
            if (!isPfxProvided && !isCrtPemProvided)
            {
                _Logger.Warn($"missing SSL certificate: please configure {GRPC_CERT_ENV_KEY}/{GRPC_CERT_PASSWORD_ENV_KEY} or {GRPC_SSL_CRT_FILE_ENV}/{GRPC_SSL_CRT_KEY_ENV} , grpc engpint will not start");
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
                    o.Limits.MinRequestBodyDataRate = null;
                    o.ListenAnyIP(port, listenOptions =>
                    {
                        if (isCrtPemProvided)
                        {
                            var cert = SSLHelpers.NewX509Certificate2FromCrtAndKey(grpcSSLCertFilePath, grpcSSLCertKey);
                            listenOptions.UseHttps(cert);
                        }
                        else if (isPfxProvided)
                        {
                            listenOptions.UseHttps(grpcCertFilePath, grpcCertPassword);
                        }
                        
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddProvider(new KLoggerProvider());
                })
                .UseStartup<GrpcStartup>()
                .Build();

            _GrpcServerTask = _GrpcHost.RunAsync();
            return Task.CompletedTask;
        }

        private void ConfigureSSL(ListenOptions listenOptions)
        {
            throw new NotImplementedException();
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
