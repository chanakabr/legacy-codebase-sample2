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
using Tvinci.Core.DAL;


namespace DAL
{
    public class UtilsDal : BaseDal
    {
        private const string SP_GET_OPERATOR_GROUP_ID = "sp_GetGroupIDByOperatorID";
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;

        private static void HandleException(Exception ex)
        {
            log.ErrorFormat("UtilsDal failure. message = {0}, ST = {1}", ex.Message, ex.StackTrace, ex);
            //throw new NotImplementedException();
        }


        public static DataRow GetModuleImpementationID(int nGroupID, int moduleID)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

        public static int GetModuleImplID(int nGroupID, int moduleID, string connectionKey = "")
        {
            int nImplID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select implementation_id from groups_modules_implementations with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", moduleID);
                selectQuery.SetConnectionKey(connectionKey);

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

        public static string GetModuleImplName(int nGroupID, int moduleID, int operatorId = -1)
        {
            string moduleName = string.Empty;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            try
            {
                if (operatorId == -1)
                {
                    // regular user
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

        public static DataRow GetEncrypterData(int nGroupID)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

        public static string getUserMediaMarkDocKey(int nSiteUserGuid, int nMediaID)
        {
            return string.Format("u{0}_m{1}", nSiteUserGuid, nMediaID);
        }

        public static string getUserMediaMarkDocKey(string sSiteUserGuid, int nMediaID)
        {
            return string.Format("u{0}_m{1}", sSiteUserGuid, nMediaID);
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

        public static int Get_NPVRProviderID(long groupID)
        {
            int res = 0;
            StoredProcedure sp = new StoredProcedure("Get_NPVRProviderID");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@ParentGroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["npvr_provider_id"]);
                }
            }

            return res;
        }

        public static string getUserNpvrMarkDocKey(int nSiteUserGuid, string sNpvrID)
        {
            return string.Format("u{0}_n{1}", nSiteUserGuid, sNpvrID);

        }

        public static string getUserEpgMarkDocKey(int userID, string epgID)
        {
            return string.Format("u{0}_epg{1}", userID, epgID);
        }

        public static string GetPlayCycleKey(string siteGuid, int MediaFileID, int groupID, string UDID, int platform)
        {
            return string.Format("g{0}_u{1}_mf{2}_d{3}_p{4}", groupID, siteGuid, MediaFileID, UDID, platform);
        }

        public static string GetScheduledTaksKeyByType(ApiObjects.ScheduledTaskType scheduledTaskType)
        {
            string key = string.Empty;
            switch (scheduledTaskType)
            {
                case ApiObjects.ScheduledTaskType.recordingsLifetime:
                    key = "recordings_lifetime";
                    break;
                case ApiObjects.ScheduledTaskType.recordingsScheduledTasks:
                    key = "recordings_scheduledTasks";
                    break;
                case ApiObjects.ScheduledTaskType.recordingsCleanup:
                    key = "recordings_cleanup";
                    break;
                default:
                    break;
            }

            return key;
        }

        public static string GetDomainQuotaKey(long domainId)
        {
            return string.Format("domain_{0}_quota", domainId);
        }

        public static string GetDefaultQuotaInSeconds(int groupId, long domainId)
        {
            return string.Format("{0}_{1}", groupId, "DefaultQuotaSeconds");
        }

        public static string GetFirstFollowerLockKey(int groupId, string seriesId, int seasonNumber, string channelId)
        {
            return string.Format("{0}_series{1}_season{2}_channel{3}", groupId, seriesId, seasonNumber, channelId);
        }

        #region ScheduledTasks

        // return object incase someone will want to extend ScheduledTaskLastRunResponse
        private static object GetLastScheduleTaksSuccessfulRunDetails(ScheduledTaskType scheduledTaskType)
        {
            object response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string scheduledTaksKey = UtilsDal.GetScheduledTaksKeyByType(scheduledTaskType);
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed UtilsDal.GetScheduledTaksKeyByName for scheduledTaskName: {0}", scheduledTaskType.ToString());
                return response;
            }
            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<object>(scheduledTaksKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        log.ErrorFormat("Error while trying to get last successful scheduled task run date, KeyNotFound. scheduleTaskName: {0}, key: {1}", scheduledTaksKey, scheduledTaksKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("ScheduledTaskLastRunResponse with scheduleTaskName: {0} and key {1} was found", scheduledTaksKey, scheduledTaksKey);
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving ScheduledTaskLastRunResponse with scheduledTaskName: {0} and key {1} failed with status: {2}, retryAttempt: {3}, maxRetries: {4}", scheduledTaskType.ToString(), scheduledTaksKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get last successful schedule task details, scheduledTaskName: {0}, ex: {1}", scheduledTaskType.ToString(), ex);
            }

            return response;
        }

        // get object incase someone will want to extend ScheduledTaskLastRunResponse
        public static bool UpdateScheduledTaskSuccessfulRun(ScheduledTaskType scheduledTaskType, object scheduledTaskToUpdate)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string scheduledTaksKey = UtilsDal.GetScheduledTaksKeyByType(scheduledTaskType);
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed UtilsDal.GetScheduledTaksKeyByName for scheduledTaskName: {0}", scheduledTaskType);
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    object currentScheduledTask = cbClient.GetWithVersion<object>(scheduledTaksKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success || status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        result = cbClient.SetWithVersion<object>(scheduledTaksKey, scheduledTaskToUpdate, version);
                    }
                    
                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating successful scheduled task run details. scheduledTaskName: {0}, number of tries: {1}/{2}",
                                         scheduledTaskType.ToString(), numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating successful scheduled task run details, scheduledTaskName: {0}, ex: {1}", scheduledTaskType.ToString(), ex);
            }

            return result;
        }

        // return object incase someone will want to extend ScheduledTaskLastRunResponse
        public static object GetLastScheduleTaksSuccessfulRun(ScheduledTaskType scheduledTaskType)
        {
            object response = UtilsDal.GetLastScheduleTaksSuccessfulRunDetails(scheduledTaskType);
            if (response != null)
            {
                ScheduledTaskLastRunResponse scheduledTaskLastRunResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ScheduledTaskLastRunResponse>(response.ToString());
                if (scheduledTaskLastRunResponse.Status.Code != (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    log.ErrorFormat("Error while trying to get last scheduled task successful run details, scheduledTaskName: {0}, status code: {1}, status message: {2}",
                        scheduledTaskType, scheduledTaskLastRunResponse.Status.Code, scheduledTaskLastRunResponse.Status.Message);
                }
                else
                {
                    switch (scheduledTaskLastRunResponse.ScheduledTaskType)
                    {
                        case ScheduledTaskType.recordingsLifetime:
                        case ScheduledTaskType.recordingsScheduledTasks:
                        case ScheduledTaskType.recordingsCleanup:
                            return scheduledTaskLastRunResponse;
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        public static bool UpdateScheduledTaskNextRunIntervalInSeconds(ScheduledTaskType scheduledTaskType, double updatedNextRunIntervalInSeconds)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.SCHEDULED_TASKS);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string scheduledTaksKey = UtilsDal.GetScheduledTaksKeyByType(scheduledTaskType);
            if (string.IsNullOrEmpty(scheduledTaksKey))
            {
                log.ErrorFormat("Failed UtilsDal.GetScheduledTaksKeyByName for scheduledTaskName: {0}", scheduledTaskType);
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    ScheduledTaskLastRunResponse scheduledTask = cbClient.GetWithVersion<ScheduledTaskLastRunResponse>(scheduledTaksKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success)
                    {
                        scheduledTask.NextRunIntervalInSeconds = updatedNextRunIntervalInSeconds;
                        result = cbClient.SetWithVersion<ScheduledTaskLastRunResponse>(scheduledTaksKey, scheduledTask, version);
                    }
                    else if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        break;
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating scheduled task next run interval. scheduledTaskName: {0}, number of tries: {1}/{2}",
                                         scheduledTaskType.ToString(), numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating scheduled task next run interval, scheduledTaskName: {0}, ex: {1}", scheduledTaskType.ToString(), ex);
            }

            return result;
        }

        #endregion

    }
}
