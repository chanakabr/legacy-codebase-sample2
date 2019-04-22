using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager;
using FtpApiServer.InMemoryFtp;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;
using FubarDev.FtpServer.FileSystem.Generic;
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
            ApplicationConfiguration.Initialize();

            // Setup dependency injection
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddConsole());

            services.AddFtpServer(builder => builder
               .UseInMemoryFileSystem()
               .UseKalturaOttAuthentication());

            services.Configure<InMemoryFileSystemOptions>(o => o.OnFileUpload = OnFileUploaded);

            // Configure the FTP server
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = "127.0.0.1";
                opt.Port = 21;
            });

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