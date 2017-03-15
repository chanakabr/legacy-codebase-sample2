using ApiObjects;
using ApiObjects.TimeShiftedTv;
using KLogMonitor;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class RecordingsDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;
        private const uint FIRST_FOLLOWER_BLOCK_LOCK_TTL_SEC = 300;
        private const uint FIRST_FOLLOWER_INDEX_LOCK_TTL_SEC = 60;
        private const string RECORDING_CONNECTION = "RECORDING_CONNECTION_STRING";        

        private static void HandleException(Exception ex)
        {
            log.Error("HandleException occurred ", ex);
        }

        public static DataTable GetRecordingByEpgId(int groupId, long epgId)
        {
            ODBCWrapper.StoredProcedure spGetRecordingByEpgId = new ODBCWrapper.StoredProcedure("GetRecordingByEpgId");
            spGetRecordingByEpgId.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingByEpgId.AddParameter("@GroupID", groupId);
            spGetRecordingByEpgId.AddParameter("@EpgId", epgId);    
            DataTable dt = spGetRecordingByEpgId.Execute();

            return dt;
        }

        public static DataTable InsertRecording(Recording recording, int groupId, RecordingInternalStatus? status, DateTime? viewableUntilDate)
        {
            int recordingStatus = 0;

            if (status != null)
            {
                recordingStatus = (int)status.Value;
            }
            else
            {
                switch (recording.RecordingStatus)
                {
                    case TstvRecordingStatus.Scheduled:
                    case TstvRecordingStatus.Recording:
                    case TstvRecordingStatus.Recorded:
                    case TstvRecordingStatus.OK:
                        {
                            recordingStatus = 0;
                            break;
                        }
                    case TstvRecordingStatus.Failed:
                        {
                            recordingStatus = 1;
                            break;
                        }
                    case TstvRecordingStatus.Canceled:
                        {
                            recordingStatus = 2;
                            break;
                        }
                    case TstvRecordingStatus.Deleted:
                        {
                            recordingStatus = 4;
                            break;
                        }
                    default:
                        break;
                }
            }

            ODBCWrapper.StoredProcedure spInsertRecording = new ODBCWrapper.StoredProcedure("InsertRecording");
            spInsertRecording.SetConnectionKey(RECORDING_CONNECTION);
            spInsertRecording.AddParameter("@GroupID", groupId);            
            spInsertRecording.AddParameter("@EpgId", recording.EpgId);
            spInsertRecording.AddParameter("@EpgChannelId", recording.ChannelId);
            spInsertRecording.AddParameter("@ExternalRecordingId", string.IsNullOrEmpty(recording.ExternalRecordingId) ? null : recording.ExternalRecordingId);
            spInsertRecording.AddParameter("@RecordingStatus", recordingStatus);
            spInsertRecording.AddParameter("@startDate", recording.EpgStartDate);
            spInsertRecording.AddParameter("@endDate", recording.EpgEndDate);
            spInsertRecording.AddParameter("@GetStatusRetries", recording.GetStatusRetries);
            spInsertRecording.AddParameter("@ViewableUntilDate", viewableUntilDate);
            spInsertRecording.AddParameter("@ViewableUntilEpoch", recording.ViewableUntilDate);
            spInsertRecording.AddParameter("@Crid", string.IsNullOrEmpty(recording.Crid) ? null : recording.Crid);

            DataTable dt = spInsertRecording.Execute();
            return dt;
        }

        public static DataTable GetRecordingById(long id, bool takeOnlyValidRecording = true)
        {
            ODBCWrapper.StoredProcedure spGetRecordingById = new ODBCWrapper.StoredProcedure("GetRecordingById");
            spGetRecordingById.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingById.AddParameter("@Id", id);
            spGetRecordingById.AddParameter("@TakeOnlyValidRecording", takeOnlyValidRecording ? 1 : 0);
            DataTable dt = spGetRecordingById.Execute();

            return dt;
        }

        public static bool UpdateRecording(Recording recording, int groupId, int rowStatus, int isActive, RecordingInternalStatus? status, DateTime? viewableUntilDate)
        {
            bool result = false;
            int recordingStatus = 0;

            if (status != null)
            {
                recordingStatus = (int)status.Value;
            }
            else
            {
                switch (recording.RecordingStatus)
                {
                    case TstvRecordingStatus.Scheduled:
                    case TstvRecordingStatus.Recording:
                    case TstvRecordingStatus.Recorded:
                    case TstvRecordingStatus.OK:
                        {
                            recordingStatus = 0;
                            break;
                        }
                    case TstvRecordingStatus.Failed:
                        {
                            recordingStatus = 1;
                            break;
                        }
                    case TstvRecordingStatus.Canceled:
                        {
                            recordingStatus = 2;
                            break;
                        }
                    case TstvRecordingStatus.Deleted:
                        {
                            recordingStatus = 4;
                            break;
                        }
                    default:
                        break;
                }
            }

            ODBCWrapper.StoredProcedure spUpdateRecording = new ODBCWrapper.StoredProcedure("UpdateRecording");
            spUpdateRecording.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateRecording.AddParameter("@GroupID", groupId);
            spUpdateRecording.AddParameter("@Id", recording.Id);
            spUpdateRecording.AddParameter("@EpgId", recording.EpgId);
            spUpdateRecording.AddParameter("@EpgChannelId", recording.ChannelId);
            spUpdateRecording.AddParameter("@ExternalRecordingId", string.IsNullOrEmpty(recording.ExternalRecordingId) ? null : recording.ExternalRecordingId);
            spUpdateRecording.AddParameter("@RecordingStatus", recordingStatus);
            spUpdateRecording.AddParameter("@startDate", recording.EpgStartDate);
            spUpdateRecording.AddParameter("@endDate", recording.EpgEndDate);
            spUpdateRecording.AddParameter("@status", rowStatus);
            spUpdateRecording.AddParameter("@isActive", isActive);
            spUpdateRecording.AddParameter("@GetStatusRetries", recording.GetStatusRetries);
            spUpdateRecording.AddParameter("@ViewableUntilDate", viewableUntilDate );
            spUpdateRecording.AddParameter("@ViewableUntilEpoch", recording.ViewableUntilDate);
            spUpdateRecording.AddParameter("@Crid", string.IsNullOrEmpty(recording.Crid) ? null : recording.Crid);

            result = spUpdateRecording.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool CancelRecording(long recordingId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelRecording");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@RecordID", recordingId);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool DeleteRecording(long recordingId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("DeleteRecording");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@RecordID", recordingId);

            return sp.ExecuteReturnValue<bool>();
        }

        public static DataTable GetDomainExistingRecordingsByEpgIds(int groupID, long domainID, List<long> epgIds)
        {
            ODBCWrapper.StoredProcedure spGetDomainExistingRecordingID = new ODBCWrapper.StoredProcedure("GetDomainExistingRecordingsByEpgIds");
            spGetDomainExistingRecordingID.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainExistingRecordingID.AddParameter("@GroupID", groupID);
            spGetDomainExistingRecordingID.AddParameter("@DomainID", domainID);
            spGetDomainExistingRecordingID.AddIDListParameter<long>("@EpgIds", epgIds, "ID");

            DataTable dt = spGetDomainExistingRecordingID.Execute();

            return dt;
        }

        public static bool UpdateOrInsertDomainRecording(int groupID, long userID, long domainID, Recording recording, long domainSeriesRecordingId = 0)
        {
            DataTable dt = null;
            bool res = false;

            ODBCWrapper.StoredProcedure spUpdateOrInsertDomainRecording = new ODBCWrapper.StoredProcedure("UpdateOrInsertDomainRecording");
            spUpdateOrInsertDomainRecording.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateOrInsertDomainRecording.AddParameter("@GroupID", groupID);
            spUpdateOrInsertDomainRecording.AddParameter("@UserID", userID);
            spUpdateOrInsertDomainRecording.AddParameter("@DomainID", domainID);
            spUpdateOrInsertDomainRecording.AddParameter("@EpgID", recording.EpgId);
            spUpdateOrInsertDomainRecording.AddParameter("@RecordingID", recording.Id);
            spUpdateOrInsertDomainRecording.AddParameter("@EpgChannelId", recording.ChannelId);
            spUpdateOrInsertDomainRecording.AddParameter("@RecordingType", (int)recording.Type);
            spUpdateOrInsertDomainRecording.AddParameter("@DomainSeriesRecordingId", domainSeriesRecordingId);
            List<TstvRecordingStatus> deleteStatus = new List<TstvRecordingStatus>() { TstvRecordingStatus.Deleted,TstvRecordingStatus.SeriesDelete };
            spUpdateOrInsertDomainRecording.AddParameter("@Status", deleteStatus.Contains(recording.RecordingStatus) ? 2 : 1);            

            switch (recording.RecordingStatus)
            {
                case TstvRecordingStatus.Canceled:
                    spUpdateOrInsertDomainRecording.AddParameter("@RecordingState",2);
                    break;
                case TstvRecordingStatus.Deleted:
                    spUpdateOrInsertDomainRecording.AddParameter("@RecordingState",3);
                    break;
                case TstvRecordingStatus.SeriesCancel:
                    spUpdateOrInsertDomainRecording.AddParameter("@RecordingState", 5);
                    break;
                case TstvRecordingStatus.SeriesDelete:
                    spUpdateOrInsertDomainRecording.AddParameter("@RecordingState", 6);
                    break;
                default:
                    spUpdateOrInsertDomainRecording.AddParameter("@RecordingState", 1);
                    break;
            }  
            dt = spUpdateOrInsertDomainRecording.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                long domainRecordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                if (domainRecordingId > 0)
                {
                    recording.Id = domainRecordingId;
                    recording.CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                    recording.UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");

                    res = true;
                }
            }

            return res;
        }

        public static DataSet GetRecordings(int groupId, List<long> recordingIds)
        {            
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Recordings");
            storedProcedure.SetConnectionKey(RECORDING_CONNECTION);
            storedProcedure.AddIDListParameter<long>("@RecordingIds", recordingIds, "Id");
            storedProcedure.AddParameter("@GroupId", groupId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();
            return dataSet;
        }        

        public static bool InsertRecordingLinks(List<RecordingLink> links, int groupId, long recordingId)
        {
            bool result = false;

            DataTable linksTable = new DataTable("recordingLinks");

            linksTable.Columns.Add("FILE_TYPE", typeof(string));
            linksTable.Columns.Add("URL", typeof(string));

            foreach (RecordingLink item in links)
            {
                DataRow row = linksTable.NewRow();
                row["FILE_TYPE"] = item.FileType;
                row["URL"] = item.Url;
                linksTable.Rows.Add(row);
            }

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Insert_RecordingLinks");
            storedProcedure.SetConnectionKey(RECORDING_CONNECTION);
            storedProcedure.AddDataTableParameter("@RecordingLinks", linksTable);
            storedProcedure.AddParameter("GroupId", groupId);
            storedProcedure.AddParameter("RecordingId", recordingId);

            result = storedProcedure.ExecuteReturnValue<bool>();

            return result;
        }

        public static DataSet GetAllRecordingsByStatuses(int groupId, List<int> statuses)
        {
            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_All_Recordings_By_Recording_Status");
            storedProcedure.SetConnectionKey(RECORDING_CONNECTION);
            storedProcedure.AddParameter("@GroupId", groupId);
            storedProcedure.AddIDListParameter<int>("@RecordingStatuses", statuses.Select(s => (int)s).ToList(), "ID");
            DataSet dataSet = storedProcedure.ExecuteDataSet();

            return dataSet;
        }

        public static DataTable GetDomainRecordingsByRecordingStatuses(int groupID, long domainID, List<int> domainRecordingStatuses)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDomainRecordings = new ODBCWrapper.StoredProcedure("GetDomainRecordingsByRecordingStatuses");
            spGetDomainRecordings.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainRecordings.AddParameter("@GroupID", groupID);
            spGetDomainRecordings.AddParameter("@DomainID", domainID);
            spGetDomainRecordings.AddIDListParameter<int>("@RecordingStatuses", domainRecordingStatuses, "ID");
            dt = spGetDomainRecordings.Execute();

            return dt;
        }

        public static DataTable GetDomainRecordingsByIds(int groupID, long domainID, List<long> domainRecordingIds)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDomainRecordingsByIds = new ODBCWrapper.StoredProcedure("GetDomainRecordingsByIds");
            spGetDomainRecordingsByIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainRecordingsByIds.AddParameter("@GroupID", groupID);
            spGetDomainRecordingsByIds.AddParameter("@DomainID", domainID);
            spGetDomainRecordingsByIds.AddIDListParameter<long>("@DomainRecordingIds", domainRecordingIds, "ID");
            dt = spGetDomainRecordingsByIds.Execute();

            return dt;
        }

        public static bool CancelDomainRecording(long recordingID, DomainRecordingStatus recordingState)
        {
            ODBCWrapper.StoredProcedure spCancelDomainRecording = new ODBCWrapper.StoredProcedure("CancelDomainRecording");
            spCancelDomainRecording.SetConnectionKey(RECORDING_CONNECTION);
            spCancelDomainRecording.AddParameter("@RecordID", recordingID);
            spCancelDomainRecording.AddParameter("@RecordingState", (int)recordingState);
            return spCancelDomainRecording.ExecuteReturnValue<bool>();
        }

        public static DataTable GetExistingDomainRecordingsByRecordingID(int groupID, long recordID, RecordingType type)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetExistingDomainRecordingsByRecordingID");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@RecordID", recordID);
            sp.AddParameter("@Type", (int)type);
            DataTable dt = sp.Execute();

            return dt;

        }

        public static bool DeleteDomainRecording(long recordingID, DomainRecordingStatus recordingState)
        {
            ODBCWrapper.StoredProcedure spDeleteDomainRecording = new ODBCWrapper.StoredProcedure("DeleteDomainRecording");
            spDeleteDomainRecording.SetConnectionKey(RECORDING_CONNECTION);
            spDeleteDomainRecording.AddParameter("@RecordID", recordingID);
            spDeleteDomainRecording.AddParameter("@RecordingState", (int)recordingState);
            return spDeleteDomainRecording.ExecuteReturnValue<bool>();
        }

        public static bool DeleteDomainRecording(List<long> domainRecordingIds, DomainRecordingStatus recordingState)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("DeleteDomainMultiRecordings");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddIDListParameter<long>("@RecordIDs", domainRecordingIds, "Id");
            sp.AddParameter("@RecordingState", (int)recordingState);
            return sp.ExecuteReturnValue<bool>();
        }

        public static DataTable GetDomainProtectedRecordings(int groupID, long domainID, long unixTimeStampNow)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDomainProtectedRecordings = new ODBCWrapper.StoredProcedure("GetDomainProtectedRecordings");
            spGetDomainProtectedRecordings.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainProtectedRecordings.AddParameter("@GroupID", groupID);
            spGetDomainProtectedRecordings.AddParameter("@DomainID", domainID);
            spGetDomainProtectedRecordings.AddParameter("@UtcNowEpoch", unixTimeStampNow);
            dt = spGetDomainProtectedRecordings.Execute();

            return dt;
        }

        public static bool ProtectRecording(long recordingId, DateTime protectedUntilDate, long protectedUntilEpoch)
        {
            bool isProtected = false;
            try
            {
                ODBCWrapper.StoredProcedure spProtectRecording = new ODBCWrapper.StoredProcedure("ProtectRecording");
                spProtectRecording.SetConnectionKey(RECORDING_CONNECTION);
                spProtectRecording.AddParameter("@ID", recordingId);
                spProtectRecording.AddParameter("@ProtectedUntilDate", protectedUntilDate);
                spProtectRecording.AddParameter("@ProtectedUntilEpoch", protectedUntilEpoch);

                isProtected = spProtectRecording.ExecuteReturnValue<bool>();
            }

            catch (Exception ex)
            {
                log.Error("Failed protecting recording when running the stored procedure: ProtectRecording", ex);
            }

            return isProtected;
        }

        public static Dictionary<long, KeyValuePair<int, Recording>> GetRecordingsForCleanup(long utcNowEpoch)
        {
            Dictionary<long, KeyValuePair<int, Recording>> recordingsForCleanup = new Dictionary<long, KeyValuePair<int, Recording>>();
            ODBCWrapper.StoredProcedure spGetRecordginsForCleanup = new ODBCWrapper.StoredProcedure("GetRecordingsForCleanup");
            spGetRecordginsForCleanup.SetConnectionKey(RECORDING_CONNECTION);            

            DataTable dt = spGetRecordginsForCleanup.Execute();
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int groupID = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID", 0);
                    string externalRecordingID = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                    long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID", 0);
                    long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "id", 0);
                    long channelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID");
                    string crid = ODBCWrapper.Utils.GetSafeStr(dr, "CRID");
                    DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                    if (groupID > 0 && recordingId > 0 && epgId > 0 && !string.IsNullOrEmpty(externalRecordingID) && !recordingsForCleanup.ContainsKey(recordingId))
                    {
                        KeyValuePair<int, Recording> pair = new KeyValuePair<int, Recording>(groupID, new Recording() { Id = recordingId, ExternalRecordingId = externalRecordingID, EpgId = epgId,
                                                                                                                        ChannelId = channelId, Crid = crid, EpgEndDate = endDate });
                        recordingsForCleanup.Add(recordingId, pair);
                    }
                }
            }

            return recordingsForCleanup;
        }

        public static int InsertExpiredRecordingsTasks(long unixTimeStampNow)
        {
            int impactedItems = -1;
            ODBCWrapper.StoredProcedure spInsertExpiredRecordingsTasks = new ODBCWrapper.StoredProcedure("InsertExpiredRecordingsTasks");
            spInsertExpiredRecordingsTasks.SetConnectionKey(RECORDING_CONNECTION);
            spInsertExpiredRecordingsTasks.AddParameter("@UtcNowEpoch", unixTimeStampNow);

            impactedItems = spInsertExpiredRecordingsTasks.ExecuteReturnValue<int>();

            return impactedItems;
        }

        public static DataTable UpdateAndGetExpiredRecordingsTasks(long unixTimeStampNow)
        {
            ODBCWrapper.StoredProcedure spUpdateAndGetExpiredRecordingsTasks = new ODBCWrapper.StoredProcedure("UpdateAndGetExpiredRecordingsTasks");
            spUpdateAndGetExpiredRecordingsTasks.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateAndGetExpiredRecordingsTasks.AddParameter("@UtcNowEpoch", unixTimeStampNow);

            return spUpdateAndGetExpiredRecordingsTasks.Execute();
        }

        public static DataTable UpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate(long recordingId, long unixTimeStampNow, int status, DomainRecordingStatus domainRecordingStatus, long maxDomainRecordingId)
        {
            ODBCWrapper.StoredProcedure spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate = new ODBCWrapper.StoredProcedure("UpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate");
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.AddParameter("@RecordingId", recordingId);
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.AddParameter("@UtcNowEpoch", unixTimeStampNow);
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.AddParameter("@Status", status);
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.AddParameter("@DomainRecordingStatus", (int)domainRecordingStatus);
            spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.AddParameter("@MaxDomainRecordingId", maxDomainRecordingId);

            return spUpdateAndGetDomainsRecordingsByRecordingIdAndProtectDate.Execute();
        }

        public static long GetRecordingMinProtectedEpoch(long recordingId, long unixTimeStampNow)
        {
            long minProtectedUntilEpoch = -1;
            ODBCWrapper.StoredProcedure spGetRecordingMinProtectedEpoch = new ODBCWrapper.StoredProcedure("GetRecordingMinProtectedEpoch");
            spGetRecordingMinProtectedEpoch.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingMinProtectedEpoch.AddParameter("@RecordingId", recordingId);
            spGetRecordingMinProtectedEpoch.AddParameter("@UtcNowEpoch", unixTimeStampNow);

            DataTable dt = spGetRecordingMinProtectedEpoch.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                minProtectedUntilEpoch = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "minProtectedUntilEpoch", 0);
            }

            return minProtectedUntilEpoch;
        }

        public static bool InsertExpiredRecordingNextTask(long recordingId, int groupId, long minProtectionEpoch, DateTime minProtectionDate)
        {
            ODBCWrapper.StoredProcedure spInsertExpiredRecordingNextTask = new ODBCWrapper.StoredProcedure("InsertExpiredRecordingNextTask");
            spInsertExpiredRecordingNextTask.SetConnectionKey(RECORDING_CONNECTION);
            spInsertExpiredRecordingNextTask.AddParameter("@RecordingId", recordingId);
            spInsertExpiredRecordingNextTask.AddParameter("@GroupId", groupId);
            spInsertExpiredRecordingNextTask.AddParameter("@MinProtectionEpoch", minProtectionEpoch);
            spInsertExpiredRecordingNextTask.AddParameter("@MinProtectionDate", minProtectionDate);

            return spInsertExpiredRecordingNextTask.ExecuteReturnValue<bool>();
        }

        public static bool UpdateExpiredRecordingAfterScheduledTask(long id)
        {
            ODBCWrapper.StoredProcedure spUpdateExpiredRecordingAfterScheduledTask = new ODBCWrapper.StoredProcedure("UpdateExpiredRecordingAfterScheduledTask");
            spUpdateExpiredRecordingAfterScheduledTask.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateExpiredRecordingAfterScheduledTask.AddParameter("@Id", id);

            return spUpdateExpiredRecordingAfterScheduledTask.ExecuteReturnValue<bool>();
        }

        public static DataTable GetEpgToRecordingsMapByCridChannelAndEpgId(int groupId, string crid, long channelId, long epgId)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetEpgToRecordingsMapByCrid = new ODBCWrapper.StoredProcedure("GetRecordingsByCridChannelAndEpgId");
            spGetEpgToRecordingsMapByCrid.SetConnectionKey(RECORDING_CONNECTION);
            spGetEpgToRecordingsMapByCrid.AddParameter("@GroupID", groupId);
            spGetEpgToRecordingsMapByCrid.AddParameter("@Crid", crid);
            spGetEpgToRecordingsMapByCrid.AddParameter("@ChannelId", channelId);
            spGetEpgToRecordingsMapByCrid.AddParameter("@EpgId", epgId);
            dt = spGetEpgToRecordingsMapByCrid.Execute();

            return dt;
        }

        public static DataTable FollowSeries(int groupId, string userId, long domainID, long epgId, long epgChannelId, string seriesId, int seasonNumber, int episodeNumber)
        {
            ODBCWrapper.StoredProcedure spFollowSeries = new ODBCWrapper.StoredProcedure("FollowSeries");
            spFollowSeries.SetConnectionKey(RECORDING_CONNECTION);
            spFollowSeries.AddParameter("@GroupID", groupId);
            spFollowSeries.AddParameter("@UserID", userId);
            spFollowSeries.AddParameter("@DomainID", domainID);
            spFollowSeries.AddParameter("@EpgID", epgId);
            spFollowSeries.AddParameter("@EpgChannelID", epgChannelId);
            spFollowSeries.AddParameter("@SeriesID", seriesId);
            spFollowSeries.AddParameter("@SeasonNumber", seasonNumber);
            spFollowSeries.AddParameter("@EpisodeNumber", episodeNumber);
            DataTable dt = spFollowSeries.Execute();

            return dt;
        }

        public static bool IsSeriesFollowed(int groupId, string seriesId, int seasonNumber, long channelId)
        {
            ODBCWrapper.StoredProcedure spIsSeriesFollowed = new ODBCWrapper.StoredProcedure("IsSeriesFollowed");
            spIsSeriesFollowed.SetConnectionKey(RECORDING_CONNECTION);
            spIsSeriesFollowed.AddParameter("@GroupID", groupId);            
            spIsSeriesFollowed.AddParameter("@SeriesId", seriesId);
            spIsSeriesFollowed.AddParameter("@SeasonNumber", seasonNumber);
            spIsSeriesFollowed.AddParameter("@ChannelId", channelId);

            int rowsFound = spIsSeriesFollowed.ExecuteReturnValue<int>();

            return rowsFound > 0;
        }

        public static long GetDomainSeriesId(int groupId, long domainID, string seriesId, int seasonNumber, long channelId)
        {
            long domainSeriesId = 0;
            ODBCWrapper.StoredProcedure spIsFollowingSeries = new ODBCWrapper.StoredProcedure("GetDomainSeriesId");            
            spIsFollowingSeries.SetConnectionKey(RECORDING_CONNECTION);
            spIsFollowingSeries.AddParameter("@GroupID", groupId);            
            spIsFollowingSeries.AddParameter("@DomainId", domainID);
            spIsFollowingSeries.AddParameter("@SeriesId", seriesId);
            spIsFollowingSeries.AddParameter("@SeasonNumber", seasonNumber);
            spIsFollowingSeries.AddParameter("@ChannelId", channelId);

            domainSeriesId = spIsFollowingSeries.ExecuteReturnValue<long>();

            return domainSeriesId;
        }        

        public static DataSet GetDomainSeriesRecordings(int groupId, long domainId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DomainSeries");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@domainId", domainId);

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        public static Dictionary<long, long> GetEpgToRecordingsMap(int groupId, List<long> recordingIds)
        {
            Dictionary<long, long> epgsToRecordingsMap = new Dictionary<long, long>();

            ODBCWrapper.StoredProcedure spGetEpgsByRecordingIds = new ODBCWrapper.StoredProcedure("GetEpgsByRecordingIds");
            spGetEpgsByRecordingIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetEpgsByRecordingIds.AddParameter("@GroupID", groupId);
            spGetEpgsByRecordingIds.AddIDListParameter<int>("@RecordingIds", recordingIds.Select(s => (int)s).ToList(), "ID");

            DataTable dt = spGetEpgsByRecordingIds.Execute();
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID", 0);

                    if (recordingId > 0 && epgId > 0)
                    {
                        // If not contained already, add to dictionary
                        if (!epgsToRecordingsMap.ContainsKey(epgId))
                        {
                            epgsToRecordingsMap.Add(epgId, recordingId);
                        }
                        else
                        {
                            // Otherwise only update if recording is newer...
                            var existingRecordingId = epgsToRecordingsMap[epgId];

                            if (recordingId > existingRecordingId)
                            {
                                epgsToRecordingsMap[epgId] = recordingId;
                            }
                        }
                    }
                }
            }

            return epgsToRecordingsMap;
        }

        public static Dictionary<long, long> GetEpgToRecordingsMapByRecordingStatuses(int groupId, List<int> recordingStatuses)
        {
            Dictionary<long, long> epgsToRecordingsMap = new Dictionary<long, long>();

            ODBCWrapper.StoredProcedure spGetEpgsByRecordingStatus = new ODBCWrapper.StoredProcedure("GetEpgsByRecordingStatus");
            spGetEpgsByRecordingStatus.SetConnectionKey(RECORDING_CONNECTION);
            spGetEpgsByRecordingStatus.AddParameter("@GroupID", groupId);
            spGetEpgsByRecordingStatus.AddIDListParameter<int>("@RecordingStatuses", recordingStatuses, "ID");

            DataTable dt = spGetEpgsByRecordingStatus.Execute();

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID", 0);

                    if (recordingId > 0 && epgId > 0)
                    {
                        // If not contained already, add to dictionary
                        if (!epgsToRecordingsMap.ContainsKey(epgId))
                        {
                            epgsToRecordingsMap.Add(epgId, recordingId);
                        }
                        else
                        {
                            // Otherwise only update if recording is newer...
                            var existingRecordingId = epgsToRecordingsMap[epgId];

                            if (recordingId > existingRecordingId)
                            {
                                epgsToRecordingsMap[epgId] = recordingId;
                            }
                        }
                    }
                }
            }

            return epgsToRecordingsMap;
        }

        public static DataSet GetDomainSeriesRecordingsById(int groupId, long domainId, long domainSeriesRecordingId)
        {
            DataSet ds = null;
            ODBCWrapper.StoredProcedure spGetDomainRecordingsByIds = new ODBCWrapper.StoredProcedure("GetDomainSeriesRecordingsById");
            spGetDomainRecordingsByIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainRecordingsByIds.AddParameter("@GroupID", groupId);
            spGetDomainRecordingsByIds.AddParameter("@DomainID", domainId);
            spGetDomainRecordingsByIds.AddParameter("@DomainRecordingId", domainSeriesRecordingId);
            ds = spGetDomainRecordingsByIds.ExecuteDataSet();

            return ds;
        }

        public static bool CancelSeriesRecording(long Id)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelSeriesRecording");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@Id", Id);

            return sp.ExecuteReturnValue<bool>(); 
        }

        public static DataTable GetSeriesFollowingDomains(int groupId, string seriesId, int seasonNumber, long maxDomainSeriesId)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetSeriesFollowingDomains = new ODBCWrapper.StoredProcedure("GetSeriesFollowingDomains");
            spGetSeriesFollowingDomains.SetConnectionKey(RECORDING_CONNECTION);
            spGetSeriesFollowingDomains.AddParameter("@GroupID", groupId);
            spGetSeriesFollowingDomains.AddParameter("@SeriesId", seriesId);
            spGetSeriesFollowingDomains.AddParameter("@SeasonNumber", seasonNumber);
            spGetSeriesFollowingDomains.AddParameter("@MaxId", maxDomainSeriesId);
            dt = spGetSeriesFollowingDomains.Execute();

            return dt;
        }

        public static DataTable GetSeriesFollowingDomainsByIds(string ids)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetSeriesFollowingDomainsByIds = new ODBCWrapper.StoredProcedure("GetSeriesFollowingDomainsByIds");
            spGetSeriesFollowingDomainsByIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetSeriesFollowingDomainsByIds.AddParameter("@ListIds", ids);
            dt = spGetSeriesFollowingDomainsByIds.Execute();

            return dt;
        }

        public static HashSet<long> GetSeriesFollowingDomainsIds(int groupId, string seriesId, int seasonNumber, ref long maxDomainSeriesId)
        {
            HashSet<long> domainSeriesIds = new HashSet<long>();
            ODBCWrapper.StoredProcedure spGetSeriesFollowingDomainsIds = new ODBCWrapper.StoredProcedure("GetSeriesFollowingDomainsIds");
            spGetSeriesFollowingDomainsIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetSeriesFollowingDomainsIds.AddParameter("@GroupID", groupId);
            spGetSeriesFollowingDomainsIds.AddParameter("@SeriesId", seriesId);
            spGetSeriesFollowingDomainsIds.AddParameter("@SeasonNumber", seasonNumber);
            spGetSeriesFollowingDomainsIds.AddParameter("@MaxId", maxDomainSeriesId);
            DataTable dt = spGetSeriesFollowingDomainsIds.Execute();

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long domainSeriesId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    if (domainSeriesId > 0 && !domainSeriesIds.Contains(domainSeriesId))
                    {
                        domainSeriesIds.Add(domainSeriesId);
                    }
                }

                maxDomainSeriesId = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[dt.Rows.Count - 1], "ID", -1);
            }

            return domainSeriesIds;
        }

        public static RecordingLink GetRecordingLinkByFileType(int groupId, long externalRecordingId, string fileType)
        {
            RecordingLink recordingLink = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetRecordingLinksByFileType");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@recordingId", externalRecordingId);
            sp.AddParameter("@fileType", fileType);

            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null)
            {
                recordingLink = new RecordingLink()
                {
                    FileType = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "FILE_TYPE"),
                    Url = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "URL")
                };
            }
            return recordingLink;
        }

        #region Couchbase

        public static RecordingCB GetRecordingByProgramId_CB(long programId)
        {
            RecordingCB result = null;

            CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);

            result = client.Get<RecordingCB>(programId.ToString());
            return result;
        }

        public static void UpdateRecording_CB(RecordingCB recording)
        {
            if (recording != null)
            {
                CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);

                bool result = client.Set<RecordingCB>(recording.EpgID.ToString(), recording);

                if (!result)
                {
                    log.ErrorFormat("Failed updating recording in Couchbase. Recording id = {0}, EPG ID = {1}", recording.RecordingId, recording.EpgID);
                }
            }
        }

        public static void DeleteRecording_CB(RecordingCB recording)
        {
            if (recording != null)
            {
                CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);

                bool result = client.Remove(recording.EpgID.ToString());

                if (!result)
                {
                    log.ErrorFormat("Failed removing recording in Couchbase. Recording id = {0}, EPG ID = {1}", recording.RecordingId, recording.EpgID);
                }
            }
        }

        /// <summary>
        /// DO NOT DIRECTLY USE THIS FUNCTION, USE QuotaManager.GetDomainQuota
        /// </summary>        
        public static bool GetDomainQuota(int groupId, long domainId, out DomainQuota quota, int defaultTotal)
        {
            bool result = false;
            object quotaObj;
            quota = null;
            
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
            if (string.IsNullOrEmpty(domainQuotaKey))
            {
                log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
            }
            else
            {
                try
                {
                    int numOfRetries = 0;
                    while (numOfRetries < limitRetries)
                    {
                        quotaObj = cbClient.Get<object>(domainQuotaKey, out getResult);

                        if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            log.DebugFormat("domain: {0} does not have a quota document with key: {1}", domainId, domainQuotaKey);
                            break;
                        }
                        else if (getResult == Couchbase.IO.ResponseStatus.Success)
                        {
                            Type type = quotaObj.GetType();
                            if (type != null)
                            {
                                if (quotaObj.GetType() == typeof(Int64))
                                {
                                    int availableQuota = JsonConvert.DeserializeObject<int>(quotaObj.ToString());

                                    quota = new DomainQuota(0, defaultTotal - availableQuota, true);
                                    result = SetDomainQuota(domainId, quota); // insert to cb total quota + used as OBJECT
                                }
                                else //JObject
                                {
                                    quota = JsonConvert.DeserializeObject<DomainQuota>(quotaObj.ToString());
                                    if (quota.Total == 0)
                                    {
                                        quota.IsDefaultQuota = true;
                                        quota.Total = defaultTotal;
                                    }
                                    else
                                    {
                                        quota.IsDefaultQuota = false;
                                    }
                                    result = true;
                                }
                            }
                            break;
                        }
                        else
                        {
                            log.ErrorFormat("Retrieving domain quota with domainId: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", domainId, domainQuotaKey, getResult, numOfRetries, limitRetries);
                            numOfRetries++;
                            System.Threading.Thread.Sleep(r.Next(50));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get domain quota, domainId: {0}, ex: {1}", domainId, ex);
                }
            }

            return result;
        }


        /// <summary>
        /// DO NOT DIRECTLY USE THIS FUNCTION, USE QuotaManager.IncreaseDomainQuota
        /// update domain quota 
        /// </summary>
        public static bool UpdateDomainQuota(long domainId, DomainQuota domainQuota)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
            if (string.IsNullOrEmpty(domainQuotaKey))
            {
                log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
                return result;
            }

            if (domainQuota.IsDefaultQuota)
                domainQuota.Total = 0;

            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;                   
                    Couchbase.IO.ResponseStatus status;
                    DomainQuota quota = cbClient.GetWithVersion<DomainQuota>(domainQuotaKey, out version, out status); // get the domain quota from CB only for version issue
                    if (status == Couchbase.IO.ResponseStatus.Success || status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, version);
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while adding quota to domain. number of tries: {0}/{1}. domainId: {2}, domainQuota : {3}", numOfRetries, limitRetries, domainId, domainQuota.ToString());
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding quota to domain, domainId: {0}, domainQuota: {1}, ex: {2}", domainId, domainQuota.ToString(), ex);
            }

            return result;
        }

        ///// <summary>
        ///// DO NOT DIRECTLY USE THIS FUNCTION, USE QuotaManager.IncreaseDomainQuota
        ///// reduce quota to the used quota mean total quota increased
        ///// </summary>
        //public static bool IncreaseDomainQuota(long domainId, int quotaToIncrease)
        //{
        //    bool result = false;
        //    CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
        //    int limitRetries = RETRY_LIMIT;
        //    Random r = new Random();
        //    string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
        //    if (string.IsNullOrEmpty(domainQuotaKey))
        //    {
        //        log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
        //        return result;
        //    }

        //    try
        //    {
        //        int numOfRetries = 0;
        //        while (!result && numOfRetries < limitRetries)
        //        {
        //            ulong version;
        //            DomainQuota domainQuota;
        //            Couchbase.IO.ResponseStatus status;
        //            domainQuota = cbClient.GetWithVersion<DomainQuota>(domainQuotaKey, out version, out status);
        //            if (status == Couchbase.IO.ResponseStatus.Success)
        //            {
        //                domainQuota.Used -= quotaToIncrease;
        //                result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, version);
        //            }

        //            if (!result)
        //            {
        //                numOfRetries++;
        //                log.ErrorFormat("Error while adding quota to domain. number of tries: {0}/{1}. domainId: {2}, quotaToIncrease: {3}", numOfRetries, limitRetries, domainId, quotaToIncrease);
        //                System.Threading.Thread.Sleep(r.Next(50));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while adding quota to domain, domainId: {0}, quotaToIncrease: {1}, ex: {2}", domainId, quotaToIncrease, ex);
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// DO NOT DIRECTLY USE THIS FUNCTION, USE QuotaManager.DecreaseDomainQuota
        ///// adding quota to the used quota mean total quota decreased
        ///// </summary>
        //public static bool DecreaseDomainQuota(long domainId, int quotaToDecrease, int totalDomainQuota)
        //{
        //    bool result = false;
        //    CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
        //    int limitRetries = RETRY_LIMIT;
        //    Random r = new Random();
        //    string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
        //    if (string.IsNullOrEmpty(domainQuotaKey))
        //    {
        //        log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
        //        return result;
        //    }

        //    try
        //    {
        //        int numOfRetries = 0;
        //        while (!result && numOfRetries < limitRetries)
        //        {
        //            ulong version;
        //            DomainQuota domainQuota;
                    
        //            Couchbase.IO.ResponseStatus status;
        //            domainQuota = cbClient.GetWithVersion<DomainQuota>(domainQuotaKey, out version, out status);
        //            if (status == Couchbase.IO.ResponseStatus.Success)
        //            {
                        
        //                domainQuota.Used += quotaToDecrease;
        //                domainQuota.Total = totalDomainQuota;

        //                result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, version);
        //            }
        //            else if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
        //            {
        //                domainQuota = new DomainQuota()
        //                {
        //                    Total = totalDomainQuota,
        //                    Used = quotaToDecrease
        //                };

        //                result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, 0);
        //            }

        //            if (!result)
        //            {
        //                numOfRetries++;
        //                log.ErrorFormat("Error while decreasing quota to domain. number of tries: {0}/{1}. domainId: {2},  quotaToDecrease: {4}", numOfRetries, limitRetries, domainId, quotaToDecrease);
        //                System.Threading.Thread.Sleep(r.Next(50));
        //            }
        //            else
        //            {
        //                log.DebugFormat("successfully updated domain quota to {0}. domainId = {1}", domainQuota.Used, domainId);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while decreasing quota to domain, domainId: {0}, quotaToDecrease: {1}, ex: {2}", domainId, quotaToDecrease, ex);
        //    }           

        //    return result;
        //}

        public static bool InsertFirstFollowerLock(int groupId, string seriesId, int seasonNumber, string channelId, bool isBlockLock)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string firstFollowerLockKey = UtilsDal.GetFirstFollowerLockKey(groupId, seriesId, seasonNumber, channelId);
            if (string.IsNullOrEmpty(firstFollowerLockKey))
            {
                log.ErrorFormat("Failed getting firstFollowerLockKey for groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}, isBlockLock: {4}", groupId, seriesId, seasonNumber, channelId, isBlockLock);
                return result;
            }

            try
            {
                uint ttl = isBlockLock ? FIRST_FOLLOWER_BLOCK_LOCK_TTL_SEC : FIRST_FOLLOWER_INDEX_LOCK_TTL_SEC;
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    result = cbClient.Set<long>(firstFollowerLockKey, ODBCWrapper.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow), ttl);                    
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while InsertFirstFollowerLock. number of tries: {0}/{1}. groupId: {2}, seriesId: {3}, seasonNumber: {4}, channelId: {5}, isBlockLock: {6}",
                                        numOfRetries, limitRetries, groupId, seriesId, seasonNumber, channelId, isBlockLock);
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on InsertFirstFollowerLock, groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}, isBlockLock: {4}",
                                            groupId, seriesId, seasonNumber, channelId, isBlockLock), ex);
            }

            return result;
        }

        public static bool IsFirstFollowerLockExists(int groupId, string seriesId, int seasonNumber, string channelId)
        {
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string firstFollowerLockKey = UtilsDal.GetFirstFollowerLockKey(groupId, seriesId, seasonNumber, channelId);
            if (string.IsNullOrEmpty(firstFollowerLockKey))
            {
                log.ErrorFormat("Failed getting firstFollowerLockKey for groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}", groupId, seriesId, seasonNumber, channelId);
                return true;
            }

            try
            {                
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {                    
                    Couchbase.IO.ResponseStatus cbResponse;
                    long lockTime = 0;
                    lockTime = cbClient.Get<long>(firstFollowerLockKey, out cbResponse);
                    if (cbResponse != Couchbase.IO.ResponseStatus.KeyNotFound && cbResponse != Couchbase.IO.ResponseStatus.Success)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while GetFirstFollowerLock. number of tries: {0}/{1}. groupId: {2}, seriesId: {3}, seasonNumber: {4}, channelId: {5}",
                                        numOfRetries, limitRetries, groupId, seriesId, seasonNumber, channelId);
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                    // if document exists return true
                    else if (lockTime > 0)
                    {
                        return true;
                    }
                    // if document does not exists return false
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on GetFirstFollowerLock, groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}",
                                            groupId, seriesId, seasonNumber, channelId), ex);
                return true;
            }            
        }

        public static bool DeleteFirstFollowerLock(int groupId, string seriesId, int seasonNumber, string channelId)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string firstFollowerLockKey = UtilsDal.GetFirstFollowerLockKey(groupId, seriesId, seasonNumber, channelId);
            if (string.IsNullOrEmpty(firstFollowerLockKey))
            {
                log.ErrorFormat("Failed getting firstFollowerLockKey for groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}", groupId, seriesId, seasonNumber, channelId);
                return result;
            }

            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    result = cbClient.Remove(firstFollowerLockKey);
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while InsertFirstFollowerLock. number of tries: {0}/{1}. groupId: {2}, seriesId: {3}, seasonNumber: {4}, channelId: {5}",
                                        numOfRetries, limitRetries, groupId, seriesId, seasonNumber, channelId);
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on InsertFirstFollowerLock, groupId: {0}, seriesId: {1}, seasonNumber: {2}, channelId: {3}",
                                            groupId, seriesId, seasonNumber, channelId), ex);
            }

            return result;
        }

        #endregion

        public static DataTable GetDomainsRecordingsByRecordingStatusesAndChannel(List<long> domains, int groupId, long channelId, List<int> statuses, int recordingType, DateTime epgStartDate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetDomainsRecordingsByRecordingStatusesAndChannel");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@ChannelID", channelId);
            sp.AddParameter("@Type", recordingType);
            sp.AddParameter("@StartDate", epgStartDate);
            sp.AddIDListParameter<long>("@DomainIDs", domains, "ID");
            sp.AddIDListParameter<int>("@RecordingStatuses", statuses, "ID");
            
            DataTable dt = sp.Execute();
            return dt;
        }

        public static bool InsertOrUpdateDomainSeriesExclude(int groupId, long domainId, string userId, long domainSeriesRecordingId, long seasonNumber, int status = 1)
        {
            bool result = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertOrUpdateDomainSeriesExclude");
                
                sp.SetConnectionKey(RECORDING_CONNECTION);
                sp.AddParameter("@DomainSeriesRecordingID", domainSeriesRecordingId);               
                sp.AddParameter("@SeasonNumber", seasonNumber);
                sp.AddParameter("@Status", status);

                result = sp.ExecuteReturnValue<bool>();
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static DataTable GetFutureDomainRecordingsByRecordingIDs(int groupID, long domainID, List<long> recordingIds, RecordingType recordingType)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetFutureDomainRecordingsByRecordingIDs = new ODBCWrapper.StoredProcedure("GetFutureDomainRecordingsByRecordingIDs");
            spGetFutureDomainRecordingsByRecordingIDs.SetConnectionKey(RECORDING_CONNECTION);
            spGetFutureDomainRecordingsByRecordingIDs.AddParameter("@GroupID", groupID);
            spGetFutureDomainRecordingsByRecordingIDs.AddParameter("@DomainID", domainID);
            spGetFutureDomainRecordingsByRecordingIDs.AddParameter("@RecordingType", (int)recordingType);
            spGetFutureDomainRecordingsByRecordingIDs.AddIDListParameter<long>("@RecordingIDs", recordingIds, "ID");
            dt = spGetFutureDomainRecordingsByRecordingIDs.Execute();

            return dt;
        }

        public static HashSet<string> GetDomainRecordingsCridsByDomainsSeriesIds(int groupID, long domainID, List<long> domainSeriesIds, string specificCrid = null)
        {
            HashSet<string> crids = new HashSet<string>();
            DataTable dt = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetDomainRecordingsCridsByDomainsSeriesIds");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@DomainID", domainID);
            sp.AddIDListParameter<long>("@DomainsSeriesIds", domainSeriesIds, "ID");
            if (!string.IsNullOrEmpty(specificCrid))
            {
                sp.AddParameter("@SpecificCrid", specificCrid);
            }

            dt = sp.Execute();

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {                
                foreach (DataRow row in dt.Rows)
                {
                    string cridToAdd = ODBCWrapper.Utils.GetSafeStr(row, "CRID");
                    if (!crids.Contains(cridToAdd))
                    {
                        crids.Add(cridToAdd);
                    }
                }
            }

            return crids;
        }

        public static DataTable GetRecordingsByExternalRecordingId(int groupId, string externalRecordingId)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetEpgToRecordingsMapByExternalRecordingId = new ODBCWrapper.StoredProcedure("GetRecordingsByExternalRecordingId");
            spGetEpgToRecordingsMapByExternalRecordingId.SetConnectionKey(RECORDING_CONNECTION);
            spGetEpgToRecordingsMapByExternalRecordingId.AddParameter("@GroupID", groupId);
            spGetEpgToRecordingsMapByExternalRecordingId.AddParameter("@ExternalRecordingId", externalRecordingId);
            dt = spGetEpgToRecordingsMapByExternalRecordingId.Execute();

            return dt;
        }

        public static DataTable GetDomainRecordingsByDomainSeriesId(int groupID, long domainID, long domainSeriesRecordingId)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDomainRecordingsByDomainSeriesId = new ODBCWrapper.StoredProcedure("GetDomainRecordingsByDomainSeriesId");
            spGetDomainRecordingsByDomainSeriesId.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainRecordingsByDomainSeriesId.AddParameter("@GroupId", groupID);
            spGetDomainRecordingsByDomainSeriesId.AddParameter("@DomainId", domainID);
            spGetDomainRecordingsByDomainSeriesId.AddParameter("@DomainSeriesRecordingId", domainSeriesRecordingId);            
            dt = spGetDomainRecordingsByDomainSeriesId.Execute();

            return dt;
        }


        public static bool UpdateDomainSeriesRecordingsUserId(int groupId, List<long> domainSeriesIdsToUpdate, string masterUserId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateDomainSeriesRecordingsUserId");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@domainSeriesIds", domainSeriesIdsToUpdate, "ID");
            sp.AddParameter("@UserId", masterUserId);
            int res = sp.ExecuteReturnValue<int>();

            return res > 0;
        }

        public static bool UpdateDomainScheduledRecordingsUserId(int groupId, List<long> domainSceduledIdsToUpdate, string masterUserId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateDomainScheduledRecordingsUserId");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@GroupId", groupId);
            sp.AddIDListParameter("@domainRecordingIds", domainSceduledIdsToUpdate, "ID");
            sp.AddParameter("@UserId", masterUserId);
            int res = sp.ExecuteReturnValue<int>();

            return res > 0;
        }
        
        public static bool SetDomainQuota(long domainId, DomainQuota domainQuota)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
            if (string.IsNullOrEmpty(domainQuotaKey))
            {
                log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
                return result;
            }

            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;                    
                    Couchbase.IO.ResponseStatus status;

                    object quotaObj = cbClient.GetWithVersion<object>(domainQuotaKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success)
                    {   
                        result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, version);
                    }
                    else if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, 0);
                    }
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while set domain quota. number of tries: {0}/{1}. domainId: {2}, domainQuota: {3}", numOfRetries, limitRetries, domainId, domainQuota.ToString());
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while set quota to domain, domainId: {0}, domainQuota: {1}, ex: {2}", domainId, domainQuota.ToString(), ex);
            }

            return result;
        }

        public static bool UpdateDomainUsedQuota(long domainId, int quota, int defaultQuota, bool shouldForceUpdate = true)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string domainQuotaKey = UtilsDal.GetDomainQuotaKey(domainId);
            if (string.IsNullOrEmpty(domainQuotaKey))
            {
                log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
                return result;
            }
            
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    DomainQuota domainQuota = cbClient.GetWithVersion<DomainQuota>(domainQuotaKey, out version, out status); // get the domain quota from CB only for version issue
                    log.DebugFormat("after GetWithVersion domainQuota: {0}, status:{1}", domainQuota != null ? domainQuota.ToString() : "null", status.ToString());
                    if (status == Couchbase.IO.ResponseStatus.Success)
                    {
                        int total = domainQuota.Total == 0 ? defaultQuota : domainQuota.Total;
                        if (shouldForceUpdate || total - domainQuota.Used >= quota)
                        {
                            domainQuota.Used += quota;
                            result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, version);
                        }
                    }
                    else if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        if (shouldForceUpdate || defaultQuota >= quota)
                        {
                            domainQuota = new DomainQuota(0, Math.Max(quota, 0), true);
                            result = cbClient.SetWithVersion<DomainQuota>(domainQuotaKey, domainQuota, 0);
                        }
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while adding quota to domain. number of tries: {0}/{1}. domainId: {2}, domainQuota : {3}", numOfRetries, limitRetries, domainId, domainQuota.ToString());
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding quota to domain, domainId: {0}, quota: {1}, ex: {2}", domainId, quota, ex);
            }

            return result;
        }
    }
}
