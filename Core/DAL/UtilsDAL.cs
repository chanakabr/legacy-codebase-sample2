using ApiObjects;
using Phx.Lib.Appconfig;
using CouchbaseManager;
using Phx.Lib.Log;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace DAL
{
    public class UtilsDal : BaseDal
    {
        private const string SP_GET_OPERATOR_GROUP_ID = "sp_GetGroupIDByOperatorID";
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;
        private const int NUM_OF_TRIES = 3;
        private const string MAIN_CONNECTION_STRING = "MAIN_CONNECTION_STRING";
        private static readonly JsonSerializerSettings jsonSerializerSettings =
            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

        #region Generic Methods

        public static bool SaveObjectWithVersionCheckInCB<T>(uint ttl, eCouchbaseBucket couchbaseBucket, string key, Action<T> updateObjectAction, bool updateObjectActionIfNotExist = false,
                                                            bool compress = false, int limitMaxNumOfInsertTries = -1, Func<int, TimeSpan> retryStrategy = null) where T : new()
        {
            var internalCouchBaseManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
            var cbManager = compress ? (ICouchbaseManager)new CompressionCouchbaseManager(internalCouchBaseManager) : internalCouchBaseManager;
            var numOfTries = 0;
            int maxNumOfInsertTries = Math.Max(limitMaxNumOfInsertTries, ApplicationConfiguration.Current.CbMaxInsertTries.Value);
            ulong version;
            eResultStatus getResult = eResultStatus.ERROR;
            var r = new Random();
            var currentRetryStrategy = retryStrategy ?? (_ => TimeSpan.FromMilliseconds(r.Next(50)));

            try
            {
                while (numOfTries < maxNumOfInsertTries)
                {
                    var objectToSave = cbManager.GetWithVersion<T>(key, out version, out getResult);
                    if (getResult == eResultStatus.KEY_NOT_EXIST)
                    {
                        if (!updateObjectActionIfNotExist)
                        {
                            log.ErrorFormat("KeyNotFound - Error while SaveObjectWithVersionCheckInCB. key:{0}.", key);
                            return false;
                        }

                        objectToSave = new T();
                    }

                    if (getResult != eResultStatus.ERROR)
                    {
                        updateObjectAction(objectToSave);

                        if (cbManager.SetWithVersion(key, objectToSave, version, ttl))
                        {
                            log.DebugFormat("successfully SaveObjectWithVersionCheckInCB. key:{0}, number of tries:{1}/{2}.",
                                key,
                                numOfTries,
                                maxNumOfInsertTries);
                            return true;
                        }
                    }

                    numOfTries++;
                    log.WarnFormat("while SaveObjectWithVersionCheckInCB. key:{0}, number of tries:{1}/{2}.",
                        key, numOfTries, maxNumOfInsertTries);
                    Thread.Sleep(currentRetryStrategy(numOfTries));
                }

                if (getResult != eResultStatus.SUCCESS)
                {
                    log.Error($"Error while SaveObjectWithVersionCheckInCB, key:[{key}], all retry attempts exhausted object was not saved");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception - Error while SaveObjectWithVersionCheckInCB. key:{0}.", key), ex);
            }

            return false;
        }

        public static T GetObjectFromCB<T>(eCouchbaseBucket couchbaseBucket, string key, bool serializeToString = false)
        {
            eResultStatus getResult = eResultStatus.ERROR;
            return GetObjectFromCB<T>(couchbaseBucket, key, out getResult, serializeToString);
        }

        public static T GetObjectFromCB<T>(eCouchbaseBucket couchbaseBucket, string key, out eResultStatus getResult, bool serializeToString = false)
        {
            var internalCouchbaseManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
            var cbManager = new CompressionCouchbaseManager(internalCouchbaseManager);

            int numOfTries = 0;
            T responseT = default(T);
            getResult = eResultStatus.ERROR;

            try
            {
                Random r = new Random();
                while (numOfTries < NUM_OF_TRIES)
                {
                    string stringResponse = string.Empty;

                    if (serializeToString)
                    {
                        stringResponse = cbManager.Get<string>(key, out getResult);
                    }
                    else
                    {
                        responseT = cbManager.Get<T>(key, out getResult);
                    }

                    if (getResult == eResultStatus.KEY_NOT_EXIST)
                    {
                        log.DebugFormat("Error while trying GetObjectFromCB, KeyNotFound. key: {0}", key);
                        break;
                    }
                    else if (getResult == eResultStatus.SUCCESS)
                    {
                        log.DebugFormat("successfully GetObjectFromCB. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_TRIES,
                            key);

                        if (serializeToString)
                        {
                            return JsonConvert.DeserializeObject<T>(stringResponse, jsonSerializerSettings);
                        }
                        else
                        {
                            return responseT;
                        }
                    }
                    else
                    {
                        numOfTries++;
                        log.DebugFormat("Error while GetObjectFromCB. number of tries: {0}/{1}. key: {2}", numOfTries, NUM_OF_TRIES, key);
                        Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to GetObjectFromCB. key: {0}, ex: {1}", key, ex);
            }

            return responseT;
        }

        public static List<T> GetObjectListFromCB<T>(eCouchbaseBucket couchbaseBucket, List<string> keys, bool serializeToString = false)
        {
            if (keys == null || keys.Count == 0)
            {
                return null;
            }

            var cbManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
            int numOfTries = 0;

            try
            {
                Random r = new Random();
                while (numOfTries < NUM_OF_TRIES)
                {
                    if (serializeToString)
                    {
                        var cbValues = cbManager.GetValues<string>(keys, true);
                        if (cbValues != null)
                        {
                            var objectsList = new List<T>();
                            log.DebugFormat("successfully GetObjectListFromCB. number of tries: {0}/{1}. key {2}", numOfTries, NUM_OF_TRIES, string.Join(", ", keys));

                            foreach (var cbValue in cbValues)
                            {
                                if (Json.TryDeserialize<T>(cbValue.Value, out var deserializedObject))
                                {
                                    objectsList.Add(deserializedObject);
                                }
                                else
                                {
                                    log.Error($"Error while trying to DeserializeObject. key: {cbValue.Key}, value: {cbValue.Value}.");
                                }
                            }

                            return objectsList;
                        }
                    }
                    else
                    {
                        var cbValues = cbManager.GetValues<T>(keys, true);

                        if (cbValues != null)
                        {
                            log.DebugFormat("successfully GetObjectListFromCB. number of tries: {0}/{1}. key {2}", numOfTries, NUM_OF_TRIES, string.Join(", ", keys));
                            return new List<T>(cbValues.Select(x => x.Value));
                        }
                    }

                    numOfTries++;
                    log.ErrorFormat("Error while GetObjectListFromCB. number of tries: {0}/{1}. keys: {2}", numOfTries, NUM_OF_TRIES, string.Join(", ", keys));
                    Thread.Sleep(r.Next(50));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to GetObjectListFromCB. keys: {0}, ex: {1}", string.Join(", ", keys), ex);
            }

            return null;
        }

        public static bool SaveObjectInCB<T>(eCouchbaseBucket couchbaseBucket, string key, T objectToSave, bool serializeToString = false, uint expirationTTL = 0, bool compress = false)
        {
            return SaveObjectInCB(couchbaseBucket.ToString(), key, objectToSave, serializeToString, expirationTTL, compress);
        }

        public static bool SaveObjectInCB<T>(string couchbaseBucket, string key, T objectToSave, bool serializeToString = false, uint expirationTTL = 0, bool compress = false)
        {
            if (objectToSave != null)
            {
                var internalCouchbaseManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
                var cbManager = compress ? (ICouchbaseManager)new CompressionCouchbaseManager(internalCouchbaseManager) : internalCouchbaseManager;
                int numOfTries = 0;
                int maxNumOfInsertTries = ApplicationConfiguration.Current.CbMaxInsertTries.Value;

                try
                {
                    Random r = new Random();
                    string serializeObject = string.Empty;
                    if (serializeToString)
                    {
                        serializeObject = JsonConvert.SerializeObject(objectToSave, jsonSerializerSettings);
                    }

                    while (numOfTries < maxNumOfInsertTries)
                    {
                        if (serializeToString)
                        {
                            if (cbManager.Set(key, serializeObject, expirationTTL))
                            {
                                log.Debug($"successfully SaveObjectInCB. key: {key}, number of tries: {numOfTries}/{maxNumOfInsertTries}.");
                                return true;
                            }
                        }
                        else
                        {
                            if (cbManager.Set(key, objectToSave, expirationTTL))
                            {
                                log.Debug($"successfully SaveObjectInCB. key: {key}, number of tries: {numOfTries}/{maxNumOfInsertTries}.");
                                return true;
                            }
                        }

                        numOfTries++;
                        log.Error($"Error while SaveObjectInCBy. key: {key}, number of tries: {numOfTries}/{maxNumOfInsertTries}.");
                        Thread.Sleep(r.Next(50));
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error while trying to SaveObjectInCB. key: {key}, ex: {ex}.");
                }
            }

            return false;
        }

        public static bool DeleteObjectFromCB(eCouchbaseBucket couchbaseBucket, string key)
        {
            bool result = false;
            var cbManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);

            try
            {
                result = cbManager.Remove(key);
                if (result)
                    log.DebugFormat("Successfully removed {0}", key);
                else
                    log.ErrorFormat("Error while removing {0}", key);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing {0}. ex: {1}", key, ex);
            }

            return result;
        }

        public static async Task<bool> DeleteObjectFromCBAsync(eCouchbaseBucket couchbaseBucket, string key)
        {
            var result = false;
            var cbManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);

            try
            {
                result = await cbManager.RemoveAsync(key);
                if (result)
                    log.DebugFormat("Successfully removed {0}", key);
                else
                    log.ErrorFormat("Error while removing {0}", key);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing {0}. ex: {1}", key, ex);
            }

            return result;
        }

        public static async Task<bool> BulkDeleteObjectFromCBAsync(eCouchbaseBucket couchbaseBucket, IEnumerable<string> keys)
        {
            try
            {
                var finalResult = true;
                var cbManager = new CouchbaseManager.CouchbaseManager(couchbaseBucket);
                var r = new Random();
                foreach (var key in keys)
                {
                    var operationResult = await cbManager.RemoveAsync(key);
                    await Task.Delay(r.Next(50));
                    if (operationResult)
                    {
                        continue;
                    }

                    log.ErrorFormat("Bulk delete: failed to delete entry, key = {0}", key);
                    finalResult = false;
                }

                return finalResult;
            }
            catch (Exception ex)
            {
                log.Error("Bulk delete operation failed", ex);

                return false;
            }
        }

        public static DataTable Execute(string storedProcedure, Dictionary<string, object> parameters = null, string connectionKey = MAIN_CONNECTION_STRING)
        {
            StoredProcedure sp = new StoredProcedure(storedProcedure);
                        
            sp.SetConnectionKey(string.IsNullOrEmpty(connectionKey)? MAIN_CONNECTION_STRING : connectionKey);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    sp.AddParameter(param.Key, param.Value);
                }
            }

            return sp.Execute();
        }

        public static DataSet ExecuteDataSet(string storedProcedure, Dictionary<string, object> parameters, string connectionKey = MAIN_CONNECTION_STRING)
        {
            StoredProcedure sp = new StoredProcedure(storedProcedure);
            sp.SetConnectionKey(string.IsNullOrEmpty(connectionKey) ? MAIN_CONNECTION_STRING : connectionKey);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    sp.AddParameter(param.Key, param.Value);
                }
            }

            return sp.ExecuteDataSet();
        }

        public static T ExecuteReturnValue<T>(string storedProcedure, Dictionary<string, object> parameters, string connectionKey = MAIN_CONNECTION_STRING)
        {
            StoredProcedure sp = new StoredProcedure(storedProcedure);
            sp.SetConnectionKey(string.IsNullOrEmpty(connectionKey) ? MAIN_CONNECTION_STRING : connectionKey);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    sp.AddParameter(param.Key, param.Value);
                }
            }

            return sp.ExecuteReturnValue<T>();
        }

        #endregion

        #region Keys

        public static string GetUserMonthlyMediaMarksDocKey(string userId, DateTime createdAt)
        {
            return string.Format("u{0}_t{1}", userId, createdAt.ToString("yyyyMM"));
        }

        public static string GetUserAllAssetMarksDocKey(long userId)
        {
            return string.Format("u{0}", userId);
        }
        
        public static uint UserMediaMarksTtl => (uint)ApplicationConfiguration.Current.MediaMarksTTL.Value * 60 * 60 * 24;

        public static string GetUserMediaMarkDocKey(string siteUserGuid, int mediaId)
        {
            return string.Format("u{0}_m{1}", siteUserGuid, mediaId);
        }

        public static string GetUserNpvrMarkDocKey(long siteUserGuid, string npvrId)
        {
            return string.Format("u{0}_n{1}", siteUserGuid, npvrId);
        }

        public static string GetUserEpgMarkDocKey(long userID, long epgID)
        {
            return string.Format("u{0}_epg{1}", userID, epgID);
        }

        public static string GetPlayCycleKey(string siteGuid, int MediaFileID, int groupID, string UDID, int platform)
        {
            return string.Format("g{0}_u{1}_mf{2}_d{3}_p{4}", groupID, siteGuid, MediaFileID, UDID, platform);
        }

        public static string GetDomainQuotaKey(long domainId)
        {
            return string.Format("domain_{0}_quota", domainId);
        }

        public static string GetDomainRetryRecordingKey(long groupId, long epgId)
        {
            return $"group_{groupId}_Epg_{epgId}_Recording_Retry";
        }

        public static string GetFirstFollowerLockKey(int groupId, string seriesId, int seasonNumber, string channelId)
        {
            return string.Format("{0}_series{1}_season{2}_channel{3}", groupId, seriesId, seasonNumber, channelId);
        }

        public static string GetCachedEntitlementResultsKey(string version, long domainId, int mediaFileId)
        {
            return string.Format("version_{0}_domainId_{1}_mediaFileId_{2}", version, domainId, mediaFileId);
        }

        public static string MediaIdGroupFileTypesKey(int mediaID)
        {
            return string.Format("media_group_file_type_{0}", mediaID.ToString());
        }

        public static string GetDrmPolicyKey(int groupId)
        {
            return string.Format("drm_policy_{0}", groupId);
        }

        internal static string GetDomainDrmIdKey(int domainId)
        {
            return string.Format("domain_drmId_{0}", domainId);
        }

        internal static string GetDrmIdKey(string drmId, int groupId)
        {
            return string.Format("drmId_{0}_groupId_{1}", drmId, groupId);
        }

        internal static string GetSubscriptionSetModifyKey(int groupId, long id, SubscriptionSetModifyType type)
        {
            return string.Format("groupId_{0}_Id_{1}_type_{2}", groupId, id, type.ToString());
        }

        public static string GetDomainUnifiedBillingCycleKey(long domainId, long renewBillingCycle)
        {
            return string.Format("unifiedBillingCycle_householdId_{0}_cycle_{1}", domainId, renewBillingCycle);
        }

        public static string GetAssetUserRuleKey(long ruleId)
        {
            return string.Format("asset_user_rule_{0}", ruleId);
        }

        public static string GetDefaultQuotaInSecondsKey(int groupId, long domainId)
        {
            return string.Format("{0}_{1}", groupId, "DefaultQuotaSeconds");
        }

        internal static string GetPurchaseCouponRemainderKey(long purchaseId)
        {
            return string.Format("purchase_coupon_remainder_{0}", purchaseId);
        }

        internal static string GetRecurringRenewDetailsKey(long purchaseId)
        {
            return string.Format("recurring_renew_details_{0}", purchaseId);
        }

        public static string GetPartnerResetPasswordKey(int groupId)
        {
            return $"Partner_{groupId}_Reset_Password_Key";
        }

        #endregion

        private static void HandleException(Exception ex)
        {
            log.ErrorFormat("UtilsDal failure. message = {0}, ST = {1}", ex.Message, ex.StackTrace, ex);
            //throw new NotImplementedException();
        }

        public static DataRow GetModuleImpementationID(int nGroupID, int moduleID, string connectionKey)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(connectionKey);
                selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", moduleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = selectQuery.Table("query").DefaultView[0].Row;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static int GetModuleImplID(int nGroupID, int moduleID, string connectionKey)
        {
            int nImplID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(connectionKey);
                selectQuery += "select implementation_id from groups_modules_implementations with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", moduleID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nImplID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "implementation_id", 0);
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }

            return nImplID;
        }

        public static string GetModuleImplName(int nGroupID, int moduleID, string connectionKey, int operatorId = -1)
        {
            string moduleName = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            try
            {
                if (operatorId == -1)
                {
                    // regular user
                    selectQuery.SetConnectionKey(connectionKey);
                    selectQuery += "SELECT module_name FROM groups_modules_implementations with (nolock) WHERE is_active=1 AND status=1 AND ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += " AND ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", moduleID);
                }
                else
                {
                    // SSO user

                    // change to main DB
                    selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                    selectQuery += "SELECT * FROM groups_operators with (nolock) WHERE STATUS=1 AND IS_ACTIVE=1 AND ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += " AND ";
                    // if operator ID is 0 - take the default operator
                    if (operatorId != 0)
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", operatorId);
                    else
                        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_default", "=", 1);
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    DataTable dt = selectQuery.Table("query");
                    if (dt.DefaultView.Count > 0)
                        moduleName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "module_name", 0);
                }
            }
            finally
            {
                if (selectQuery != null)
                    selectQuery.Finish();
            }

            return moduleName;
        }

        public static DataRow GetEncrypterData(int nGroupID, string connectionKey)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(connectionKey);
                selectQuery += "select ENCRYPTER_IMPLEMENTATION from groups_parameters WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = selectQuery.Table("query").DefaultView[0].Row;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static int GetCountryIDFromIP(long nIPVal)
        {
            int nCountryID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("main_connection");
                selectQuery += "select top 1 COUNTRY_ID from ip_to_country WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "<=", nIPVal);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", ">=", nIPVal);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUNTRY_ID"].ToString().ToLower());
                        //nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString().ToLower());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nCountryID;
        }

        public static List<int> GetStatesByCountry(int nCountryID)
        {
            List<int> lStateIDs = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += " select ID from states WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("country_id", "=", nCountryID);
                selectQuery += " order by STATE_NAME";
                selectQuery.SetCachedSec(2678400);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        lStateIDs = new List<int>();
                    }
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        lStateIDs.Add(nID);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lStateIDs;
        }

        public static List<int> GetAllCountries()
        {
            List<int> ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += " select ID from countries WITH (nolock) order by COUNTRY_NAME";
                selectQuery.SetCachedSec(2678400);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = new List<int>();
                    }

                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        ret.Add(nID);
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static int GetGroupID(string sUN, string sPass, string sModuleName, string sIP, string sWSName)
        {
            int nGroupID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select group_id from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sModuleName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ")";
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nGroupID;
        }

        public static int GetGroupID(string sUN, string sPass, string sWSName)
        {
            int nGroupID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select group_id from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nGroupID;
        }

        public static string GetSecretCode(string sWSName, string sModuleName, string sUN, ref int nGroupID)
        {
            string sSecret = string.Empty;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select group_id,secret_code from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sModuleName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ") ";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "and ALLOW_CLIENT_SIDE=1";

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        if (selectQuery.Table("query").DefaultView[0].Row["secret_code"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["secret_code"] != DBNull.Value)
                        {
                            sSecret = selectQuery.Table("query").DefaultView[0].Row["secret_code"].ToString();
                        }

                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sSecret;
        }

        public static bool GetWSUNPass(int nGroupID, string sIP, string sWSFunctionName, string sWSName, ref string sWSUN, ref string sWSPassword)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select USERNAME,PASSWORD from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sWSFunctionName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ") and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sWSUN = selectQuery.Table("querY").DefaultView[0].Row["USERNAME"].ToString();
                        sWSPassword = selectQuery.Table("querY").DefaultView[0].Row["PASSWORD"].ToString();
                        res = true;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                res = false;
            }

            return res;
        }

        public static bool GetWSCredentials(int nGroupID, string sWSName, ref string sWSUN, ref string sWSPassword)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select USERNAME,PASSWORD from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sWSUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                        sWSPassword = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                        res = true;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                res = false;
            }

            return res;
        }

        public static bool GetAllWSCredentials(string sIP, ref DataTable modules)
        {
            bool res = false;
            modules = new DataTable();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select DISTINCT GROUP_ID, WS_NAME, USERNAME, PASSWORD from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        modules = selectQuery.Table("query");
                        res = true;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                res = false;
            }

            return res;
        }

        public static string GetIP2CountryCode(long nIPVal)
        {
            string sCountry = string.Empty;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select top 1 c.country_name , ipc.COUNTRY_ID,ipc.ID from ip_to_country ipc WITH (nolock), countries c WITH (nolock) where c.id = ipc.country_id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "<=", nIPVal);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", ">=", nIPVal);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sCountry = selectQuery.Table("query").DefaultView[0].Row["country_name"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sCountry;
        }

        public static List<int> GetAllRelatedGroups(int nGroupID)
        {
            List<int> lGroupIDs = new List<int>();
            lGroupIDs.Add(nGroupID);

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                selectQuery += "select id from groups WITH (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nCount; i++)
                    {
                        int nChildGroupID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());

                        //lGroupIDs.Add(nChildGroupID);
                        lGroupIDs.AddRange(GetAllRelatedGroups(nChildGroupID));
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lGroupIDs;
        }

        public static int GetOperatorGroupID(int nGroupID, string sOperatorCoGuid, ref int nOperatorID)
        {
            int nOperatorGroupID = 0;

            try
            {
                ODBCWrapper.StoredProcedure spGetOperatorGroupID = new ODBCWrapper.StoredProcedure(SP_GET_OPERATOR_GROUP_ID);
                spGetOperatorGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");

                spGetOperatorGroupID.AddParameter("@parentGroupID", nGroupID);
                spGetOperatorGroupID.AddParameter("@operatorID", sOperatorCoGuid);


                DataSet ds = spGetOperatorGroupID.ExecuteDataSet();


                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
                {
                    return nOperatorGroupID;
                }

                int nCount = ds.Tables[0].DefaultView.Count;

                if (nCount > 0)
                {
                    nOperatorID = int.Parse(ds.Tables[0].DefaultView[0].Row["ID"].ToString());
                    nOperatorGroupID = int.Parse(ds.Tables[0].DefaultView[0].Row["SUB_GROUP_ID"].ToString());
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nOperatorGroupID;

        }

        public static int GetParentGroupID(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spParentGroupID = new ODBCWrapper.StoredProcedure("GetParentGroupID");
            spParentGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spParentGroupID.AddParameter("@GroupID", nGroupID);

            int result = spParentGroupID.ExecuteReturnValue<int>();
            return result;
        }

        public static int GetLangGroupID(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spParentGroupID = new ODBCWrapper.StoredProcedure("GetLangGroupID");
            spParentGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spParentGroupID.AddParameter("@GroupID", nGroupID);

            int result = spParentGroupID.ExecuteReturnValue<int>();
            return result;
        }

        public static string getDomainMediaMarksDocKey(int nDomainID)
        {
            return string.Format("d{0}", nDomainID);
        }

        #region YES
        public static DataTable YesDeleteMediasByOfferID(string nOfferID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("sp_yesDeleteMediasByOfferID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@OfferID", nOfferID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable YesDeleteChannelsByOfferID()
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("sp_FixYesChannel");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        #endregion

        public static int Get_NPVRProviderID(long groupID, out bool synchronizeNpvrWithDomain, out int version)
        {
            int res = 0;
            version = 0;
            synchronizeNpvrWithDomain = false;
            StoredProcedure sp = new StoredProcedure("Get_NPVRProviderID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ParentGroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "npvr_provider_id");
                    synchronizeNpvrWithDomain = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "synchronize_npvr_with_domain") == 0 ? false : true;
                    version = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "npvr_version");
                }
            }

            return res;
        }

        public static IEnumerable<int> GetGroupsThatImplementFeature(string featureKey)
        {
            var query = new ODBCWrapper.DataSetSelectQuery();
            query += "select GROUP_ID from tvinci.dbo.groups_features g where STATUS = 1 and ";
            query += ODBCWrapper.Parameter.NEW_PARAM("FEATURE", "=", featureKey);
            query.Execute("query", true);
            
            var groupIds = new List<int>();
            if (query.Table("query").DefaultView.Count > 0)
            {
                var table = query.Table("query").Rows;
                foreach (DataRow row in table)
                {
                    var groupId = ODBCWrapper.Utils.ExtractInteger(row, "GROUP_ID");
                    groupIds.Add(groupId);
                }
            }

            return groupIds;
        }

        public static Dictionary<GroupFeature, bool> GetGroupFeatures(int groupId)
        {
            Dictionary<GroupFeature, bool> groupFeatures = null;
            ODBCWrapper.StoredProcedure spGetGroupFeatureStatus = new ODBCWrapper.StoredProcedure("GetGroupsFeatures");
            spGetGroupFeatureStatus.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetGroupFeatureStatus.AddParameter("@GroupId", groupId);

            DataTable dt = spGetGroupFeatureStatus.Execute();
            if (dt != null && dt.Rows != null)
            {
                groupFeatures = new Dictionary<GroupFeature, bool>();
                foreach (DataRow dr in dt.Rows)
                {
                    string feature = ODBCWrapper.Utils.GetSafeStr(dr, "FEATURE");
                    GroupFeature groupFeature;
                    if (Enum.TryParse(feature, true, out groupFeature) && !groupFeatures.ContainsKey(groupFeature))
                    {
                        int status = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS", 0);
                        groupFeatures.Add(groupFeature, status == 1);
                    }
                }
            }

            return groupFeatures;
        }
    }
}
