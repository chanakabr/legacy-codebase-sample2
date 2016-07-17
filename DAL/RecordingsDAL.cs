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

        public static Recording GetRecordingByProgramId(long programId)
        {
            Recording recording = null;

            DataRow row = ODBCWrapper.Utils.GetTableSingleRowByValue("recordings", "EPG_PROGRAM_ID", programId, true, RECORDING_CONNECTION);

            if (row != null)
            {
                recording = BuildRecordingFromRow(row);
            }

            return recording;
        }

        public static Recording InsertRecording(Recording recording, int groupId, RecordingInternalStatus? status)
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

            var insertQuery = new ODBCWrapper.InsertQuery("recordings");
            insertQuery.SetConnectionKey(RECORDING_CONNECTION);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", recording.EpgEndDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_PROGRAM_ID", "=", recording.EpgId);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTERNAL_RECORDING_ID", "=", recording.ExternalRecordingId);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RECORDING_STATUS", "=", recordingStatus);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", recording.EpgStartDate);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", DateTime.UtcNow);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GET_STATUS_RETRIES", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CRID", "=", recording.Crid);

            var executeResult = insertQuery.ExecuteAndGetId();
            insertQuery.Finish();
            insertQuery = null;

            recording.Id = executeResult;

            return recording;
        }

        public static Recording GetRecordingByRecordingId(long recordingId)
        {
            Recording recording = null;

            DataRow row = ODBCWrapper.Utils.GetTableSingleRowByValue("recordings", "ID", recordingId, true, RECORDING_CONNECTION);

            if (row != null)
            {
                recording = BuildRecordingFromRow(row);
            }

            return recording;
        }

        public static bool UpdateRecording(Recording recording, int groupId, int rowStatus, int isActive, RecordingInternalStatus? status)
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

            var updateQuery = new ODBCWrapper.UpdateQuery("recordings");
            updateQuery.SetConnectionKey(RECORDING_CONNECTION);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", recording.EpgEndDate);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EPG_PROGRAM_ID", "=", recording.EpgId);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTERNAL_RECORDING_ID", "=", recording.ExternalRecordingId);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RECORDING_STATUS", "=", recordingStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("START_DATE", "=", recording.EpgStartDate);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", rowStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", isActive);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GET_STATUS_RETRIES", "=", recording.GetStatusRetries);

            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", recording.Id);
            result = updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
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
            long epgChannelId;
            if (!long.TryParse(recording.ChannelId, out epgChannelId))
            {
                log.ErrorFormat("Failed parsing epgChannelId on UpdateOrInsertDomainRecording, groupID: {0}, userID: {1}, domainID: {2}, recording: {3}", groupID, userID, domainID, recording.ToString());
                return res;
            }

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

        public static List<Recording> GetRecordings(int groupId, List<long> recordingIds)
        {
            List<Recording> recordings = new List<Recording>();

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_Recordings");
            storedProcedure.SetConnectionKey(RECORDING_CONNECTION);
            storedProcedure.AddIDListParameter<long>("@RecordingIds", recordingIds, "Id");
            storedProcedure.AddParameter("@GroupId", groupId);

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            recordings = BuildRecordingsFromDataSet(dataSet);

            return recordings;
        }

        private static List<Recording> BuildRecordingsFromDataSet(DataSet dataSet)
        {
            List<Recording> recordings = new List<Recording>();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 &&
                dataSet.Tables[0] != null && dataSet.Tables[0].Rows != null)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    Recording recording = BuildRecordingFromRow(row);

                    recordings.Add(recording);
                }
            }

            return recordings;
        }

        private static Recording BuildRecordingFromRow(DataRow row)
        {
            Recording recording = new Recording();
            recording.EpgId = ODBCWrapper.Utils.ExtractValue<long>(row, "EPG_PROGRAM_ID");
            recording.Id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");
            RecordingInternalStatus recordingStatus = (RecordingInternalStatus)ODBCWrapper.Utils.ExtractInteger(row, "RECORDING_STATUS");
            recording.ExternalRecordingId = ODBCWrapper.Utils.ExtractString(row, "EXTERNAL_RECORDING_ID");
            recording.EpgStartDate = ODBCWrapper.Utils.ExtractDateTime(row, "START_DATE");
            recording.EpgEndDate = ODBCWrapper.Utils.ExtractDateTime(row, "END_DATE");
            recording.GetStatusRetries = ODBCWrapper.Utils.ExtractInteger(row, "GET_STATUS_RETRIES");

            TstvRecordingStatus status = TstvRecordingStatus.OK;

            switch (recordingStatus)
            {
                case RecordingInternalStatus.Waiting:
                    {
                        // If we are still waiting for confirmation but program started already, we say it is failed
                        if (recording.EpgStartDate < DateTime.UtcNow)
                        {
                            status = TstvRecordingStatus.Failed;
                        }
                        else
                        {
                            status = TstvRecordingStatus.Scheduled;
                        }

                        break;
                    }
                case RecordingInternalStatus.OK:
                    {
                        // If program already finished, we say it is recorded
                        if (recording.EpgEndDate < DateTime.UtcNow)
                        {
                            status = TstvRecordingStatus.Recorded;
                        }
                        // If program already started but didn't finish, we say it is recording
                        else if (recording.EpgStartDate < DateTime.UtcNow)
                        {
                            status = TstvRecordingStatus.Recording;
                        }
                        else
                        {
                            status = TstvRecordingStatus.Scheduled;
                        }
                        break;
                    }
                case RecordingInternalStatus.Failed:
                    {
                        status = TstvRecordingStatus.Failed;
                        break;
                    }
                case RecordingInternalStatus.Canceled:
                    {
                        status = TstvRecordingStatus.Canceled;
                        break;
                    }
                case RecordingInternalStatus.Deleted:
                    {
                        status = TstvRecordingStatus.Deleted;
                        break;
                    }
                default:
                    {
                        status = TstvRecordingStatus.Deleted;
                        break;
                    }
            }

            recording.RecordingStatus = status;

            return recording;
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

        public static List<Recording> GetAllRecordingsByStatuses(int groupId, List<int> statuses)
        {
            List<Recording> recordings = new List<Recording>();

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_All_Recordings_By_Recording_Status");
            storedProcedure.SetConnectionKey(RECORDING_CONNECTION);

            storedProcedure.AddParameter("@GroupId", groupId);
            storedProcedure.AddIDListParameter<int>("@RecordingStatuses", statuses.Select(s => (int)s).ToList(), "ID");

            DataSet dataSet = storedProcedure.ExecuteDataSet();

            recordings = BuildRecordingsFromDataSet(dataSet);

            return recordings;
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

        public static bool FollowSeries(int groupId, string userId, long domainID, long epgId, string epgChannelId, string seriesId, int seasonNumber, int episodeNumber)
        {
            long channelId;
            if (!long.TryParse(epgChannelId, out channelId))
            {
                log.ErrorFormat("Error on FollowSeries while trying to parse epgChannelId: {0}", epgChannelId);
                return false;
            }

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

            return spFollowSeries.ExecuteReturnValue<bool>();
        }

        public static bool IsFirstFollower(int groupId, string seriesId, int seasonNumber)
        {
            ODBCWrapper.StoredProcedure spIsFirstFollower = new ODBCWrapper.StoredProcedure("IsFirstFollower");
            spIsFirstFollower.SetConnectionKey(RECORDING_CONNECTION);
            spIsFirstFollower.AddParameter("@GroupID", groupId);            
            spIsFirstFollower.AddParameter("@SeriesId", seriesId);
            spIsFirstFollower.AddParameter("@SeasonNumber", seasonNumber);

            int rowsFound = spIsFirstFollower.ExecuteReturnValue<int>();

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

        public static List<DomainSeriesRecording> GetDomainSeriesRecordings(int groupId, long domainId)
        {
            List<DomainSeriesRecording> response = new List<DomainSeriesRecording>();

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DomainSeries");
            sp.SetConnectionKey(RECORDING_CONNECTION);
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@domainId", domainId);

            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    response.Add(new DomainSeriesRecording()
                    {
                        EpgId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_ID", 0),
                        EpisodeNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "EPISODE_NUMBER", 0),
                        SeasonNumber = ODBCWrapper.Utils.GetIntSafeVal(dr, "SEASON_NUMBER", 0),
                        SeriesId = ODBCWrapper.Utils.GetSafeStr(dr, "SERIES_ID"),
                        UserId = ODBCWrapper.Utils.GetSafeStr(dr, "USER_ID"),
                        EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(dr, "EPG_CHANNEL_ID", 0),
                    });
                }
            }

            return response;
        }

        #region Couchbase

        public static RecordingCB GetRecordingByProgramId_CB(long programId)
        {
            RecordingCB result = null;

            CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);

            result = client.Get<RecordingCB>(programId.ToString());
            return result;
        }

        //public static RecordingCB GetRecordingByRecordingId_CB(long recordingId)
        //{
        //    RecordingCB result = null;

        //    CouchbaseManager.CouchbaseManager client = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.RECORDINGS);

        //    result = client.Get<RecordingCB>(recordingId.ToString());
        //    return result;
        //}

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

        #endregion


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
    }
}
