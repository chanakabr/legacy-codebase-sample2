using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager;
using FtpApiServer.Authentication;
using FtpApiServer.Helpers;
using FtpApiServer.InMemoryFtp;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;
using FubarDev.FtpServer.FileSystem.Generic;
using Kaltura;
using KLogMonitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
namespace FtpApiServer
{
    class Program
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void Main(string[] args)
        {
            InitLogger();
            EnableOutbountHttpsComunication();
            ApplicationConfiguration.Initialize();

            // Setup dependency injection
            var services = new ServiceCollection();
            services.Configure<InMemoryFileSystemOptions>(o => o.OnFileUpload = OnFileUploaded);
            services.Configure<FtpServerOptions>(opt =>  {
                opt.ServerAddress = ApplicationConfiguration.FtpApiServerConfiguration.FtpServerAddress.Value;
                opt.Port = ApplicationConfiguration.FtpApiServerConfiguration.FtpServerPort.IntValue;
            });

            services.AddScoped<Kaltura.Client>(GetCalturaClient);
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddConsole());
            services.AddFtpServer(builder => builder
               .UseInMemoryFileSystem()
               .UseKalturaOttAuthentication());

            // Configure the FTP server

            // Build the service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Initialize the FTP server
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServer>();
                // Start the FTP server
                ftpServerHost.Start();

                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                Console.ReadLine();

                // Stop the FTP server
                ftpServerHost.Stop();
            }
        }



        private static Kaltura.Client GetCalturaClient(IServiceProvider sp)
        {
            var clientConfig = new Kaltura.Configuration
            {
                Logger = new KalturaClientKloggerWrapper(),
                ServiceUrl = ApplicationConfiguration.FtpApiServerConfiguration.PhoenixServerUrl.Value,
            };
            var client = new Kaltura.Client(clientConfig);
            client.setClientTag($"Kaltura.FtpApiServer.{Assembly.GetExecutingAssembly().GetName().Version}");
            return client;
        }

        private static void EnableOutbountHttpsComunication()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, error) => true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.SystemDefault;
        }


        private static void OnFileUploaded(string folderName, string fileName, Stream fileDataStream)
        {
            _Logger.Info($"Sending bulkUpload to profilExternalId:[{folderName}], fileName:[{fileName}]");
        }

        // TODO: move this to a common Klogger function
        private static void InitLogger()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyVersion = $"{fvi.FileMajorPart}_{fvi.FileMinorPart}_{fvi.FileBuildPart}";
            var logDir = Environment.GetEnvironmentVariable("API_LOG_DIR");
            logDir = logDir != null ? Environment.ExpandEnvironmentVariables(logDir) : @"C:\log\EventHandlers\";
            log4net.GlobalContext.Properties["LogDir"] = logDir;
            log4net.GlobalContext.Properties["ApiVersion"] = assemblyVersion;
            log4net.GlobalContext.Properties["LogName"] = assembly.GetName().Name;

            KMonitor.Configure("log4net.config", KLogEnums.AppType.WindowsService);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WindowsService);

        }

    }
}