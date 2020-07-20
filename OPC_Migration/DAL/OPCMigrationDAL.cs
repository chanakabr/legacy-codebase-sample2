using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace OPC_Migration
{
    public class OPCMigrationDAL : BaseDal
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string timeoutFromConfig = System.Configuration.ConfigurationManager.AppSettings["OPC_DAL_TIMEOUT"];
        private const int DEFAULT_OPC_TIMEOUT = 300;

        public static int GetTimeoutValue()
        {
            int result = 0;
            if (string.IsNullOrEmpty(timeoutFromConfig) || !int.TryParse(timeoutFromConfig, out result) || result <= DEFAULT_OPC_TIMEOUT)
            {
                result = DEFAULT_OPC_TIMEOUT;
            }

            return result;
        }

        #region BACKUP Procedures

        internal static long GetSequenceIdForBackup()
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_Get_Sequence");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());

            return sp.ExecuteReturnValue<long>();
        }

        // Debug  = 1 doesn't create the snapshot, 0 = create the snapshot
        internal static bool CreateTvinciSnapshotForBackup(long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_Create_Snapshot");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@Debug", 0);
            sp.AddParameter("@sequence", sequenceId); 
            sp.SetTimeout(GetTimeoutValue());

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupGroupIdNeededTables(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupGroupIdNeededTables");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@Where_clause", whereClause);           
            sp.AddParameter("@SequenceId", sequenceId);
            sp.SetTimeout(GetTimeoutValue());

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupMediaTypes(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("BackupMediaTypes");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupMediaAssets(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("BackupAssets");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);            
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupChannels(string whereClause, long sequenceId)
        {            
            StoredProcedure sp = new StoredProcedure("BackupChannels");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupEpgChannels(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupEpgChannels");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupGroupsMediaType(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupGroupsMediaType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupTagsAndTagsTranslate(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupTagsAndTagsTranslate");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupMediaFiles(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupMediaFiles");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupPics(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupPics");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupGroupLanguagesAndPicSizes(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupGroupLanguagesAndPicSizes");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupParentalRulesAndValues(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupParentalRulesAndValues");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupMediaConcurrencyRulesAndValues(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupMediaConcurrencyRulesAndValues");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool BackupAssetLiceCycleRulesActions(string whereClause, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_BackupAssetLiceCycleRulesActions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@Where_clause", whereClause);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        #endregion

        #region Rollback Procedures

        internal static DataTable GetRollbackTables(long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_Get_Backup_Tables");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@seq", sequenceId);
            sp.SetTimeout(GetTimeoutValue());

            return sp.Execute();
        }

        internal static bool RollbackNewOpcTables(int groupId, long updaterId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_RollbackNewOpcTables");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.SetTimeout(GetTimeoutValue()); 

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool RollbackPerTable(string tableName, List<string> columnsList, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_Rollback_Per_Table");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@backup_table", tableName);
            sp.AddParameter("@seq", sequenceId);
            sp.AddIDListParameter<string>("@columns_list", columnsList, "STR");            
            sp.SetTimeout(GetTimeoutValue());

            return sp.ExecuteReturnValue<int>() > 0;
        }

        #endregion

        internal static DataTable GetGroupLinearMediaTypeIds(List<int> groupIds)
        {
            DataTable dt = null;
            DataSetSelectQuery selectQuery = new DataSetSelectQuery();
            selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
            selectQuery.SetTimeout(GetTimeoutValue());
            selectQuery += string.Format("select id from media_types with(nolock) where [status]=1 and is_linear=1 and group_id in ({0})", string.Join(",", groupIds));            
            dt = selectQuery.Execute("getGroupLinearMediaTypeIds", true);
            selectQuery.Finish();
            selectQuery = null;

            return dt;
        }

        internal static DataSet GetMediaTypeMetasAndTags(long mediaTypeId)
        {
            StoredProcedure sp = new StoredProcedure("GetMediaTypeMetasAndTags");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@MediaTypeId", mediaTypeId);

            return sp.ExecuteDataSet(true);
        }

        internal static DataTable GetGroupMetasSearchRelatedInfo(int groupId)
        {
            StoredProcedure sp = new StoredProcedure("GetGroupMetasSearchRelatedInfo");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);

            return sp.Execute(true);
        }

        internal static DataTable GetTagTypesSearchRelatedInfo(List<int> tagTypeIds)
        {
            StoredProcedure sp = new StoredProcedure("GetTagTypesSearchRelatedInfo");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@TagTypeIds", tagTypeIds, "Id");

            return sp.Execute(true);
        }

        internal static DataTable GetAllGroupsRatios(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetAllGroupsRatios");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");

            return sp.Execute(true);
        }

        internal static DataTable GetMediaFileTypeToQualityMap(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetMediaFileTypeToQualityMap");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");

            return sp.Execute(true);
        }

        internal static DataTable GetAllGroupsFileTypes(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetAllGroupsFileTypes");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");

            return sp.Execute(true);
        }

        internal static bool UpdateTagTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeIdToTopicIdMap, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdateTagTableWithTopicIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddKeyValueListParameter<long, long>("@TagTypeIdToTopicIdMap", tagTypeIdToTopicIdMap, "key", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static DataTable GetAllGroupMediaIds(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetAllGroupMediaIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");            

            return sp.Execute(true);
        }

        internal static bool UpdatePicsTableWithImageTypeId(List<KeyValuePair<long, long>> picIdToImageTypeId, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdatePicsTableWithImageTypeId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddKeyValueListParameter<long, long>("@PicIdToImageTypeId", picIdToImageTypeId, "key", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdatePicContentIdsBaseUrl(List<KeyValuePair<long, string>> contentIdToUpdatedContentIdValue, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdatePicContentIdsBaseUrl");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddKeyValueListParameter<long, string>("@PicIdToUpdateBaseUrl", contentIdToUpdatedContentIdValue, "key", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdateMediaFilesTableWithMediaTypeId(int groupId, List<KeyValuePair<int, long>> mediaTypeIdToMediaFileTypeId, long updaterId, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("UpdateMediaFilesTableWithMediaTypeId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddKeyValueListParameter<int, long>("@MediaTypeIdToMediaFileTypeId", mediaTypeIdToMediaFileTypeId, "key", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdateGroupIdInNeededTables(int groupId, long updaterId, long sequenceId, bool shouldBackup)
        {            
            StoredProcedure sp = new StoredProcedure("UpdateGroupIdInNeededTables");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        // if updaterId > 0 is sent, the procedure assumes the updater_id column exists for the table and updates it
        // if withUpdateDate = 1 is sent, the procedure assumes the update_date column exists for the table and updates it
        internal static bool UpdateTableGroupId(string tableName, int groupId, long sequenceId, long updaterId, int withUpdateDate, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_Update_Table");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@table", tableName);
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@update_date", withUpdateDate);
            sp.AddParameter("@updater_id", updaterId);
            sp.AddParameter("@seq", sequenceId);
            sp.AddParameter("@shouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }        

        internal static DataTable GetPicIdsFromPicContentIds(List<string> picBaseUrls)
        {
            StoredProcedure sp = new StoredProcedure("GetPicIdsFromPicContentIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddIDListParameter<string>("@PicBaseUrls", picBaseUrls, "STR");

            return sp.Execute(true);
        }

        internal static bool UpdateParentalRulesTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeIdToTopicIdMap, bool isEpg, long updaterId, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("UpdateParentalRulesTableWithTopicIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddKeyValueListParameter<long, long>("@TagTypeIdToTopicIdMap", tagTypeIdToTopicIdMap, "key", "value");
            sp.AddParameter("@IsEpg", isEpg ? 1 : 0);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdateMediaConcurrencyRulesTableWithTopicIds(int groupId, List<KeyValuePair<long, long>> tagTypeIdToTopicIdMap, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdateMediaConcurrencyRulesTableWithTopicIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddKeyValueListParameter<long, long>("@TagTypeIdToTopicIdMap", tagTypeIdToTopicIdMap, "key", "value");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdateDuplicateTagValuesAndTagTranslations(int groupId, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdateDuplicateTagValuesAndTagTranslations");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static DataTable GetChannelsTranslations(List<int> groupIds, HashSet<int> channelIds)
        {
            StoredProcedure sp = new StoredProcedure("GetChannelsTranslationsByChannelIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");
            sp.AddIDListParameter<int>("@ChannelIds", channelIds, "Id");

            return sp.Execute(true);
        }

        internal static bool UpdateGroupPicIds(int groupId, List<int> groupIds, HashSet<long> groupIdsToSave, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdateGroupPicIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);            
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");
            sp.AddIDListParameter<long>("@GroupIdsToSave", groupIdsToSave, "Id");            
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static DataTable GetGroupChannelIdsIncludeInActive(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetGroupChannelIdsIncludeInActive");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");          

            return sp.Execute(true);
        }

        internal static DataTable GetAllEpgChannelsOfMediaIds(List<int> groupIds, ICollection<string> mediaIdsOfEpgChannels)
        {
            StoredProcedure sp = new StoredProcedure("GetAllEpgChannelsOfMediaIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");
            sp.AddIDListParameter<string>("@MediaIds", mediaIdsOfEpgChannels, "STR");

            return sp.Execute(true);
        }

        internal static DataTable GetGroupExtraLanguagesForMigration(List<int> groupIds)
        {
            StoredProcedure sp = new StoredProcedure("GetGroupExtraLanguagesForMigration");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");            

            return sp.Execute(true);
        }

        internal static bool UpdateGroupExtraLanguages(int groupId, List<int> groupIds, HashSet<long> groupExtraLanguageIdsToSave, long updaterId, long sequenceId, bool shouldBackup)
        {
            StoredProcedure sp = new StoredProcedure("UpdateGroupExtraLanguages");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter<int>("@GroupIds", groupIds, "Id");
            sp.AddIDListParameter<long>("@GroupExtraLanguageIdsToSave", groupExtraLanguageIdsToSave, "Id");
            sp.AddParameter("@UpdaterId", updaterId);
            sp.AddParameter("@SequenceId", sequenceId);
            sp.AddParameter("@ShouldBackup", shouldBackup ? 1 : 0);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        internal static bool UpdateIdsAsMigratedInOpcBackupTable(string tableName, List<long> ids, long sequenceId)
        {
            StoredProcedure sp = new StoredProcedure("OPC_Migration_UpdateIdsAsMigratedInOpcBackupTable");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.SetTimeout(GetTimeoutValue());
            sp.AddParameter("@TableName", tableName);
            sp.AddIDListParameter<long>("@Ids", ids, "Id");            
            sp.AddParameter("@SequenceId", sequenceId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

    }
}
