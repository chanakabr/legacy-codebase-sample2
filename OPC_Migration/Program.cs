using ApiObjects.Catalog;
using Core.Catalog;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using ApiObjects;

namespace OPC_Migration
{
    class Program
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static int groupId = 0, regularGroupId = 0, linearMediaTypeId = 0, programMediaTypeId = 0;
        public static bool shouldValidate = false, shouldMigrate = false, shouldBackup = false, shouldRollback = false;
        public static Stopwatch watch = new Stopwatch();
        public static long sequenceId = 0;

        static void Main(string[] args)
        {
            try
            {
                Initialize();
                Console.WriteLine("Enter selected operation:\n 1.Migration\n 2.Validation\n 3.Backup\n 4.Rollback");

                #region Select operation and get input variables

                int operation = 0;
                while (!int.TryParse(Console.ReadLine(), out operation) || operation < 1 || operation > 4)
                {
                    Console.WriteLine("Please enter a valid operation number between 1-4");
                }

                switch (operation)
                {
                    // only migration
                    case 1:
                        shouldMigrate = true;
                        break;
                    // only validation
                    case 2:
                        shouldValidate = true;
                        break;
                    // only backup
                    case 3:
                        shouldBackup = true;
                        break;
                    // only rollback
                    case 4:
                        shouldRollback = true;
                        break;
                }

                Console.WriteLine("Enter parent groupId");
                while (!int.TryParse(Console.ReadLine(), out groupId) || groupId < 1)
                {
                    Console.WriteLine("Please enter a valid groupId");
                }

                if (shouldValidate || shouldMigrate)
                {
                    Console.WriteLine("Enter regular groupId");
                    while (!int.TryParse(Console.ReadLine(), out regularGroupId) || groupId < 1)
                    {
                        Console.WriteLine("Please enter a valid regular groupId");
                    }
                    Console.WriteLine("Enter Linear media type id, 0 for none");
                    while (!int.TryParse(Console.ReadLine(), out linearMediaTypeId) || groupId < 1)
                    {
                        Console.WriteLine("Please enter a valid Linear media type id or 0 for none");
                    }
                    Console.WriteLine("Enter Program(EPG) media type id, 0 for none");
                    while (!int.TryParse(Console.ReadLine(), out programMediaTypeId) || groupId < 1)
                    {
                        Console.WriteLine("Please enter a valid Program(EPG) media type id or 0 for none");
                    }
                }

                #endregion

                #region Initialize objects

                HashSet<long> groupExtraLanguageIdsToSave = new HashSet<long>();
                HashSet<long> groupPicIdsToSave = new HashSet<long>();
                Dictionary<string, Core.Catalog.Ratio> groupRatios = new Dictionary<string, Core.Catalog.Ratio>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, ImageType> groupImageTypes = new Dictionary<string, ImageType>(StringComparer.OrdinalIgnoreCase);
                Dictionary<long, MediaFileType> groupMediaFileTypes = new Dictionary<long, MediaFileType>();
                Dictionary<int, long> mediaTypeIdToMediaFileTypeIdMap = new Dictionary<int, long>();
                Dictionary<string, Dictionary<string, Topic>> groupTopics = new Dictionary<string, Dictionary<string, Topic>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, AssetStruct> groupAssetStructs = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
                Dictionary<long, Dictionary<string, string>> assetStructTopicsMap = new Dictionary<long, Dictionary<string, string>>();
                List<MediaAsset> assets = new List<MediaAsset>();
                Dictionary<long, string> picIdToImageTypeNameMap = new Dictionary<long, string>();
                Dictionary<long, Dictionary<string, string>> assetsImageTypesToAdd = new Dictionary<long, Dictionary<string, string>>();
                Dictionary<long, string> picIdToUpdatedContentIdValue = new Dictionary<long, string>();
                List<GroupsCacheManager.Channel> groupChannels = new List<GroupsCacheManager.Channel>();
                GroupsCacheManager.Group group = null;

                #endregion

                #region Validation and prepare data

                if (shouldValidate || shouldMigrate)
                {

                    group = new GroupsCacheManager.GroupManager().GetGroup(groupId);
                    if (group == null)
                    {
                        log.ErrorFormat("Failed getting group object for groupId: {0}, stopping validation and data preparation process until issue will be solved", groupId);
                        Console.WriteLine("Failed getting group object for groupId: {0}, stopping validation and data preparation process until issue will be solved", groupId);
                        return;
                    }
                    else
                    {
                        Utils.TrimEndSpaceFromGroup(group);
                    }

                    // Validate and prepare data
                    ValidateAndPrepareDataManager prepDataMang = new ValidateAndPrepareDataManager(groupId, regularGroupId, linearMediaTypeId, programMediaTypeId);
                    Console.WriteLine("Starting validation and data preparation");
                    log.DebugFormat("Starting validation and data preparation");
                    watch.Restart();
                    List<eMigrationResultStatus> prepareDataResults = prepDataMang.PrepareMigrationData(group, ref groupPicIdsToSave, ref groupRatios, ref groupImageTypes, ref groupMediaFileTypes, ref mediaTypeIdToMediaFileTypeIdMap,
                                                                                ref groupTopics, ref groupAssetStructs, ref assetStructTopicsMap, ref assets, ref picIdToImageTypeNameMap,
                                                                                ref groupChannels, ref assetsImageTypesToAdd, ref picIdToUpdatedContentIdValue, ref groupExtraLanguageIdsToSave);
                    watch.Stop();
                    Console.WriteLine("Validation and prepare data results: {0}, ElapsedMilliseconds: {1}", string.Join(",", prepareDataResults.Select(x => x.ToString())), watch.ElapsedMilliseconds);
                    log.DebugFormat("Validation and prepare data results: {0}, ElapsedMilliseconds: {1}", string.Join(",", prepareDataResults.Select(x => x.ToString())), watch.ElapsedMilliseconds);
                    if (prepareDataResults.Count > 1 || prepareDataResults[0] != eMigrationResultStatus.OK)
                    {
                        log.ErrorFormat("Stopping validation and data preparation process until issues will be resolved for groupId: {0}", groupId);
                        Console.WriteLine("Stopping validation and data preparation process until issues will be resolved for groupId: {0} press enter to close", groupId);
                        Console.ReadLine();
                        return;
                    }
                }

                #endregion

                #region Backup

                // added for https://kaltura.atlassian.net/browse/GEN-693 - Simplify the OPC migration script to not relate to the rollback script
                if (shouldMigrate)
                {
                    Console.WriteLine("Do you want to create backup? (true/false) THIS ISN'T NEEDED IF YOU ARE USING MIGRATION PROCESS CREATED BY Arnon Lempert!!!");
                    while (!bool.TryParse(Console.ReadLine(), out shouldBackup))
                    {
                        Console.WriteLine("Please enter a valid answer (true or false)");
                    }
                }

                if (shouldBackup)
                {
                    // Backup existing data before migration
                    BackupManager backupMang = new BackupManager(groupId);
                    Console.WriteLine("Starting backup process");
                    log.DebugFormat("Starting backup process");
                    watch.Restart();
                    eMigrationResultStatus backUpStatus = backupMang.CreateBackup();
                    watch.Stop();
                    log.DebugFormat("Backup result: {0}, ElapsedMilliseconds: {1}, sequenceId: {2}", backUpStatus.ToString(), watch.ElapsedMilliseconds, backupMang.SequenceId);
                    Console.WriteLine("Backup result: {0}, ElapsedMilliseconds: {1}, sequenceId: {2}", backUpStatus.ToString(), watch.ElapsedMilliseconds, backupMang.SequenceId);
                    sequenceId = backupMang.SequenceId;
                    if (backUpStatus != eMigrationResultStatus.OK)
                    {
                        log.ErrorFormat("Stopping Backup process until backup issues will be resolved for groupId: {0}", groupId);
                        Console.WriteLine("Stopping Backup process until backup issues will be resolved for groupId: {0}, press enter to close", groupId);
                        return;
                    }
                }

                #endregion

                #region Migration

                if (shouldMigrate)
                {
                    bool shouldStartMigration = false;
                    Console.WriteLine("Start migration? true/false");
                    while (!bool.TryParse(Console.ReadLine(), out shouldStartMigration))
                    {
                        Console.WriteLine("Please enter a valid answer (true or false)");
                    }
                    
                    Console.WriteLine("Use mig_ table prefix? true/false");
                    bool useMigTablesPrefix = false;
                    while (!bool.TryParse(Console.ReadLine(), out useMigTablesPrefix))
                    {
                        Console.WriteLine("Please enter a valid answer (true or false)");
                    }

                    if (shouldStartMigration)
                    {
                        // Perform the migration
                        MigrateManager migrationMang = new MigrateManager(groupId, sequenceId, shouldBackup, useMigTablesPrefix);
                        Console.WriteLine("Performing Migration");
                        log.DebugFormat("Performing Migration");
                        watch.Restart();
                        eMigrationResultStatus migrationStatus = migrationMang.PerformMigration(group, groupPicIdsToSave, ref groupRatios, ref groupImageTypes, ref groupMediaFileTypes, ref mediaTypeIdToMediaFileTypeIdMap,
                                                                                ref groupTopics, groupAssetStructs, assetStructTopicsMap, assets, picIdToImageTypeNameMap,
                                                                                assetsImageTypesToAdd, picIdToUpdatedContentIdValue, groupChannels, groupExtraLanguageIdsToSave);
                        watch.Stop();
                        log.DebugFormat("Migration result: {0}, ElapsedMilliseconds: {1}", migrationStatus.ToString(), watch.ElapsedMilliseconds);
                        Console.WriteLine("Migration result: {0}, ElapsedMilliseconds: {1}", migrationStatus.ToString(), watch.ElapsedMilliseconds);
                        if (migrationStatus != eMigrationResultStatus.OK)
                        {
                            log.ErrorFormat("Stopping Migration process until issues will be resolved for groupId: {0}", groupId);
                            Console.WriteLine("Stopping Migration process until issues will be resolved");
                            // Rollback data
                            Console.WriteLine("Should perform rollBack? true/false");
                            bool shouldRollBack = bool.Parse(Console.ReadLine());
                            if (shouldRollBack)
                            {
                                PerformRollback();
                            }
                        }
                    }
                }

                #endregion

                #region Rollback

                if (shouldRollback)
                {
                    if (sequenceId == 0)
                    {
                        Console.WriteLine("Please enter a valid sequenceId to perform rollback");
                        while (!long.TryParse(Console.ReadLine(), out sequenceId) || sequenceId < 1)
                        {
                            Console.WriteLine("Please enter a valid sequenceId to perform rollback");
                        }
                    }

                    PerformRollback();
                }

                #endregion
            }
            catch (Exception ex)
            {
                log.Error("An error occurred during OPC migration process", ex);
                Console.WriteLine($"An error occurred during OPC migration process {ex.Message}");
            }

            Console.WriteLine("Done, press enter to close");
            Console.ReadLine();
        }

        private static void PerformRollback()
        {
            Console.WriteLine("Starting rollback");
            log.DebugFormat("Starting rollback");
            watch.Restart();
            // Rollback the migration
            RollbackManager rollbackManager = new RollbackManager(groupId, sequenceId);
            eMigrationResultStatus rollBackStatus = rollbackManager.PerformRollback();
            watch.Stop();
            log.DebugFormat("Rollback result: {0}, ElapsedMilliseconds: {1}", rollBackStatus.ToString(), watch.ElapsedMilliseconds);
            Console.WriteLine("Rollback result: {0}, ElapsedMilliseconds: {1}", rollBackStatus.ToString(), watch.ElapsedMilliseconds);
        }

        private static void Initialize()
        {
            string monitorUniqueGuid = Guid.NewGuid().ToString();
            var defaultLogDir = $@"/var/log/opc-migration";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);

            ConfigurationManager.ApplicationConfiguration.Init();
            CachingProvider.LayeredCache.LayeredCache.Instance.DisableInMemoryCache();
        }

    }
}
