using System;
using System.IO;
using System.Threading;
using ApiObjects;
using Core.Api;
using Core.Catalog;
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
                var indexManager = IndexManagerFactory.Instance.GetIndexManager(config.PartnerId);
                switch (config.OriginalEpgVersion)
                {
                    case EpgFeatureVersion.V1:
                        indexManager.RollbackEpgV3ToV1(config.BatchSize);
                        break;
                    case EpgFeatureVersion.V2:
                        indexManager.RollbackEpgV3ToV2(config.BatchSize);
                        break;
                    default: throw new Exception("unsupported epg feature version to rollback");
                }

                log.Info($"Migration completed...");
            }
            catch (Exception e)
            {
                log.Error("unexpected error: ", e);
            }
        }
    }
}
