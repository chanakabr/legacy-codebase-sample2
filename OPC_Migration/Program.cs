using ApiObjects.Catalog;
using Core.Catalog;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using ApiObjects;
using Phx.Lib.Appconfig;

namespace OPC_Migration
{
    /*
     * Env vars:
     * OPERATION - valid values (not case sensitive)
     *  validate
     *  migrate
     *  rollback
     *  backup
     * PARENT_GROUP_ID (should be > 0)
     * REGULAR_GROUP_ID (should be > 0)
     * LINEAR_MEDIA_TYPE_ID (should be >= 0)
     * PROGRAM_MEDIA_TYPE_ID (should be >= 0)
     * SHOULD_BACKUP
     * SHOULD_USE_MIG_TABLE_PREFIX
     * SHOULD_START_MIGRATION_AUTOMATICALLY
     */

    class Program
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static int _groupId = 0;
        public static int _regularGroupId = 0;
        public static int _linearMediaTypeId = 0;
        public static int _programMediaTypeId = 0;
        public static bool _shouldValidate = false;
        public static bool _shouldMigrate = false;
        public static bool _shouldBackup = false;
        public static bool _shouldRollback = false;
        public static Stopwatch _watch = new Stopwatch();
        public static long _sequenceId = 0;

        static void Main(string[] args)
        {
            try
            {
                Initialize();

                #region Select operation and get input variables

                var operationEnv = Environment.GetEnvironmentVariable("OPERATION");

                MigratorOperation migratorOperation = MigratorOperation.none;
                if (!string.IsNullOrEmpty(operationEnv))
                {
                    Enum.TryParse<MigratorOperation>(operationEnv.ToLowerInvariant(), out migratorOperation);
                }

                if (migratorOperation == MigratorOperation.none)
                {
                    Console.WriteLine("Enter selected operation:\n 1.Migration\n 2.Validation\n 3.Backup\n 4.Rollback");

                    int operation = 0;
                    while (!int.TryParse(Console.ReadLine(), out operation) || operation < 1 || operation > 4)
                    {
                        Console.WriteLine("Please enter a valid operation number between 1-4");
                    }

                    migratorOperation = (MigratorOperation)operation;
                }
                switch (migratorOperation)
                {
                    case MigratorOperation.migrate:
                        _shouldMigrate = true;
                        break;
                    case MigratorOperation.validate:
                        _shouldValidate = true;
                        break;
                    case MigratorOperation.backup:
                        _shouldBackup = true;
                        break;
                    case MigratorOperation.rollback:
                        _shouldRollback = true;
                        break;
                }

                int.TryParse(Environment.GetEnvironmentVariable("PARENT_GROUP_ID"), out _groupId);

                if (_groupId < 1)
                {
                    Console.WriteLine("Enter parent groupId");
                    while (!int.TryParse(Console.ReadLine(), out _groupId) || _groupId < 1)
                    {
                        Console.WriteLine("Please enter a valid groupId");
                    }
                }

                if (_shouldValidate || _shouldMigrate)
                {
                    int.TryParse(Environment.GetEnvironmentVariable("REGULAR_GROUP_ID"), out _regularGroupId);

                    if (_regularGroupId < 1)
                    {
                        Console.WriteLine("Enter regular groupId");
                        while (!int.TryParse(Console.ReadLine(), out _regularGroupId) || _regularGroupId < 1)
                        {
                            Console.WriteLine("Please enter a valid regular groupId");
                        }
                    }

                    int.TryParse(Environment.GetEnvironmentVariable("LINEAR_MEDIA_TYPE_ID"), out _linearMediaTypeId);

                    if (_linearMediaTypeId < 0)
                    {
                        Console.WriteLine("Enter Linear media type id, 0 for none");
                        while (!int.TryParse(Console.ReadLine(), out _linearMediaTypeId) || _linearMediaTypeId < 0)
                        {
                            Console.WriteLine("Please enter a valid Linear media type id or 0 for none");
                        }
                    }
                    int.TryParse(Environment.GetEnvironmentVariable("PROGRAM_MEDIA_TYPE_ID"), out _programMediaTypeId);

                    if (_programMediaTypeId < 1)
                    {
                        Console.WriteLine("Enter Program(EPG) media type id, 0 for none");
                        while (!int.TryParse(Console.ReadLine(), out _programMediaTypeId) || _programMediaTypeId < 0)
                        {
                            Console.WriteLine("Please enter a valid Program(EPG) media type id or 0 for none");
                        }
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

                if (_shouldValidate || _shouldMigrate)
                {
                    group = new GroupsCacheManager.GroupManager().GetGroup(_groupId);
                    if (group == null)
                    {
                        log.ErrorFormat("Failed getting group object for groupId: {0}, stopping validation and data preparation process until issue will be solved", _groupId);
                        Console.WriteLine("Failed getting group object for groupId: {0}, stopping validation and data preparation process until issue will be solved", _groupId);
                        return;
                    }
                    else
                    {
                        Utils.TrimEndSpaceFromGroup(group);
                    }

                    // Validate and prepare data
                    ValidateAndPrepareDataManager prepDataMang = new ValidateAndPrepareDataManager(_groupId, _regularGroupId, _linearMediaTypeId, _programMediaTypeId);
                    Console.WriteLine("Starting validation and data preparation");
                    log.DebugFormat("Starting validation and data preparation");
                    _watch.Restart();
                    List<eMigrationResultStatus> prepareDataResults = prepDataMang.PrepareMigrationData(group, ref groupPicIdsToSave, ref groupRatios, ref groupImageTypes, ref groupMediaFileTypes, ref mediaTypeIdToMediaFileTypeIdMap,
                                                                                ref groupTopics, ref groupAssetStructs, ref assetStructTopicsMap, ref assets, ref picIdToImageTypeNameMap,
                                                                                ref groupChannels, ref assetsImageTypesToAdd, ref picIdToUpdatedContentIdValue, ref groupExtraLanguageIdsToSave);
                    _watch.Stop();
                    Console.WriteLine("Validation and prepare data results: {0}, ElapsedMilliseconds: {1}", string.Join(",", prepareDataResults.Select(x => x.ToString())), _watch.ElapsedMilliseconds);
                    log.DebugFormat("Validation and prepare data results: {0}, ElapsedMilliseconds: {1}", string.Join(",", prepareDataResults.Select(x => x.ToString())), _watch.ElapsedMilliseconds);
                    if (prepareDataResults.Count > 1 || prepareDataResults[0] != eMigrationResultStatus.OK)
                    {
                        log.ErrorFormat("Stopping validation and data preparation process until issues will be resolved for groupId: {0}", _groupId);
                        Console.WriteLine("Stopping validation and data preparation process until issues will be resolved for groupId: {0} press enter to close", _groupId);
                        Console.ReadLine();
                        return;
                    }
                }

                #endregion

                #region Backup

                // added for https://kaltura.atlassian.net/browse/GEN-693 - Simplify the OPC migration script to not relate to the rollback script
                if (_shouldMigrate)
                {
                    string shouldBackupString = Environment.GetEnvironmentVariable("SHOULD_BACKUP");
                    if (string.IsNullOrEmpty(shouldBackupString) || !bool.TryParse(shouldBackupString, out _shouldBackup))
                    {
                        Console.WriteLine("Do you want to create backup? (true/false) THIS ISN'T NEEDED IF YOU ARE USING MIGRATION PROCESS CREATED BY Arnon Lempert!!!");
                        while (!bool.TryParse(Console.ReadLine(), out _shouldBackup))
                        {
                            Console.WriteLine("Please enter a valid answer (true or false)");
                        }
                    }
                }

                if (_shouldBackup)
                {
                    // Backup existing data before migration
                    BackupManager backupMang = new BackupManager(_groupId);
                    Console.WriteLine("Starting backup process");
                    log.DebugFormat("Starting backup process");
                    _watch.Restart();
                    eMigrationResultStatus backUpStatus = backupMang.CreateBackup();
                    _watch.Stop();
                    log.DebugFormat("Backup result: {0}, ElapsedMilliseconds: {1}, sequenceId: {2}", backUpStatus.ToString(), _watch.ElapsedMilliseconds, backupMang.SequenceId);
                    Console.WriteLine("Backup result: {0}, ElapsedMilliseconds: {1}, sequenceId: {2}", backUpStatus.ToString(), _watch.ElapsedMilliseconds, backupMang.SequenceId);
                    _sequenceId = backupMang.SequenceId;
                    if (backUpStatus != eMigrationResultStatus.OK)
                    {
                        log.ErrorFormat("Stopping Backup process until backup issues will be resolved for groupId: {0}", _groupId);
                        Console.WriteLine("Stopping Backup process until backup issues will be resolved for groupId: {0}, press enter to close", _groupId);
                        return;
                    }
                }

                #endregion

                #region Migration

                if (_shouldMigrate)
                {
                    bool shouldStartMigration = false;

                    string shouldStartMigrationAutomatically = Environment.GetEnvironmentVariable("SHOULD_START_MIGRATION_AUTOMATICALLY");

                    if (!bool.TryParse(shouldStartMigrationAutomatically, out shouldStartMigration) || !shouldStartMigration)
                    {
                        Console.WriteLine("Start migration? true/false");
                        while (!bool.TryParse(Console.ReadLine(), out shouldStartMigration))
                        {
                            Console.WriteLine("Please enter a valid answer (true or false)");
                        }
                    }

                    string shouldUseMigTablePrefix = Environment.GetEnvironmentVariable("SHOULD_USE_MIG_TABLE_PREFIX");

                    bool useMigTablesPrefix = false;
                    if (string.IsNullOrEmpty(shouldUseMigTablePrefix) || !bool.TryParse(shouldUseMigTablePrefix, out useMigTablesPrefix))
                    {
                        Console.WriteLine("Use mig_ table prefix? true/false");
                        while (!bool.TryParse(Console.ReadLine(), out useMigTablesPrefix))
                        {
                            Console.WriteLine("Please enter a valid answer (true or false)");
                        }
                    }

                    if (shouldStartMigration)
                    {
                        // Perform the migration
                        MigrateManager migrationMang = new MigrateManager(_groupId, _sequenceId, _shouldBackup, useMigTablesPrefix);
                        Console.WriteLine("Performing Migration");
                        log.DebugFormat("Performing Migration");
                        _watch.Restart();
                        var migrationStatuses = migrationMang.PerformMigration(group, groupPicIdsToSave, ref groupRatios, ref groupImageTypes, ref groupMediaFileTypes, ref mediaTypeIdToMediaFileTypeIdMap,
                                                                                ref groupTopics, groupAssetStructs, assetStructTopicsMap, assets, picIdToImageTypeNameMap,
                                                                                assetsImageTypesToAdd, picIdToUpdatedContentIdValue, groupChannels, groupExtraLanguageIdsToSave);
                        _watch.Stop();
                        log.DebugFormat("Migration result: {0}, ElapsedMilliseconds: {1}", string.Join(",", migrationStatuses.Select(x => x.ToString())), _watch.ElapsedMilliseconds);
                        Console.WriteLine("Migration result: {0}, ElapsedMilliseconds: {1}", string.Join(",", migrationStatuses.Select(x => x.ToString())), _watch.ElapsedMilliseconds);
                        if (migrationStatuses.Any(status => status != eMigrationResultStatus.OK))
                        {
                            log.ErrorFormat("Stopping Migration process until issues will be resolved for groupId: {0}", _groupId);
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

                if (_shouldRollback)
                {
                    if (_sequenceId == 0)
                    {
                        Console.WriteLine("Please enter a valid sequenceId to perform rollback");
                        while (!long.TryParse(Console.ReadLine(), out _sequenceId) || _sequenceId < 1)
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
            _watch.Restart();
            // Rollback the migration
            RollbackManager rollbackManager = new RollbackManager(_groupId, _sequenceId);
            eMigrationResultStatus rollBackStatus = rollbackManager.PerformRollback();
            _watch.Stop();
            log.DebugFormat("Rollback result: {0}, ElapsedMilliseconds: {1}", rollBackStatus.ToString(), _watch.ElapsedMilliseconds);
            Console.WriteLine("Rollback result: {0}, ElapsedMilliseconds: {1}", rollBackStatus.ToString(), _watch.ElapsedMilliseconds);
        }

        private static void Initialize()
        {
            string monitorUniqueGuid = Guid.NewGuid().ToString();
            var defaultLogDir = $@"/var/log/opc-migration";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);

            ApplicationConfiguration.Init();
            CachingProvider.LayeredCache.LayeredCache.Instance.DisableInMemoryCache();
        }
    }

    public enum MigratorOperation
    {
        none = 0,
        migrate = 1,
        validate = 2,
        backup = 3,
        rollback = 4
    }
}
