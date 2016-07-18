using ApiObjects;
using ApiObjects.ScheduledTasks;
using ApiObjects.TimeShiftedTv;
using KLogMonitor;
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

        public static DataTable GetRecordingById(long id)
        {
            ODBCWrapper.StoredProcedure spGetRecordingById = new ODBCWrapper.StoredProcedure("GetRecordingById");
            spGetRecordingById.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingById.AddParameter("@Id", id);
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

        public static DataTable GetDomainExistingRecordingsByEpdIgs(int groupID, long domainID, List<long> epgIds)
        {
            ODBCWrapper.StoredProcedure spGetDomainExistingRecordingID = new ODBCWrapper.StoredProcedure("GetDomainExistingRecordingsByEpdIgs");
            spGetDomainExistingRecordingID.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainExistingRecordingID.AddParameter("@GroupID", groupID);
            spGetDomainExistingRecordingID.AddParameter("@DomainID", domainID);
            spGetDomainExistingRecordingID.AddIDListParameter<long>("@EpgIds", epgIds, "ID");

            DataTable dt = spGetDomainExistingRecordingID.Execute();

            return dt;
        }

        public static bool UpdateOrInsertDomainRecording(int groupID, long userID, long domainID, Recording recording)
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

            linksTable.Columns.Add("BRAND_ID", typeof(int));
            linksTable.Columns.Add("URL", typeof(string));

            foreach (RecordingLink item in links)
            {
                DataRow row = linksTable.NewRow();
                row["BRAND_ID"] = item.DeviceTypeBrand;
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

        public static bool CancelDomainRecording(long recordingID)
        {
            ODBCWrapper.StoredProcedure spCancelDomainRecording = new ODBCWrapper.StoredProcedure("CancelDomainRecording");
            spCancelDomainRecording.SetConnectionKey(RECORDING_CONNECTION);
            spCancelDomainRecording.AddParameter("@RecordID", recordingID);

            return spCancelDomainRecording.ExecuteReturnValue<bool>();
        }

        public static DataTable GetExistingRecordingsByRecordingID(int groupID, long recordID)
        {
            ODBCWrapper.StoredProcedure spGetExistingRecordingsByRecordingID = new ODBCWrapper.StoredProcedure("GetExistingRecordingsByRecordingID");
            spGetExistingRecordingsByRecordingID.SetConnectionKey(RECORDING_CONNECTION);
            spGetExistingRecordingsByRecordingID.AddParameter("@GroupID", groupID);
            spGetExistingRecordingsByRecordingID.AddParameter("@RecordID", recordID);
            DataTable dt = spGetExistingRecordingsByRecordingID.Execute();

            return dt;

        }

        public static bool DeleteDomainRecording(long recordingID)
        {
            ODBCWrapper.StoredProcedure spDeleteDomainRecording = new ODBCWrapper.StoredProcedure("DeleteDomainRecording");
            spDeleteDomainRecording.SetConnectionKey(RECORDING_CONNECTION);
            spDeleteDomainRecording.AddParameter("@RecordID", recordingID);

            return spDeleteDomainRecording.ExecuteReturnValue<bool>();
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
            spGetRecordginsForCleanup.AddParameter("@UtcNowEpoch", utcNowEpoch);

            DataTable dt = spGetRecordginsForCleanup.Execute();
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int groupID = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID", 0);
                    string externalRecordingID = ODBCWrapper.Utils.GetSafeStr(dr, "EXTERNAL_RECORDING_ID");
                    long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID", 0);
                    long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "id", 0);
                    if (groupID > 0 && recordingId > 0 && epgId > 0 && !string.IsNullOrEmpty(externalRecordingID) && !recordingsForCleanup.ContainsKey(recordingId))
                    {
                        KeyValuePair<int, Recording> pair = new KeyValuePair<int, Recording>(groupID, new Recording() { Id = recordingId, ExternalRecordingId = externalRecordingID, EpgId = epgId });
                        recordingsForCleanup.Add(recordingId, pair);
                    }
                }
            }

            return recordingsForCleanup;
        }

        public static bool UpdateSuccessfulRecordingsCleanup(DateTime lastSuccessfulCleanUpDate, int deletedRecordingOnLastCleanup, int domainRecordingsUpdatedOnLastCleanup, int intervalInMinutes)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string recordingsCleanupKey = UtilsDal.GetRecordingsCleanupKey();
            try
            {
                int numOfRetries = 0;
                RecordingCleanupResponse recordingCleanupDetails = new RecordingCleanupResponse(lastSuccessfulCleanUpDate, deletedRecordingOnLastCleanup, domainRecordingsUpdatedOnLastCleanup, intervalInMinutes);
                while (!result && numOfRetries < limitRetries)
                {
                    result = cbClient.Set(recordingsCleanupKey, recordingCleanupDetails);
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating successful recordings cleanup date. number of tries: {0}/{1}. RecordingCleanupResponse: {2}",
                                         numOfRetries, limitRetries, recordingCleanupDetails.ToString());

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating successful recordings cleanup date: {0}, ex: {1}", lastSuccessfulCleanUpDate, ex);
            }

            return result;
        }

        public static int UpdateDomainRecordingsAfterCleanup(List<long> recordingsIds)
        {
            int updatedRowsCount = 0;
            ODBCWrapper.StoredProcedure spUpdateDomainRecordingsAfterCleanup = new ODBCWrapper.StoredProcedure("UpdateDomainRecordingsAfterCleanup");
            spUpdateDomainRecordingsAfterCleanup.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateDomainRecordingsAfterCleanup.AddIDListParameter<long>("@RecordingIds", recordingsIds, "ID");

            updatedRowsCount = spUpdateDomainRecordingsAfterCleanup.ExecuteReturnValue<int>();

            return updatedRowsCount;
        }

        public static RecordingCleanupResponse GetLastSuccessfulRecordingsCleanupDetails()
        {
            RecordingCleanupResponse response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string recordingsCleanupKey = UtilsDal.GetRecordingsCleanupKey();
            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<RecordingCleanupResponse>(recordingsCleanupKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        response = new RecordingCleanupResponse() { Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, "CouchBase KeyNotFound") };
                        log.ErrorFormat("Error while trying to get last successful recordings cleanup date, key: {0}", recordingsCleanupKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("RecordingCleanupResponse with key {0} was found with value {1}", recordingsCleanupKey, response.ToString());
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving RecordingCleanupResponse with key {0} failed with status: {1}, retryAttempt: {1}, maxRetries: {2}", recordingsCleanupKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get last successful recordings cleanup date, ex: {0}", ex);
            }

            if (response == null)
            {
                response = new RecordingCleanupResponse() { Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, "Failed Getting RecordingCleanupResponse from CB") };
            }

            return response;
        }

        public static ScheduledTaskLastRunResponse GetLastScheduleTaksSuccessfulRunDetails(string scheduleTaskName)
        {
            ScheduledTaskLastRunResponse response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string scheduledTaksKey = UtilsDal.GetScheduledTaksKeyByName(scheduleTaskName);
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                response = new ScheduledTaskLastRunResponse() { Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, "Failed UtilsDal.GetScheduledTaksKeyByName") };
                return response;
            }
            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<ScheduledTaskLastRunResponse>(scheduledTaksKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        response = new ScheduledTaskLastRunResponse() { Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, "CouchBase KeyNotFound") };
                        log.ErrorFormat("Error while trying to get last successful scheduled task run date, scheduleTaskName: {0}, key: {1}", scheduleTaskName, scheduledTaksKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("ScheduledTaskLastRunResponse with scheduleTaskName: {0} and key {1} was found with value {2}", scheduleTaskName, scheduledTaksKey, response.ToString());
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving ScheduledTaskLastRunResponse with scheduleTaskName: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", scheduleTaskName, scheduledTaksKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get last successful schedule task details, scheduleTaskName: {0}, ex: {1}", scheduleTaskName, ex);
            }

            if (response == null)
            {
                response = new ScheduledTaskLastRunResponse() { Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, "Failed Getting ScheduledTaskLastRunResponse from CB") };
            }

            return response;
        }

        public static bool UpdateScheduledTaskSuccessfulRun(string scheduleTaskName, DateTime lastSuccessfulRunDate, int impactedItems, double nextRunIntervalInSeconds)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string scheduledTaksKey = UtilsDal.GetScheduledTaksKeyByName(scheduleTaskName);
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed UtilsDal.GetScheduledTaksKeyByName for ScheduleTaskName: {0}", scheduleTaskName);
                return false;
            }
            try
            {
                int numOfRetries = 0;
                ScheduledTaskLastRunResponse scheduledTaskRunDetails = new ScheduledTaskLastRunResponse(lastSuccessfulRunDate, impactedItems, nextRunIntervalInSeconds);
                while (!result && numOfRetries < limitRetries)
                {
                    result = cbClient.Set(scheduledTaksKey, scheduledTaskRunDetails);
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating successful scheduled task run details. scheduleTaskName: {0}, number of tries: {1}/{2}. ScheduledTaskLastRunResponse: {3}",
                                         scheduleTaskName, numOfRetries, limitRetries, scheduledTaskRunDetails.ToString());

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating successful scheduled task run details, ScheduleTaskName: {0}, ex: {1}", scheduleTaskName, ex);
            }

            return result;
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

        public static DataTable GetExpiredRecordingsTasks(long unixTimeStampNow)
        {
            ODBCWrapper.StoredProcedure spGetExpiredRecordingsTasks = new ODBCWrapper.StoredProcedure("GetExpiredRecordingsTasks");
            spGetExpiredRecordingsTasks.SetConnectionKey(RECORDING_CONNECTION);
            spGetExpiredRecordingsTasks.AddParameter("@UtcNowEpoch", unixTimeStampNow);

            return spGetExpiredRecordingsTasks.Execute();
        }

        public static DataTable GetExpiredDomainsRecordings(long recordingId, long unixTimeStampNow)
        {
            ODBCWrapper.StoredProcedure spGetExpiredDomainsRecordings = new ODBCWrapper.StoredProcedure("GetExpiredDomainsRecordings");
            spGetExpiredDomainsRecordings.SetConnectionKey(RECORDING_CONNECTION);
            spGetExpiredDomainsRecordings.AddParameter("@RecordingId", recordingId);
            spGetExpiredDomainsRecordings.AddParameter("@UtcNowEpoch", unixTimeStampNow);

            return spGetExpiredDomainsRecordings.Execute();
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

        public static int GetRecordingDuration(long recordingId)
        {
            int recordingDuration = -1;
            ODBCWrapper.StoredProcedure spGetRecordingDuration = new ODBCWrapper.StoredProcedure("GetRecordingDuration");
            spGetRecordingDuration.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingDuration.AddParameter("@ID", recordingId);

            DataTable dt = spGetRecordingDuration.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                recordingDuration = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "DURATION", 0);
            }

            return recordingDuration;
        }

        public static DataTable GetEpgToRecordingsMapByCrid(int groupId, string crid)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetEpgToRecordingsMapByCrid = new ODBCWrapper.StoredProcedure("GetRecordingsByCrid");
            spGetEpgToRecordingsMapByCrid.SetConnectionKey(RECORDING_CONNECTION);
            spGetEpgToRecordingsMapByCrid.AddParameter("@GroupID", groupId);
            spGetEpgToRecordingsMapByCrid.AddParameter("@Crid", crid);
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
            spFollowSeries.AddParameter("@SeriesId", seriesId);
            spFollowSeries.AddParameter("@SeasonNumber", seasonNumber);
            spFollowSeries.AddParameter("@EpisodeNumber", episodeNumber);
            DataTable dt = spFollowSeries.Execute();

            return dt;
        }

        public static bool IsSeriesFollowed(int groupId, string seriesId, int seasonNumber)
        {
            ODBCWrapper.StoredProcedure spIsSeriesFollowed = new ODBCWrapper.StoredProcedure("IsSeriesFollowed");
            spIsSeriesFollowed.SetConnectionKey(RECORDING_CONNECTION);
            spIsSeriesFollowed.AddParameter("@GroupID", groupId);            
            spIsSeriesFollowed.AddParameter("@SeriesId", seriesId);
            spIsSeriesFollowed.AddParameter("@SeasonNumber", seasonNumber);

            int rowsFound = spIsSeriesFollowed.ExecuteReturnValue<int>();

            return rowsFound == 1;
        }

        public static bool IsFollowingSeries(int groupId, long domainID, string seriesId, int seasonNumber)
        {
            ODBCWrapper.StoredProcedure spIsFollowingSeries = new ODBCWrapper.StoredProcedure("IsFollowingSeries");            
            spIsFollowingSeries.SetConnectionKey(RECORDING_CONNECTION);
            spIsFollowingSeries.AddParameter("@GroupID", groupId);            
            spIsFollowingSeries.AddParameter("@DomainId", domainID);
            spIsFollowingSeries.AddParameter("@SeriesId", seriesId);
            spIsFollowingSeries.AddParameter("@SeasonNumber", seasonNumber);

            int rowsFound = spIsFollowingSeries.ExecuteReturnValue<int>();

            return rowsFound == 1;
        }

        public static bool UpdateRecordingsExternalId(int groupId, string externalRecordingId, string crid)
        {
            int updatedRowsCount = 0;
            ODBCWrapper.StoredProcedure spUpdateRecordingsExternalId = new ODBCWrapper.StoredProcedure("UpdateRecordingsExternalId");
            spUpdateRecordingsExternalId.SetConnectionKey(RECORDING_CONNECTION);
            spUpdateRecordingsExternalId.AddParameter("@GroupID", groupId);
            spUpdateRecordingsExternalId.AddParameter("@ExternalRecordingId", externalRecordingId);
            spUpdateRecordingsExternalId.AddParameter("@Crid", crid);

            updatedRowsCount = spUpdateRecordingsExternalId.ExecuteReturnValue<int>();

            return updatedRowsCount > 0;            
        }

        public static DataTable GetDomainSeriesRecordings(int groupId, long domainId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DomainSeries");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@domainId", domainId);

            DataTable dt = sp.Execute();
            return dt;
        }

        public static Dictionary<long, long> GetEpgToRecordingsMap(int groupId, List<long> recordingIds)
        {
            Dictionary<long, long> epgsToRecordingsMap = new Dictionary<long, long>();

            ODBCWrapper.StoredProcedure spGetRecordingsToEpgMap = new ODBCWrapper.StoredProcedure("GetRecordingsEpgs");
            spGetRecordingsToEpgMap.SetConnectionKey(RECORDING_CONNECTION);
            spGetRecordingsToEpgMap.AddParameter("@GroupID", groupId);
            spGetRecordingsToEpgMap.AddIDListParameter<int>("@RecordingIds", recordingIds.Select(s => (int)s).ToList(), "ID");

            DataTable dt = spGetRecordingsToEpgMap.Execute();
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    long recordingId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    long epgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_PROGRAM_ID", 0);
                    if (recordingId > 0 && epgId > 0 && !epgsToRecordingsMap.ContainsKey(recordingId))
                    {
                        epgsToRecordingsMap.Add(epgId, recordingId);
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
                    if (recordingId > 0 && epgId > 0 && !epgsToRecordingsMap.ContainsKey(recordingId))
                    {
                        epgsToRecordingsMap.Add(epgId, recordingId);
                    }
                }
            }

            return epgsToRecordingsMap;
        }

        public static DataTable GetDomainSeriesRecordingsById(int groupId, long domainId, long domainSeriesRecordingId)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetDomainRecordingsByIds = new ODBCWrapper.StoredProcedure("GetDomainSeriesRecordingsById");
            spGetDomainRecordingsByIds.SetConnectionKey(RECORDING_CONNECTION);
            spGetDomainRecordingsByIds.AddParameter("@GroupID", groupId);
            spGetDomainRecordingsByIds.AddParameter("@DomainID", domainId);
            spGetDomainRecordingsByIds.AddParameter("@DomainRecordingId", domainSeriesRecordingId);
            dt = spGetDomainRecordingsByIds.Execute();

            return dt;
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

        public static int CountRecordingsByExternalRecordingId(int groupId, string externalRecordingId)
        {
            ODBCWrapper.StoredProcedure spCountRecordingsByExternalRecordingId = new ODBCWrapper.StoredProcedure("CountRecordingsByExternalRecordingId");
            spCountRecordingsByExternalRecordingId.SetConnectionKey(RECORDING_CONNECTION);
            spCountRecordingsByExternalRecordingId.AddParameter("@GroupID", groupId);
            spCountRecordingsByExternalRecordingId.AddParameter("@ExternalRecordingId", externalRecordingId);

            return spCountRecordingsByExternalRecordingId.ExecuteReturnValue<int>();           
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
        public static bool GetDomainQuota(int groupId, long domainId, out int quota)
        {
            bool result = false;
            quota = 0;
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
                        quota = cbClient.Get<int>(domainQuotaKey, out getResult);
                        if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            log.ErrorFormat("Error while trying to get domain quota, domainId: {0}, key: {1}", domainId, domainQuotaKey);
                            break;
                        }
                        else if (getResult == Couchbase.IO.ResponseStatus.Success)
                        {
                            result = true;
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
        /// </summary>
        public static bool IncreaseDomainQuota(long domainId, int quotaToIncrease)
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
                    int currentQuota = -1;
                    currentQuota = cbClient.GetWithVersion<int>(domainQuotaKey, out version);
                    if (version != 0 && currentQuota > -1)
                    {
                        int updatedQuota = currentQuota + quotaToIncrease;
                        result = cbClient.SetWithVersion<int>(domainQuotaKey, updatedQuota, version);
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while adding quota to domain. number of tries: {0}/{1}. domainId: {2}, currentQuota: {3}, quotaToIncrease: {4}", numOfRetries, limitRetries, domainId, currentQuota, quotaToIncrease);
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding quota to domain, domainId: {0}, quotaToIncrease: {1}, ex: {2}", domainId, quotaToIncrease, ex);
            }

            return result;
        }

        /// <summary>
        /// DO NOT DIRECTLY USE THIS FUNCTION, USE QuotaManager.DecreaseDomainQuota
        /// </summary>
        public static bool DecreaseDomainQuota(long domainId, int quotaToDecrease)
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
                    int currentQuota = -1;
                    currentQuota = cbClient.GetWithVersion<int>(domainQuotaKey, out version);
                    if (version != 0 && currentQuota > -1)
                    {
                        int updatedQuota = currentQuota + quotaToDecrease;
                        result = cbClient.SetWithVersion<int>(domainQuotaKey, updatedQuota, version);
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while adding quota to domain. number of tries: {0}/{1}. domainId: {2}, currentQuota: {3}, quotaToDecrease: {4}", numOfRetries, limitRetries, domainId, currentQuota, quotaToDecrease);
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding quota to domain, domainId: {0}, quotaToDecrease: {1}, ex: {2}", domainId, quotaToDecrease, ex);
            }

            return result;
        }

        #endregion

    }
}
