using System;
using System.IO;
using System.Threading;
using Core.Api;
using Core.Catalog;
using Core.GroupManagers;
using Microsoft.Extensions.Configuration;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace Epg.V3.Migrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Init Logger
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/elasticsearch_migrator/");
            var log = new KLogger("ESMigrator");
            log.Info("epg v3 migration is starting");

            // Build configuration
            var configRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables("OTT")
                .Build();

            var config = configRoot.Get<Configuration>();
            ApplicationConfiguration.Init();

            

            try
            {
                log.Info($"Starting epg v3 migration partner:[{config.PartnerId}].");
                var currentEpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(config.PartnerId);
                log.Info($"Detected current epg feature version:{currentEpgFeatureVersion}");
                if (currentEpgFeatureVersion == ApiObjects.EpgFeatureVersion.V3)
                {
                    log.Info("feature version is already v3, nothing to do...");
                    return;
                }

                var indexManager = IndexManagerFactory.Instance.GetIndexManager(config.PartnerId);
                indexManager.MigrateEpgToV3(config.BatchSize, currentEpgFeatureVersion);
                log.Info($"Migration completed...");
            }
            catch (Exception e)
            {
                log.Error("unexpected error: ", e);
            }
        }
    }
}
