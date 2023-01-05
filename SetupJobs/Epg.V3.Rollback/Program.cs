using System;
using System.IO;
using ApiObjects;
using Core.Catalog;
using Core.GroupManagers;
using Microsoft.Extensions.Configuration;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace Epg.V3.Rollback
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Init Logger
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/elasticsearch_rollback/");
            var log = new KLogger("ESMigrator");
            log.Info("epg v3 rollback is starting");

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
                log.Info($"Starting epg v3 rollback partner:[{config.PartnerId}].");

                var currentEpgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(config.PartnerId);
                log.Info($"Detected current epg feature version:{currentEpgFeatureVersion}");
                if (currentEpgFeatureVersion == EpgFeatureVersion.V1)
                {
                    log.Info("feature version is already v1/v2, nothing to do...");
                    return;
                }

                if (currentEpgFeatureVersion == EpgFeatureVersion.V2 && !config.RollbackFromBackup)
                {
                    log.Info("feature version is already v1/v2, nothing to do...");
                    return;
                }

                var indexManager = IndexManagerFactory.Instance.GetIndexManager(config.PartnerId);
                switch (config.OriginalEpgVersion)
                {
                    case EpgFeatureVersion.V1:
                        if (config.PreserveDataFromV3)
                        {
                            indexManager.RollbackEpgV3ToV1(config.BatchSize);
                        }
                        else
                        {
                            indexManager.RollbackEpgV3ToV1WithoutReindexing(config.RollbackFromBackup, config.BatchSize);
                        }
                        break;
                    case EpgFeatureVersion.V2:
                        if (config.PreserveDataFromV3)
                        {
                            indexManager.RollbackEpgV3ToV2(config.BatchSize);
                        }
                        else
                        {
                            indexManager.RollbackEpgV3ToV2WithoutReindexing(config.RollbackFromBackup, config.BatchSize);
                        }
                        break;
                    default: throw new Exception("unsupported epg feature version to rollback");
                }

                log.Info($"Rollback completed...");
            }
            catch (Exception e)
            {
                log.Error("unexpected error: ", e);
            }
        }
    }
}
