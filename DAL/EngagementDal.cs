using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using ApiObjects;
using ApiObjects.Notification;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;


namespace DAL
{
    public class EngagementDal
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.NOTIFICATION);
        private const string MESSAGE_BOX_CONNECTION = "MESSAGE_BOX_CONNECTION_STRING";
        private const string CB_DESIGN_DOC_ENGAGEMENT = "engagements";
        private const int SLEEP_BETWEEN_RETRIES_MILLI = 1000;
        private const int NUM_OF_TRIES = 3;

        private const int TTL_USER_ENGAGEMENT_DAYS = 30;


        private static string GetUserEngagementKey(int partnerId, int engagementId, int userId)
        {
            return string.Format("user_engagement:{0}:{1}:{2}", partnerId, engagementId, userId);
        }

        public static List<EngagementAdapter> GetEngagementAdapterList(int groupId)
        {
            List<EngagementAdapter> res = new List<EngagementAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementAdapterList");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    DataTable settingsTable = null;

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        settingsTable = ds.Tables[1];
                    }

                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        EngagementAdapter engagementAdapter = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            engagementAdapter = CreateEngagementAdapter(dr, settingsTable);
                            res.Add(engagementAdapter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagementAdapterList. groupId: {0}. Error {1}", groupId, ex);
            }
            return res;
        }

        private static EngagementAdapter CreateEngagementAdapter(DataRow adapterRow, DataTable settingsTable)
        {
            EngagementAdapter adapterRes = null;

            adapterRes = new EngagementAdapter();
            adapterRes.ProviderUrl = ODBCWrapper.Utils.GetSafeStr(adapterRow, "provider_url");
            adapterRes.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(adapterRow, "adapter_url");
            adapterRes.ID = ODBCWrapper.Utils.GetIntSafeVal(adapterRow, "ID");
            adapterRes.IsActive = ODBCWrapper.Utils.GetIntSafeVal(adapterRow, "is_active") == 1 ? true : false;
            adapterRes.Name = ODBCWrapper.Utils.GetSafeStr(adapterRow, "name");
            adapterRes.SharedSecret = ODBCWrapper.Utils.GetSafeStr(adapterRow, "shared_secret");

            if (settingsTable != null)
            {
                foreach (DataRow dr in settingsTable.Rows)
                {
                    int engagementAdapterId = ODBCWrapper.Utils.GetIntSafeVal(dr, "Engagement_Adapter_id", 0);
                    if (engagementAdapterId > 0 && engagementAdapterId != adapterRes.ID)
                        continue;

                    string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                    string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                    if (adapterRes.Settings == null)
                    {
                        adapterRes.Settings = new List<EngagementAdapterSettings>();
                    }
                    adapterRes.Settings.Add(new EngagementAdapterSettings() { Key = key, Value = value });
                }
            }

            return adapterRes;
        }

        public static EngagementAdapter GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            EngagementAdapter adapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementAdapter");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@engagementAdapterId", engagementAdapterId);

                DataSet ds = sp.ExecuteDataSet();

                adapterRes = CreateEngagementAdapter(ds);

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagementAdapter. groupId: {0}. Error {1}", groupId, ex);
            }
            return adapterRes;
        }

        private static EngagementAdapter CreateEngagementAdapter(DataSet ds)
        {
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow adapterRow = ds.Tables[0].Rows[0];
                DataTable settingsTable = null;

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    settingsTable = ds.Tables[1];
                }

                return CreateEngagementAdapter(adapterRow, settingsTable);
            }

            return null;
        }

        public static bool DeleteEngagementAdapter(int groupId, int engagementAdapterId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_EngagementAdapter");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", engagementAdapterId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at DeleteEngagementAdapter. groupId: {0}. Error {1}", groupId, ex);
                return false;
            }
        }

        public static EngagementAdapter InsertEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            EngagementAdapter adapterRes = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_EngagementAdapter");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@name", engagementAdapter.Name);
                sp.AddParameter("@adapterUrl", engagementAdapter.AdapterUrl);
                sp.AddParameter("@providerUrl", engagementAdapter.ProviderUrl);
                sp.AddParameter("@sharedSecret", engagementAdapter.SharedSecret);
                sp.AddParameter("@isActive", engagementAdapter.IsActive);

                DataTable dt = CreateDataTable(groupId, engagementAdapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                DataSet ds = sp.ExecuteDataSet();

                adapterRes = CreateEngagementAdapter(ds);
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error at InsertEngagementAdapter. groupId: {0}. Error {1}", groupId, ex);

            }

            return adapterRes;
        }

        private static DataTable CreateDataTable(int groupId, List<EngagementAdapterSettings> list)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            try
            {
                resultTable.Columns.Add("idkey", typeof(string));
                resultTable.Columns.Add("value", typeof(string));

                foreach (EngagementAdapterSettings item in list)
                {
                    DataRow row = resultTable.NewRow();
                    row["idkey"] = item.Key;
                    row["value"] = item.Value;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at create EngagementAdapterSettings. groupId: {0}. Error {1}", groupId, ex);
                return null;
            }

            return resultTable;
        }

        public static EngagementAdapter SetEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            EngagementAdapter adapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_EngagementAdapter");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupID", groupId);
                sp.AddParameter("@ID", engagementAdapter.ID);
                sp.AddParameter("@name", engagementAdapter.Name);
                sp.AddParameter("@sharedSecret", engagementAdapter.SharedSecret);
                sp.AddParameter("@adapterUrl", engagementAdapter.AdapterUrl);
                sp.AddParameter("@providerUrl", engagementAdapter.ProviderUrl);
                sp.AddParameter("@isActive", engagementAdapter.IsActive);

                DataSet ds = sp.ExecuteDataSet();

                adapterRes = CreateEngagementAdapter(ds);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetEngagementAdapter. groupId: {0}. Error {1}", groupId, ex);
            }

            return adapterRes;
        }

        public static bool SetEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_EngagementAdapterSettings");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupID", groupId);
                sp.AddParameter("@ID", engagementAdapterId);

                DataTable dt = CreateDataTable(groupId, settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetEngagementAdapterSettings. groupId: {0}. Error {1}", groupId, ex);
                return false;
            }
        }

        public static EngagementAdapter SetEngagementAdapterSharedSecret(int groupId, int engagementAdapterId, string sharedSecret)
        {
            EngagementAdapter adapterRes = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_EngagementAdapterSharedSecret");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", engagementAdapterId);
                sp.AddParameter("@sharedSecret", sharedSecret);

                DataSet ds = sp.ExecuteDataSet();

                adapterRes = CreateEngagementAdapter(ds);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetEngagementAdapterSharedSecret. groupId: {0}. Error {1}", groupId, ex);
            }

            return adapterRes;
        }

        public static bool DeleteEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_EngagementAdapterSettings");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@GroupID", groupId);
                sp.AddParameter("@ID", engagementAdapterId);
                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static bool InsertEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_EngagementAdapterSettings");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@GroupID", groupId);
                sp.AddParameter("@ID", engagementAdapterId);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isInsert = sp.ExecuteReturnValue<bool>();
                return isInsert;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        private static DataTable CreateDataTable(List<EngagementAdapterSettings> list)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            try
            {
                resultTable.Columns.Add("idkey", typeof(string));
                resultTable.Columns.Add("value", typeof(string));

                foreach (EngagementAdapterSettings item in list)
                {
                    DataRow row = resultTable.NewRow();
                    row["idkey"] = item.Key;
                    row["value"] = item.Value;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return null;
            }

            return resultTable;
        }

        public static List<EngagementAdapter> GetEngagementAdapterSettingsList(int groupId, int engagementAdapterId)
        {
            List<EngagementAdapter> res = new List<EngagementAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementAdapterSettingsList");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@GroupID", groupId);
                sp.AddParameter("@engagementAdapterId", engagementAdapterId);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    DataTable dtConfig = ds.Tables[1];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        EngagementAdapter engagementAdapter = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            engagementAdapter = new EngagementAdapter();
                            engagementAdapter.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            engagementAdapter.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            engagementAdapter.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            engagementAdapter.ProviderUrl = ODBCWrapper.Utils.GetSafeStr(dr, "provider_url");
                            engagementAdapter.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            int is_Active = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            engagementAdapter.IsActive = is_Active == 1 ? true : false;

                            if (dtConfig != null)
                            {
                                DataRow[] drpc = dtConfig.Select("engagement_adapter_id =" + engagementAdapter.ID);

                                foreach (DataRow drp in drpc)
                                {
                                    string key = ODBCWrapper.Utils.GetSafeStr(drp, "key");
                                    string value = ODBCWrapper.Utils.GetSafeStr(drp, "value");
                                    if (engagementAdapter.Settings == null)
                                    {
                                        engagementAdapter.Settings = new List<EngagementAdapterSettings>();
                                    }
                                    engagementAdapter.Settings.Add(new EngagementAdapterSettings() { Key = key, Value = value });
                                }
                            }
                            res.Add(engagementAdapter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<EngagementAdapter>();
            }
            return res;
        }

        public static UserEngagement GetUserEngagement(int partnerId, int engagementId, int userId)
        {
            UserEngagement userEngagement = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            string key = GetUserEngagementKey(partnerId, engagementId, userId);

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    userEngagement = cbManager.Get<UserEngagement>(key, out status);
                    if (userEngagement == null)
                    {
                        if (status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while getting user engagement data. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_TRIES,
                                key);

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        result = true;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully received user engagement data. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_TRIES,
                            key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user engagement data. key: {0}, ex: {1}", key, ex);
            }

            return userEngagement;
        }

        public static List<UserEngagement> GetBulkUserEngagementView(int engagementId, int engagementBulkId)
        {
            List<UserEngagement> bulkEngagements = null;
            try
            {
                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_ENGAGEMENT, "get_bulk_engagements")
                {
                    startKey = new object[] { engagementId, engagementBulkId },
                    endKey = new object[] { engagementId, engagementBulkId },
                    staleState = ViewStaleState.False
                };

                // execute request
                bulkEngagements = cbManager.View<UserEngagement>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get bulk engagement. engagementId: {0}, bulk ID: {1}, ex: {2}", engagementId, engagementBulkId, ex);
            }

            return bulkEngagements;
        }

        public static bool SetUserEngagement(UserEngagement userEngagement)
        {
            bool result = false;
            try
            {
                // get user engagement TTL
                int userEngagementTtl = TCMClient.Settings.Instance.GetValue<int>("ttl_user_engagement_days");
                if (userEngagementTtl == 0)
                    userEngagementTtl = TTL_USER_ENGAGEMENT_DAYS;

                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    result = cbManager.Set(GetUserEngagementKey(userEngagement.PartnerId, userEngagement.EngagementId, userEngagement.UserId), userEngagement, (uint)TimeSpan.FromDays(userEngagementTtl).TotalSeconds);
                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting user engagement document. number of tries: {0}/{1}. User engagement object: {2}",
                             numOfTries,
                            NUM_OF_TRIES,
                            JsonConvert.SerializeObject(userEngagement));

                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set user engagement document. number of tries: {0}/{1}. User engagement object {2}",
                            numOfTries,
                            NUM_OF_TRIES,
                            JsonConvert.SerializeObject(userEngagement));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting user engagement document.  User engagement object: {0}, ex: {1}", JsonConvert.SerializeObject(userEngagement), ex);
            }

            return result;
        }

        public static Engagement InsertEngagement(int groupId, Engagement engagement)
        {
            Engagement res = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_Engagement");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@sendTime", engagement.SendTime);
                sp.AddParameter("@engagementType", engagement.EngagementType);
                sp.AddParameter("@adapterId", engagement.AdapterId);
                sp.AddParameter("@adapterDynamicData", engagement.AdapterDynamicData);
                sp.AddParameter("@intervalSeconds", engagement.IntervalSeconds);
                sp.AddParameter("@userList", engagement.UserList);
                sp.AddParameter("@couponGroupId", engagement.CouponGroupId);
                sp.AddParameter("@isActive", engagement.IsActive);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagement(ds.Tables[0].Rows[0]);
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error at InsertEngagement. groupId: {0}. Error {1}", groupId, ex);
            }

            return res;
        }

        public static Engagement SetEngagement(int groupId, Engagement engagement)
        {
            Engagement res = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_Engagement");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", engagement.Id);
                sp.AddParameter("@totalNumberOfRecipients", engagement.TotalNumberOfRecipients);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagement(ds.Tables[0].Rows[0]);
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetEngagement. groupId: {0}. Error {1}", groupId, ex);
            }

            return res;
        }

        public static List<Engagement> GetEngagementList(int partnerId, DateTime? fromSendDate = null, bool shouldOnlyGetActive = false, List<eEngagementType> engagementTypes = null)
        {
            List<Engagement> res = new List<Engagement>();
            List<int> engagementTypeIds = new List<int>();

            try
            {
                if (engagementTypes != null && engagementTypes.Count > 0)
                {
                    engagementTypeIds = engagementTypes.Select(i => (int)i).ToList();
                }

                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementList");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", partnerId);
                sp.AddParameter("@shouldGetOnlyActive", shouldOnlyGetActive);
                sp.AddParameter("@fromDate", fromSendDate);
                if (engagementTypeIds.Count > 0)
                    sp.AddIDListParameter("@engagementTypes", engagementTypeIds, "id");
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    Engagement engagement = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        engagement = CreateEngagement(dr);
                        res.Add(engagement);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagementList. groupId: {0}. Error {1}", partnerId, ex);
            }
            return res;
        }

        private static Engagement CreateEngagement(DataRow dr)
        {
            Engagement result = null;

            if (dr != null)
            {
                int engagementType = ODBCWrapper.Utils.GetIntSafeVal(dr, "ENGAGEMENT_TYPE");

                result = new Engagement()
                {
                    Id = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID"),
                    AdapterDynamicData = ODBCWrapper.Utils.GetSafeStr(dr, "ADAPTER_DYNAMIC_DATA"),
                    AdapterId = ODBCWrapper.Utils.GetIntSafeVal(dr, "ADAPTER_ID"),
                    EngagementType = Enum.IsDefined(typeof(eEngagementType), engagementType) ? (eEngagementType)engagementType : eEngagementType.Churn,
                    IntervalSeconds = ODBCWrapper.Utils.GetIntSafeVal(dr, "INTERVAL_SECONDS"),
                    SendTime = ODBCWrapper.Utils.GetDateSafeVal(dr, "SEND_TIME"),
                    TotalNumberOfRecipients = ODBCWrapper.Utils.GetIntSafeVal(dr, "TOTAL_NUMBER_OF_RECIPIENTS"),
                    UserList = ODBCWrapper.Utils.GetSafeStr(dr, "USER_LIST"),
                    IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE") == 1 ? true : false,
                    CouponGroupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "COUPON_GROUP_ID")
                };
            }
            return result;
        }

        public static Engagement GetEngagement(int groupId, int id)
        {
            Engagement res = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_Engagement");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagement(ds.Tables[0].Rows[0]);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagement. groupId: {0}. Error {1}", groupId, ex);
            }
            return res;
        }

        public static bool DeleteEngagement(int groupId, int id)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_Engagement");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at DeleteEngagement. groupId: {0}. Error {1}", groupId, ex);
                return false;
            }
        }

        public static EngagementBulkMessage InsertEngagementBulkMessage(int groupId, EngagementBulkMessage engagementBulkMessage)
        {
            EngagementBulkMessage res = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_EngagementBulkMessage");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@engagementId", engagementBulkMessage.EngagementId);
                sp.AddParameter("@isSent", engagementBulkMessage.IsSent ? 1 : 0);
                sp.AddParameter("@iterationOffset", engagementBulkMessage.IterationOffset);
                sp.AddParameter("@iterationSize", engagementBulkMessage.IterationSize);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagementBulkMessage(ds.Tables[0].Rows[0]);
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error at InsertEngagementBulkMessage. groupId: {0}. Error {1}", groupId, ex);
            }

            return res;
        }

        private static EngagementBulkMessage CreateEngagementBulkMessage(DataRow dataRow)
        {
            EngagementBulkMessage result = null;

            if (dataRow != null)
            {
                result = new EngagementBulkMessage()
                {
                    Id = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ID"),
                    EngagementId = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ENGAGEMENT_ID"),
                    IsSent = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_SENT") == 1 ? true : false,
                    IterationOffset = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ITERATION_OFFSET"),
                    IterationSize = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ITERATION_SIZE")
                };
            }
            return result;
        }

        public static EngagementBulkMessage GetEngagementBulkMessage(int groupId, int id)
        {
            EngagementBulkMessage res = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementBulkMessage");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagementBulkMessage(ds.Tables[0].Rows[0]);
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagementBulkMessage. groupId: {0}. Error {1}", groupId, ex);
            }
            return res;
        }

        public static EngagementBulkMessage SetEngagementBulkMessage(int groupId, EngagementBulkMessage engagementBulkMessage)
        {
            EngagementBulkMessage res = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_EngagementBulkMessage");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", engagementBulkMessage.EngagementId);
                sp.AddParameter("@isSent", engagementBulkMessage.IsSent ? 1 : 0);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = CreateEngagementBulkMessage(ds.Tables[0].Rows[0]);
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetEngagementBulkMessage. groupId: {0}. Error {1}", groupId, ex);
            }

            return res;
        }

        public static List<EngagementBulkMessage> GetEngagementBulkMessages(int partnerId, int engagementId)
        {
            List<EngagementBulkMessage> res = new List<EngagementBulkMessage>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_EngagementBulkList");
                sp.SetConnectionKey(MESSAGE_BOX_CONNECTION);
                sp.AddParameter("@groupId", partnerId);
                sp.AddParameter("@engagementId", engagementId);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    EngagementBulkMessage bulkMessage = null;
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        bulkMessage = CreateEngagementBulkMessage(dr);
                        res.Add(bulkMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEngagementBulkMessages. groupId: {0}, engagementId: {1}. Error {2}", partnerId, engagementId, ex);
            }
            return res;
        }
    }
}
