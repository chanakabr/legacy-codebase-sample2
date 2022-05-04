using System;
using System.Reflection;
using FeatureFlag;
using IngestHandler.Common.Infrastructure;
using IngestTransformationHandler.Managers;
using Ott.Lib.FeatureToggle.Managers;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

namespace IngestV2.Compaction.Tool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var partnerId = -1;
            if (args.Length == 0 || !int.TryParse(args[0], out partnerId))
            {
                throw new ArgumentException("missing or malformed partner Id arg");
            }

            InitLogger();
            ApplicationConfiguration.Init();
            Console.Title = Assembly.GetEntryAssembly().GetName().ToString();


            var featureFlagIngestContext = new DummyFeatureFlagIngestContext();
            var compactionManager = new IndexCompactionManager(new PhoenixFeatureFlag(featureFlagIngestContext, FeatureToggleManager.Instance()));
            compactionManager.RunEpgIndexCompactionIfRequired(partnerId);
        }

        private static void InitLogger()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyVersion = $"{fvi.FileMajorPart}_{fvi.FileMinorPart}_{fvi.FileBuildPart}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/EventHandlers/");
        }
    }
}
