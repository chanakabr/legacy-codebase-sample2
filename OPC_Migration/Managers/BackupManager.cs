using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OPC_Migration
{
    public class BackupManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private int groupId;
        private string WhereClause;

        public long SequenceId { get; set; }

        public BackupManager(int groupId)
        {
            this.groupId = groupId;            
        }

        public eMigrationResultStatus CreateBackup()
        {
            eMigrationResultStatus result = eMigrationResultStatus.OK;
            SequenceId = OPCMigrationDAL.GetSequenceIdForBackup();
            if (SequenceId <= 0)
            {
                log.Debug("GetSequenceIdForBackup failed");
                Console.WriteLine("GetSequenceIdForBackup failed");
                return eMigrationResultStatus.GetSequenceIdForBackupFailed;
            }

            log.DebugFormat("GetSequenceIdForBackup succeeded, sequenceId: {0}", SequenceId);
            Console.WriteLine("GetSequenceIdForBackup succeeded, sequenceId: {0}", SequenceId);

            Console.WriteLine("Should create snapshot? (only for PRODUCTION USE!!!) true/false");
            bool shouldCreateSnapshot = bool.Parse(Console.ReadLine());
            if (shouldCreateSnapshot)
            {
                if (!CreateSnapshot())
                {
                    log.Debug("CreateSnapshot failed");
                    Console.WriteLine("CreateSnapshot failed");
                    return eMigrationResultStatus.FailedCreateSnapshot;
                }

                log.Debug("CreateSnapshot succeeded");
                Console.WriteLine("CreateSnapshot succeeded");
            }

            List<int> groupIds = GroupsCacheManager.Utils.Get_SubGroupsTree(groupId);
            WhereClause = string.Format("group_id in ({0}) and [status]=1", string.Join(",", groupIds));

            if (!BackupOfPartialTables())
            {
                log.Debug("BackupOfPartialTables failed");
                Console.WriteLine("BackupOfPartialTables failed");
                return eMigrationResultStatus.BackupOfPartialTablesFailed;
            }

            log.Debug("BackupOfPartialTables succeeded");
            Console.WriteLine("BackupOfPartialTables succeeded");

            if (!BackupOfFullTables())
            {
                log.Debug("BackupOfFullTables failed");
                Console.WriteLine("BackupOfFullTables failed");
                return eMigrationResultStatus.BackupOfFullTablesFailed;
            }

            log.Debug("BackupOfFullTables succeeded");
            Console.WriteLine("BackupOfFullTables succeeded");

            return result;
        }

        private bool CreateSnapshot()
        {
            try
            {
                return OPCMigrationDAL.CreateTvinciSnapshotForBackup(SequenceId);
            }
            catch (Exception ex)
            {
                log.Error("failed CreateSnapshot for sequenceID: {0}", ex);
                return false;
            }
        }

        private bool BackupOfPartialTables()
        {
            try
            {                
                return OPCMigrationDAL.BackupGroupIdNeededTables(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupOfPartialTables for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }
        }

        private bool BackupOfFullTables()
        {
            bool res = true;
            try
            {
                if (BackupMediaTypes())
                {
                    log.DebugFormat("BackupMediaTypes Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupMediaTypes Failed");
                }

                if (BackupAssets())
                {
                    log.DebugFormat("BackupAssets Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupAssets Failed");
                }

                if (BackupChannels())
                {
                    log.DebugFormat("BackupChannels Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupChannels Failed");
                }

                if (BackupEpgChannels())
                {
                    log.DebugFormat("BackupEpgChannels Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupEpgChannels Failed");
                }

                if (BackupGroupsMediaType())
                {
                    log.DebugFormat("BackupGroupsMediaType Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupGroupsMediaType Failed");
                }

                if (BackupTagsAndTagsTranslate())
                {
                    log.DebugFormat("BackupTagsAndTagsTranslate Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupTagsAndTagsTranslate Failed");
                }

                if (BackupMediaFiles())
                {
                    log.DebugFormat("BackupMediaFiles Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupMediaFiles Failed");
                }

                if (BackupPics())
                {
                    log.DebugFormat("BackupPics Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupPics Failed");
                }

                if (BackupGroupLanguagesAndPicSizes())
                {
                    log.DebugFormat("BackupGroupLanguagesAndPicSizes Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupGroupLanguagesAndPicSizes Failed");
                }

                if (BackupParentalRulesAndValues())
                {
                    log.DebugFormat("BackupParentalRulesAndValues Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupParentalRulesAndValues Failed");
                }

                if (BackupMediaConcurrencyRulesAndValues())
                {
                    log.DebugFormat("BackupMediaConcurrencyRulesAndValues Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupMediaConcurrencyRulesAndValues Failed");
                }

                if (BackupAssetLiceCycleRulesActions())
                {
                    log.DebugFormat("BackupAssetLiceCycleRulesActions Done");
                }
                else
                {
                    res = false;
                    log.ErrorFormat("BackupAssetLiceCycleRulesActions Failed");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupOfFullTables for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }

            return res;
        }

        private bool BackupMediaTypes()
        {
            try
            {
                return OPCMigrationDAL.BackupMediaTypes(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupMediaTypes for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }
        }

        private bool BackupAssets()
        {
            try
            {
                return OPCMigrationDAL.BackupMediaAssets(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupAssets for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupChannels()
        {
            try
            {
                return OPCMigrationDAL.BackupChannels(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupChannels for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupEpgChannels()
        {
            try
            {
                return OPCMigrationDAL.BackupEpgChannels(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupEpgChannels for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }
        }

        private bool BackupGroupsMediaType()
        {
            try
            {
                return OPCMigrationDAL.BackupGroupsMediaType(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupGroupsMediaType for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupTagsAndTagsTranslate()
        {
            try
            {
                return OPCMigrationDAL.BackupTagsAndTagsTranslate(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupTagsAndTagsTranslate for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupMediaFiles()
        {
            try
            {
                return OPCMigrationDAL.BackupMediaFiles(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupMediaFiles for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }
        }

        private bool BackupPics()
        {
            try
            {
                return OPCMigrationDAL.BackupPics(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupPics for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupGroupLanguagesAndPicSizes()
        {
            try
            {
                return OPCMigrationDAL.BackupGroupLanguagesAndPicSizes(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupGroupLanguagesAndPicSizes for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupParentalRulesAndValues()
        {
            try
            {
                return OPCMigrationDAL.BackupParentalRulesAndValues(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupParentalRulesAndValues for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            }
        }

        private bool BackupMediaConcurrencyRulesAndValues()
        {
            try
            {
                return OPCMigrationDAL.BackupMediaConcurrencyRulesAndValues(WhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupMediaConcurrencyRulesAndValues for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

        private bool BackupAssetLiceCycleRulesActions()
        {
            try
            {
                string alcrWhereClause = string.Format("alcr_id in (select id from asset_life_cycle_rules where group_id={0}) and [status]=1", groupId);
                return OPCMigrationDAL.BackupAssetLiceCycleRulesActions(alcrWhereClause, SequenceId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BackupAssetLiceCycleRulesActions for whereClause :{0}, sequenceID: {1}", WhereClause, SequenceId), ex);
                return false;
            };
        }

    }
}
