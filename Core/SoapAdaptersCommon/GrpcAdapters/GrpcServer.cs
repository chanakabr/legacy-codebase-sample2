using Phx.Lib.Log;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
        private const string GRPC_INSECURE_PORT_ENV_KEY = "GRPC_INSECURE_PORT";
        private const string GRPC_INSECURE_ENV_KEY = "GRPC_ALLOW_INSECURE";
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
            // using PFX
            var grpcCertFilePath = Environment.GetEnvironmentVariable(GRPC_CERT_ENV_KEY);
            var grpcCertPassword = Environment.GetEnvironmentVariable(GRPC_CERT_PASSWORD_ENV_KEY);
            var isPfxProvided = !string.IsNullOrEmpty(grpcCertFilePath) && !string.IsNullOrEmpty(grpcCertPassword);

            // using CRT and PEM
            var grpcSSLCertFilePath = Environment.GetEnvironmentVariable(GRPC_SSL_CRT_FILE_ENV);
            var grpcSSLCertKey = Environment.GetEnvironmentVariable(GRPC_SSL_CRT_KEY_ENV);
            var isCrtPemProvided = !string.IsNullOrEmpty(grpcSSLCertFilePath) && !string.IsNullOrEmpty(grpcSSLCertKey);

            var useSecure = isPfxProvided || isCrtPemProvided;

            var secureGrpcPort = Environment.GetEnvironmentVariable(GRPC_PORT_ENV_KEY);
            if (useSecure && string.IsNullOrEmpty(secureGrpcPort))
            {
                _Logger.Warn($"{GRPC_PORT_ENV_KEY} is empty");
                return Task.CompletedTask;
            }

            var insecureValue = Environment.GetEnvironmentVariable(GRPC_INSECURE_ENV_KEY);
            var useInsecure = insecureValue == "1" || "true".Equals(insecureValue, StringComparison.OrdinalIgnoreCase);

            var insecureGrpcPort = Environment.GetEnvironmentVariable(GRPC_INSECURE_PORT_ENV_KEY);
            if (useInsecure && string.IsNullOrEmpty(insecureGrpcPort))
            {
                _Logger.Warn($"{GRPC_INSECURE_PORT_ENV_KEY} is empty");
                return Task.CompletedTask;
            }

            if (!useInsecure && !useSecure)
            {
                _Logger.Warn($"insecure endpoint is not configured and SSL certificate is missing: please configure {GRPC_CERT_ENV_KEY}/{GRPC_CERT_PASSWORD_ENV_KEY} or {GRPC_SSL_CRT_FILE_ENV}/{GRPC_SSL_CRT_KEY_ENV} or {GRPC_INSECURE_ENV_KEY}, grpc server will not start.");
                return Task.CompletedTask;
            }

            var securePort = 0;
            if (useSecure && !int.TryParse(secureGrpcPort, out securePort))
            {
                _Logger.Error($"{GRPC_PORT_ENV_KEY}={secureGrpcPort} is not an integer, grpc server will not start.");
                return Task.CompletedTask;
            }

            var insecurePort = 0;
            if (useInsecure && !int.TryParse(insecureGrpcPort, out insecurePort))
            {
                _Logger.Error($"{GRPC_INSECURE_PORT_ENV_KEY}={insecureGrpcPort} is not an integer, grpc server will not start.");
                return Task.CompletedTask;
            }

            GrpcStartup.ParentServiceCollection = _ParentServices;
            _GrpcHost = WebHost.CreateDefaultBuilder()
                .UseUrls()
                .ConfigureAppConfiguration((ctx, builder) => builder.Sources.Clear())
                .ConfigureKestrel(o =>
                {
                    o.Limits.MinRequestBodyDataRate = null;
                    if (useSecure)
                    {
                        o.ListenAnyIP(securePort, listenOptions =>
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
                    }

                    if (useInsecure)
                    {
                        o.ListenAnyIP(insecurePort, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    }
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
