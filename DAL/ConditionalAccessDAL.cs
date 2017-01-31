using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;
using ApiObjects;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;
using ApiObjects.TimeShiftedTv;

namespace DAL
{
    public class ConditionalAccessDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int RETRY_LIMIT = 5;
        private static readonly uint CACHED_ENTITLEMENT_RESULTS_TTL_SEC = TCMClient.Settings.Instance.GetValue<uint>("LicenseLinkCacheInSec");

        public static DataTable Get_MediaFileByProductCode(int nGroupID, string sProductCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaFileByProductCode");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@ProductCode", sProductCode);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_MediaFileFromCoGuid(int nGroupID, string sCoGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaFileFromCoGuid");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@coGuid", sCoGuid);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static string GetMediaFileCoGuid(int nGroupID, int nMediaFileID)
        {
            string sCoGuid = string.Empty;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MediaFileCoGuid");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@MediaFileID", nMediaFileID);

                sCoGuid = sp.ExecuteReturnValue<string>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sCoGuid;
        }

        public static bool InsertPPVPurchase(int nGroupID, string sSubCode, int nMediaFileID, string sSiteGUID, double dPrice, string sCurrency, int nNumOfUses, string sCustomData,int transactionID,
                                             string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int maxNumOfUses, int nIsActive, int nStatus, DateTime? dtEndDate, int domainId)
        {
            bool res = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                insertQuery.SetConnectionKey("CA_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                if (sSubCode != null)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
                }

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", domainId);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", nNumOfUses);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", transactionID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", maxNumOfUses);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);

                if (dtEndDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dtEndDate.Value);
                    //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", GetCurrentDBTime().AddSeconds(thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle));
                }

                res = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        private static void HandleException(Exception ex)
        {
            log.Error("HandleException occurred ", ex);
        }

        public static int GetPPVPurchaseID(int m_nGroupID, string subCode, int nMediaFileID, string sSiteGUID, double dPrice, string sCurrency, int nNumOfUses, int nMaxNumOfUses, int nIsActive, int nStatus)
        {
            int purchaseID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                selectQuery += " select id from ppv_purchases with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);

                if (subCode != null)
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", subCode);
                }

                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", nNumOfUses);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumOfUses);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                selectQuery += "order by id desc";

                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount1 = selectQuery.Table("query").DefaultView.Count;
                    if (nCount1 > 0)
                    {
                        purchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return purchaseID;
        }

        public static bool UpdatePurchaseID(int nTransactionID, int nPurchaseID)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("PURCHASE_ID", "=", nPurchaseID);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTransactionID);
                res = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static bool CancelTransaction(int nTransactionID, int nIsActive = 2, int nStatus = 2)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("billing_transactions");
                updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                updateQuery += "where";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTransactionID);
                res = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static DataView GetUserBillingHistory(string[] arrGroupIDs, string sUserGUID, int nTopNum, DateTime dStartDate, DateTime dEndDate, int orderBy)
        {
            DataView res = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                if (nTopNum > 0)
                {
                    selectQuery += string.Format("SELECT TOP {0} * FROM BILLING_TRANSACTIONS WITH (NOLOCK) WHERE BILLING_STATUS=0 AND ", nTopNum);
                }
                else
                {
                    selectQuery += "SELECT * FROM BILLING_TRANSACTIONS WITH (NOLOCK) WHERE BILLING_STATUS=0 AND ";
                }

                selectQuery += " GROUP_ID IN (" + string.Join(",", arrGroupIDs) + ") AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sUserGUID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", ">=", dStartDate);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "<=", dEndDate);

                if (orderBy == 0)
                {
                    selectQuery += " ORDER BY CREATE_DATE ASC";
                }
                else if (orderBy == 1)
                {
                    selectQuery += " ORDER BY CREATE_DATE DESC";
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        res = selectQuery.Table("query").DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
            }

            return res;

        }

        public static DataTable GetDomainBillingHistory(int groupID, int domainID, int topNum, DateTime startDate, DateTime endDate, int orderBy)
        {
            DataTable dt = null;            
            if (domainID > 0)
            {
                try
                {
                    StoredProcedure spGet_DomainBillingHistory = new ODBCWrapper.StoredProcedure("Get_DomainBillingHistory");
                    spGet_DomainBillingHistory.SetConnectionKey("MAIN_CONNECTION_STRING");                    
                    spGet_DomainBillingHistory.AddParameter("@GroupID", groupID);
                    spGet_DomainBillingHistory.AddParameter("@DomainID", domainID);
                    spGet_DomainBillingHistory.AddParameter("@StartDate", startDate);
                    spGet_DomainBillingHistory.AddParameter("@EndDate", endDate);
                    spGet_DomainBillingHistory.AddParameter("@orderBy", orderBy);
                    dt = spGet_DomainBillingHistory.Execute();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            return dt;
        }

        public static bool UpdateSubPurchase(int nGroupID, string sSiteGUID, string sSubscriptionCode, int nIsRecurring, int nSubscriptionPurchaseID = 0)
        {
            bool updated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", nIsRecurring);
                updateQuery += " WHERE ";

                if (nSubscriptionPurchaseID > 0)
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nSubscriptionPurchaseID);
                }
                else
                {
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                    updateQuery += " AND ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                }

                updated = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return updated;
        }

        public static bool CancelSubPurchase(int m_nGroupID, string sSubscriptionCode, int nPurchaseID, string sSiteGUID, int nIsActive = 2, int nStatus = 2)
        {
            bool updated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                updateQuery += " WHERE ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPurchaseID);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                updated = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return updated;
        }

        public static bool CancelPpvPurchase(int m_nGroupID, int nPurchaseID, string sSiteGUID, int nMediaFileID, int nIsActive, int nStatus)
        {
            bool updated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("ppv_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                updateQuery += " WHERE ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPurchaseID);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                updateQuery += " AND ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                updated = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return updated;
        }

        public static DataTable Get_All_Users_PPV_modules(List<int> usersIds, bool isExpired, int domainID, int orderBy = 0)
        {
            if (usersIds.Count > 0 || domainID > 0)
            {
                ODBCWrapper.StoredProcedure spGet_All_Users_PPV_modules = new ODBCWrapper.StoredProcedure("Get_UsersPermittedItems");
                spGet_All_Users_PPV_modules.SetConnectionKey("CA_CONNECTION_STRING");
                if (usersIds.Count > 0)
                {
                    spGet_All_Users_PPV_modules.AddIDListParameter<string>("@UserIDs", usersIds.Select(x => x.ToString()).ToList(), "Id");
                }
                else
                {
                    spGet_All_Users_PPV_modules.AddIDListParameter<string>("@UserIDs", new List<string>(), "Id");
                }
                spGet_All_Users_PPV_modules.AddParameter("@isExpired", isExpired);
                spGet_All_Users_PPV_modules.AddParameter("@DomainID", domainID);
                spGet_All_Users_PPV_modules.AddParameter("@OrderBy", orderBy);

                DataSet ds = spGet_All_Users_PPV_modules.ExecuteDataSet();

                if (ds != null)
                    return ds.Tables[0];
            }

            return null;
        }

        public static DataTable Get_UsersPermittedSubscriptions(List<int> usersIds, bool isExpired, int domainID, int orderBy = 0)
        {
            ODBCWrapper.StoredProcedure spGet_All_Users_Permitted_Subscriptions = new ODBCWrapper.StoredProcedure("Get_UsersPermittedSubscriptions");
            spGet_All_Users_Permitted_Subscriptions.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_All_Users_Permitted_Subscriptions.AddIDListParameter<int>("@UserIDs", usersIds, "Id");
            spGet_All_Users_Permitted_Subscriptions.AddParameter("@isExpired", isExpired);
            spGet_All_Users_Permitted_Subscriptions.AddParameter("@DomainID", domainID);
            spGet_All_Users_Permitted_Subscriptions.AddParameter("@OrderBy", orderBy);

            DataSet ds = spGet_All_Users_Permitted_Subscriptions.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UsersPermittedCollections(List<int> usersIds, bool isExpired, int domainID, int orderBy = 0)
        {
            ODBCWrapper.StoredProcedure spGet_UsersPermittedCollections = new ODBCWrapper.StoredProcedure("Get_UsersPermittedCollections");
            spGet_UsersPermittedCollections.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_UsersPermittedCollections.AddIDListParameter<int>("@UserIDs", usersIds, "Id");
            spGet_UsersPermittedCollections.AddParameter("@isExpired", isExpired);
            spGet_UsersPermittedCollections.AddParameter("@DomainID", domainID);
            spGet_UsersPermittedCollections.AddParameter("@OrderBy", orderBy);

            DataSet ds = spGet_UsersPermittedCollections.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static List<int> Get_MediaFileByID(List<int> relFileTypesStr, Int32 nMediaFileID, bool isThereFileTypes, int nMediaID)
        {
            List<int> res = null;

            ODBCWrapper.StoredProcedure spGet_MediaFileByID = new ODBCWrapper.StoredProcedure("Get_MediaFileByID");
            spGet_MediaFileByID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_MediaFileByID.AddParameter("@mediaFileID", nMediaFileID);
            spGet_MediaFileByID.AddIDListParameter<int>("@fileTypes", relFileTypesStr, "Id");
            spGet_MediaFileByID.AddParameter("@isThereFileTypes", isThereFileTypes);
            spGet_MediaFileByID.AddParameter("@mediaID", nMediaID);

            DataSet ds = spGet_MediaFileByID.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new List<int>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        res.Add(ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["ID"]));
                    }
                }
                else
                {
                    res = new List<int>(0);
                }
            }
            else
            {
                res = new List<int>(0);
            }

            return res;
        }

        public static DataTable Get_PreviewModuleIDsForEntitlementCalc(int nGroupID, string sSiteGUID, string sSubCode, DateTime dtStartLookFromThisDate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PreviewModuleIDsForEntitlementCalc");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGUID);
            sp.AddParameter("@SubscriptionCode", sSubCode);
            sp.AddParameter("@DateToStartLookFrom", dtStartLookFromThisDate.ToString("yyyy-MM-dd HH:mm:ss"));

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool Update_BusinessModulePurchaseIsActive(long lAdyenTransactionsID, bool bIsActive, int nPurchaseType, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_BusinessModulePurchaseIsActive");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@IDInAdyenTransactions", lAdyenTransactionsID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@PurchaseType", nPurchaseType);
            return sp.ExecuteReturnValue<bool>();

        }

        public static DataTable Get_allDomainsPPVUsesUsingCollection(List<int> usersList, int groupID, int MediaFileID, int nBoxsetID)
        {
            ODBCWrapper.StoredProcedure spGet_allDomainsPPVUsesUsingCollection = new ODBCWrapper.StoredProcedure("Get_allDomainsPPVUsesUsingCollection");
            spGet_allDomainsPPVUsesUsingCollection.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_allDomainsPPVUsesUsingCollection.AddIDListParameter<int>("@usersList", usersList, "Id");
            spGet_allDomainsPPVUsesUsingCollection.AddParameter("@groupID", groupID);
            spGet_allDomainsPPVUsesUsingCollection.AddParameter("@MediaFileID", MediaFileID);
            spGet_allDomainsPPVUsesUsingCollection.AddParameter("@relCollectionID", nBoxsetID);

            DataSet ds = spGet_allDomainsPPVUsesUsingCollection.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllUsersPurchases(List<int> UserIDs, List<int> FileIds, int fileID)
        {
            ODBCWrapper.StoredProcedure spGet_AllUsersPurchases = new ODBCWrapper.StoredProcedure("Get_AllUsersPurchases");
            spGet_AllUsersPurchases.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@FileIDs", FileIds, "Id");
            spGet_AllUsersPurchases.AddParameter("@nMediaFileID", fileID);

            DataSet ds = spGet_AllUsersPurchases.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool Get_AllUsersPurchases(List<int> p_lstUserIDs, List<int> p_lstFileIds, int p_nFileID, string p_sPPVCode, ref int p_nPPVID, ref string p_sSubCode,
            ref string p_sPPCode, ref int p_nWaiver, ref DateTime p_dCreateDate, ref string p_sPurchasedBySiteGuid, ref int p_nPurchasedAsMediaFileID, ref DateTime? p_dtStartDate, ref DateTime? p_dtEndDate, int domainID = 0)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure spGet_AllUsersPurchases = new ODBCWrapper.StoredProcedure("Get_AllUsersPurchases");
            spGet_AllUsersPurchases.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@UserIDs", p_lstUserIDs, "Id");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@FileIDs", p_lstFileIds, "Id");
            spGet_AllUsersPurchases.AddParameter("@nMediaFileID", p_nFileID);
            spGet_AllUsersPurchases.AddParameter("@DomainID", domainID);

            DataSet ds = spGet_AllUsersPurchases.ExecuteDataSet();
            
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    string sCustomData = string.Empty;

                    for (int i = 0; i < dt.Rows.Count && !res; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        sCustomData = ODBCWrapper.Utils.GetSafeStr(dr, "CUSTOMDATA");
                        if (sCustomData.IndexOf(string.Format("<ppvm>{0}</ppvm>", p_sPPVCode)) > 0)
                        {
                            res = true;

                            p_nPPVID = ODBCWrapper.Utils.GetIntSafeVal(dr["ID"]);
                            p_sSubCode = ODBCWrapper.Utils.GetSafeStr(dr["subscription_code"]);
                            p_sPPCode = ODBCWrapper.Utils.GetSafeStr(dr["rel_pp"]);
                            p_sPurchasedBySiteGuid = ODBCWrapper.Utils.GetSafeStr(dr["SITE_USER_GUID"]);
                            p_nPurchasedAsMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dr["MEDIA_FILE_ID"]);
                            //cancellation window 
                            p_nWaiver = ODBCWrapper.Utils.GetIntSafeVal(dr, "WAIVER");
                            p_dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
                            p_dtStartDate = ODBCWrapper.Utils.ExtractNullableDateTime(dr, "START_DATE");
                            p_dtEndDate = ODBCWrapper.Utils.ExtractNullableDateTime(dr, "END_DATE");  
                        }
                    }
                }
            }

            return res;
        }

        public static DataTable Get_AllSubscriptionInfoByUsersIDs(List<int> UserIDs, List<int> nFileTypes)
        {
            ODBCWrapper.StoredProcedure spGet_AllSubscriptionInfoByUsersIDs = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionInfoByUsersIDs");
            spGet_AllSubscriptionInfoByUsersIDs.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllSubscriptionInfoByUsersIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_AllSubscriptionInfoByUsersIDs.AddIDListParameter<int>("@fileTypesList", nFileTypes, "Id");


            DataSet ds = spGet_AllSubscriptionInfoByUsersIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllCollectionInfoByUsersIDs(List<int> UserIDs)
        {
            ODBCWrapper.StoredProcedure spGet_AllCollectionInfoByUsersIDs = new ODBCWrapper.StoredProcedure("Get_AllCollectionInfoByUsersIDs");
            spGet_AllCollectionInfoByUsersIDs.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllCollectionInfoByUsersIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");


            DataSet ds = spGet_AllCollectionInfoByUsersIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllSubscriptionInfoByUsersIDs(List<int> UserIDs)
        {
            ODBCWrapper.StoredProcedure spGet_AllSubscriptionInfoByUsersIDs = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionInfoByUsersIDs");
            spGet_AllSubscriptionInfoByUsersIDs.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllSubscriptionInfoByUsersIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");

            DataSet ds = spGet_AllSubscriptionInfoByUsersIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_SubscriptionBySubscriptionCodeAndUserIDs(List<int> UserIDs, string subscriptionCode, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure spGet_SubscriptionBySubscriptionCodeAndUserIDs = new ODBCWrapper.StoredProcedure("Get_SubscriptionBySubscriptionCodeAndUserIDs");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.AddParameter("@subscriptionCode", subscriptionCode);
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.AddParameter("@DomainID", domainID);

            DataSet ds = spGet_SubscriptionBySubscriptionCodeAndUserIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_CollectionByCollectionCodeAndUserIDs(List<int> UserIDs, string collectionCode, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure spGet_CollectionByCollectionCodeAndUserIDs = new ODBCWrapper.StoredProcedure("Get_CollectionByCollectionCodeAndUserIDs");
            spGet_CollectionByCollectionCodeAndUserIDs.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_CollectionByCollectionCodeAndUserIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_CollectionByCollectionCodeAndUserIDs.AddParameter("@collectionCode", collectionCode);
            spGet_CollectionByCollectionCodeAndUserIDs.AddParameter("@DomainID", domainID);

            DataSet ds = spGet_CollectionByCollectionCodeAndUserIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllPPVPurchasesByUserIDsAndMediaFileID(int nMediaFileID, List<int> UserIDs, int nGroupID, int domainID = 0)
        {
            if (UserIDs == null)
            {
                UserIDs = new List<int>();
            }
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllPPVPurchasesByUserIDsAndMediaFileID");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@nMediaFileID", nMediaFileID);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@DomainID", domainID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_SubUsesByUserListFileIDAndSubCode(List<int> UserIDs, string subCode, int nMediaFileID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGet_PPVUsesByUserListFileIDAndSubCode = new ODBCWrapper.StoredProcedure("Get_SubUsesByUserListFileIDAndSubCode");
            spGet_PPVUsesByUserListFileIDAndSubCode.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_PPVUsesByUserListFileIDAndSubCode.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@subscriptionCode", subCode);
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@mediaFileID", nMediaFileID);
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@groupID", nGroupID);


            DataSet ds = spGet_PPVUsesByUserListFileIDAndSubCode.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PreviewModuleDataForEntitlementCalc(int nGroupID, string sSiteGuid, string sSubCode, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PreviewModuleDataForEntitlementCalc");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@SubscriptionCode", sSubCode);
            sp.AddParameter("@DomainID", domainID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static long Insert_NewPPVPurchase(long groupID, long contentID, string siteGuid, double price, string currency, long maxNumOfUses, string customData, string subscriptionCode,
            long billingTransactionID, DateTime startDate, DateTime endDate, DateTime createAndUpdateDate, string country, string language, string deviceName, long householdID, string billingGuid = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewPPVPurchase");
            sp.SetConnectionKey("CA_CONNECTION_STRING");

            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@MediaFileID", contentID);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@Price", price);
            sp.AddParameter("@CurrencyCode", currency);
            sp.AddParameter("@NumOfUses", 0);
            sp.AddParameter("@MaxNumOfUses", maxNumOfUses);
            sp.AddParameter("@CustomData", customData);
            sp.AddParameter("@BillingTransactionID", billingTransactionID);
            sp.AddParameter("@StartDate", startDate);
            sp.AddParameter("@EndDate", endDate);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", createAndUpdateDate);
            sp.AddParameter("@CreateDate", createAndUpdateDate);
            sp.AddParameter("@CountryCode", country);
            sp.AddParameter("@LanguageCode", language);
            sp.AddParameter("@DeviceName", deviceName);
            sp.AddParameter("@domainID", householdID);
            sp.AddParameter("@billingGuid", billingGuid);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);
            sp.AddParameter("@LastViewDate", DBNull.Value);
            sp.AddParameter("@PublishDate", DBNull.Value);

            if (string.IsNullOrEmpty(subscriptionCode))
            {
                sp.AddParameter("@SubscriptionCode", string.Empty);

            }
            else
            {
                sp.AddParameter("@SubscriptionCode", subscriptionCode);
            }

            return sp.ExecuteReturnValue<long>();
        }

        public static long Insert_NewMPPPurchase(long lGroupID, string sSubscriptionCode, string sSiteGuid,
            double dPrice, string sCurrencyCode, string sCustomData, string sCountryCode, string sLanguageCode,
            string sDeviceName, long lMaxNumOfUses, long lViewLifeCycleSecs, bool bIsRecurringStatus,
            long lBillingTransactionID, long lPreviewModuleID, DateTime dtSubscriptionStartDate, DateTime dtSubscriptionEndDate,
            DateTime dtCreateAndUpdateDate, string sConnKey, int domainID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewMPPPurchase");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@SubscriptionCode", sSubscriptionCode);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@CustomData", sCustomData);
            sp.AddParameter("@NumOfUses", 0);
            sp.AddParameter("@MaxNumOfUses", lMaxNumOfUses);
            sp.AddParameter("@ViewLifeCycleSecs", lViewLifeCycleSecs);
            sp.AddParameter("@LastViewDate", DBNull.Value); // make sure it is correct
            sp.AddParameter("@StartDate", dtSubscriptionStartDate);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@EndDate", dtSubscriptionEndDate);
            sp.AddParameter("@IsRecurringStatus", bIsRecurringStatus ? 1 : 0);
            sp.AddParameter("@RecurringRuntimeStatus", 0);
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);
            sp.AddParameter("@Status", 1);
            sp.AddParameter("@Price", dPrice);
            sp.AddParameter("@CurrencyCode", sCurrencyCode);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@CreateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@PublishDate", DBNull.Value);
            sp.AddParameter("@CountryCode", sCountryCode);
            sp.AddParameter("@LanguageCode", sLanguageCode);
            sp.AddParameter("@DeviceName", sDeviceName);
            sp.AddParameter("@FailCount", 0);
            sp.AddParameter("@CollectionMetadata", 0);
            sp.AddParameter("@RelPp", DBNull.Value);
            sp.AddParameter("@NotificationSent", 0);
            sp.AddParameter("@PreviewModuleID", lPreviewModuleID);
            sp.AddParameter("@domainID", domainID);

            return sp.ExecuteReturnValue<long>();

        }

        public static DataTable Get_MPPsToRenew(DateTime dtMPPEndDateLessThanThis, long lGroupID, int nFailCount, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_MPPsToRenew");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@EndDate", dtMPPEndDateLessThanThis);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@FailCount", nFailCount);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static int Get_GroupFailCount(long lGroupID, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupFailCount");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", lGroupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static int Get_GroupFailCount(long lGroupID)
        {
            return Get_GroupFailCount(lGroupID, string.Empty);
        }

        public static void Update_MPPRenewalData(long lPurchaseID, bool bIsRecurringStatus, DateTime dtNewEndDate, long lNumOfUses, string sConnKey, string siteGuid = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPRenewalData");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@IsRecurringStatus", bIsRecurringStatus ? 1 : 0);
            sp.AddParameter("@EndDate", dtNewEndDate);
            sp.AddParameter("@NumOfUses", lNumOfUses);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", siteGuid);

            sp.ExecuteNonQuery();
        }

        public static void Update_SubscriptionPurchaseRenewalSiteGuid(int nGroupID, string billingGuid, int nPurchaseID, string siteGuid, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_SubscriptionPurchaseRenewalSiteGuid");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@BillingGuid", billingGuid);
            sp.AddParameter("@PurchaseID", nPurchaseID);
            sp.AddParameter("@SiteGuid", siteGuid);

            sp.ExecuteNonQuery();
        }

        public static void Update_MPPFailCountByPurchaseID(long lPurchaseID, bool bTrueForIncrementingByOneFalseForSettingNewValue, int nNewValue, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPFailCountByPurchaseID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@PurchaseID", lPurchaseID);
            if (bTrueForIncrementingByOneFalseForSettingNewValue)
            {
                sp.AddParameter("@IsIncrement", 1);
                sp.AddParameter("@NewValue", 0);
            }
            else
            {
                sp.AddParameter("@IsIncrement", 0);
                sp.AddParameter("@NewValue", nNewValue);
            }

            sp.ExecuteNonQuery();
        }

        public static void Update_MPPFailCountByPurchaseID(long lPurchaseID, bool bTrueForIncrementingByOneFalseForSettingNewValue, int nNewValue)
        {
            Update_MPPFailCountByPurchaseID(lPurchaseID, bTrueForIncrementingByOneFalseForSettingNewValue, nNewValue, string.Empty);
        }

        public static string Get_FirstDeviceUsedByPPVModule(int nMediaFileID, string sPPVModuleCode, List<int> usersList, out int numofRowsReturned)
        {
            string result = string.Empty;
            numofRowsReturned = 0;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_FirstDeviceUsedByPPVModule");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@MediaFileID", nMediaFileID);
            sp.AddParameter("@PPVModuleCode", sPPVModuleCode);
            sp.AddIDListParameter("@UsersList", usersList, "id");
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                numofRowsReturned = dt.Rows.Count;
                result = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["device_name"]);
            }
            return result;
        }

        public static DataTable GetSubscriptionPurchaseID(int nSubscriptionsPurchasesID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubscriptionPurchaseID");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@nSubPurchaseID", nSubscriptionsPurchasesID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static long CancelSubscription(int nSubscriptionsPurchasesID, int nGroupID, string sSiteGUID, string nSubscriptionCode, int subscriptionPurchaseStatus = 1)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelSubscription");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@nID", nSubscriptionsPurchasesID);
            sp.AddParameter("@SiteGUID", sSiteGUID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SubscriptionCode", nSubscriptionCode);
            // Thor version : new parameter for the status for cancel (Ok / fail / cancel )
            sp.AddParameter("@SubscriptionPurchaseStatus", subscriptionPurchaseStatus);

            return sp.ExecuteReturnValue<long>();
        }

        public static void Update_MPPIsRecurringStatus(bool bIsSwitchOnRecurringStatus, long lGroupID, string sSubCode,
            string sSiteGuid, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPIsRecurringStatus");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@IsRecurringStatus", bIsSwitchOnRecurringStatus ? 1 : 0);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@SubscriptionCode", sSubCode);
            sp.AddParameter("@SiteGuid", sSiteGuid);

            sp.ExecuteNonQuery();

        }

        public static bool Update_MPPIsActiveByMPPPurchaseID(long lPurchaseID, bool bIsActivate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPIsActiveByMPPPurchaseID");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@IsActivate", bIsActivate ? 1 : 0);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@MPPPurchaseID", lPurchaseID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool Update_SubscriptionPurchaseEndDate(int? nID, string sSiteGuid, int? nBillingTransID, DateTime endDate)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_SubscriptionPurchaseEndDate");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            if (nID.HasValue)
                sp.AddParameter("@SubscriptionPurchaseID", nID);
            if (nBillingTransID.HasValue)
                sp.AddParameter("@BillingTransactionID", nBillingTransID);
            sp.AddParameter("@SiteGUID", sSiteGuid);
            sp.AddParameter("@EndDate", endDate);

            return sp.ExecuteReturnValue<bool>();
        }

        public static long Insert_NewMColPurchase(long lGroupID, string sCollectionCode, string sSiteGuid,
            double dPrice, string sCurrencyCode, string sCustomData, string sCountryCode, string sLanguageCode,
            string sDeviceName, long lMaxNumOfUses, long lViewLifeCycleSecs,
            long lBillingTransactionID, DateTime dtCollectionStartDate, DateTime dtCollectionEndDate,
            DateTime dtCreateAndUpdateDate, string sConnKey, long domainID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewColPurchase");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@CollectionCode", sCollectionCode);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@CustomData", sCustomData);
            sp.AddParameter("@NumOfUses", 0);
            sp.AddParameter("@MaxNumOfUses", lMaxNumOfUses);
            sp.AddParameter("@ViewLifeCycleSecs", lViewLifeCycleSecs);
            sp.AddParameter("@LastViewDate", DBNull.Value); // make sure it is correct
            sp.AddParameter("@StartDate", dtCollectionStartDate);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@EndDate", dtCollectionEndDate);
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);
            sp.AddParameter("@Status", 1);
            sp.AddParameter("@Price", dPrice);
            sp.AddParameter("@CurrencyCode", sCurrencyCode);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@CreateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@PublishDate", DBNull.Value);
            sp.AddParameter("@CountryCode", sCountryCode);
            sp.AddParameter("@LanguageCode", sLanguageCode);
            sp.AddParameter("@DeviceName", sDeviceName);
            sp.AddParameter("@FailCount", 0);
            sp.AddParameter("@domainID", domainID);

            return sp.ExecuteReturnValue<long>();

        }

        public static bool Update_BillingMethodInBillingTransactions(int nID, int nBillingMethod)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_BillingMethodInBillingTransactions");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@BillingTransID", nID);
            sp.AddParameter("@BillingMethod", nBillingMethod);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool CancelPPVPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelPPVPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool CancelSubscriptionPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0, int subscriptionPurchaseStatus = 2)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelSubscriptionPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);
            sp.AddParameter("@SubscriptionPurchaseStatus", subscriptionPurchaseStatus);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool CancelCollectionPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelCollectionPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverPPVPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverPPVPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverSubscriptionPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverSubscriptionPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverCollectionPurchaseTransaction(string sSiteGuid, int nAssetID, int domainID = 0)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverCollectionPurchaseTransaction");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static Dictionary<int, int> Get_GroupMediaTypesIDs(int nGroupID, string sConnKey)
        {
            Dictionary<int, int> res = null;
            StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_GroupMediaTypesIDs");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = new Dictionary<int, int>(dt.Rows.Count);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["ID"]);
                        int mediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["MEDIA_TYPE_ID"]);
                        res.Add(id, mediaTypeID);
                    }
                }
                else
                {
                    res = new Dictionary<int, int>(0);
                }
            }
            else
            {
                res = new Dictionary<int, int>(0);
            }

            return res;
        }

        public static Dictionary<int, int> Get_GroupMediaTypesIDs(int nGroupID)
        {
            return Get_GroupMediaTypesIDs(nGroupID, string.Empty);
        }

        public static int Get_SubscriptionUseCount(string sSiteGuid, string sSubCode, int nGroupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_SubscriptionUseCount");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@SubCode", sSubCode);
            sp.AddParameter("@GroupID", nGroupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static int Get_PPVPurchaseCount(int nGroupID, string sSiteGuid, string sSubCode, long lMediaFileID)
        {
            StoredProcedure sp = new StoredProcedure("Get_PPVPurchaseCount");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            if (!string.IsNullOrEmpty(sSubCode))
            {
                sp.AddParameter("@SubCode", sSubCode);
            }
            else
            {
                sp.AddParameter("@SubCode", DBNull.Value);
            }
            sp.AddParameter("@MediaFileID", lMediaFileID);

            return sp.ExecuteReturnValue<int>();
        }

        public static bool Get_LatestCreateDateOfBundleUses(string sBundleCode, int nGroupID, List<int> userIDs, List<int> relatedMediaFiles,
            bool bIsSub, ref DateTime dtCreateDateOfBundleUse, ref DateTime dtNow)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_LatestCreateDateOfBundleUses");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@BundleCode", sBundleCode);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter<string>("@UsersInDomain", userIDs.Select(item => item.ToString()).ToList<string>(), "Id");
            sp.AddIDListParameter<int>("@RelatedMediaFiles", relatedMediaFiles, "Id");
            sp.AddParameter("@IsSubscription", bIsSub);


            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    dtCreateDateOfBundleUse = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["CREATE_DATE"]);
                    dtNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["DATE_NOW"]);
                    res = true;
                }
            }
            return res;
        }

        public static DataSet Get_AllBundlesInfoByUserIDs(List<int> lstUsers, List<int> lstFileTypes, int nGroupID, int domainID = 0)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllBundlesInfoByUserIDs");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@Users", lstUsers, "ID");
            sp.AddIDListParameter("@FileTypes", lstFileTypes, "ID");
            sp.AddParameter("@Group_id", nGroupID);
            sp.AddParameter("@DomainID", domainID);

            return sp.ExecuteDataSet();
        }

        public static bool Get_LatestCreateDateOfBundlesUses(List<string> subscriptionsIDs, List<string> collectionsIDs,
            List<string> domainUserIDs, List<int> relatedMediaFileIDs, int nGroupID, ref Dictionary<string, DateTime> subsToCreateDateMapping,
            ref Dictionary<string, DateTime> colsToCreateDateMapping, ref DateTime dateNowDBTime)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_LatestCreateDateOfBundlesUses");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@Subscriptions", subscriptionsIDs, "ID");
            sp.AddIDListParameter("@Collections", collectionsIDs, "ID");
            sp.AddIDListParameter("@DomainUserIDs", domainUserIDs, "ID");
            sp.AddIDListParameter("@RelatedMediaFileIDs", relatedMediaFileIDs, "ID");
            sp.AddParameter("@GroupID", nGroupID);


            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    subsToCreateDateMapping = new Dictionary<string, DateTime>();
                    colsToCreateDateMapping = new Dictionary<string, DateTime>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string bundleCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["BUNDLE_CODE"]);
                        if (bundleCode.Length > 0)
                        {
                            DateTime latestCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["LATEST_CREATE_DATE"]);
                            bool isSub = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["BUNDLE_TYPE"]) == 0;
                            if (isSub)
                            {
                                subsToCreateDateMapping.Add(bundleCode, latestCreateDate);
                            }
                            else
                            {
                                colsToCreateDateMapping.Add(bundleCode, latestCreateDate);
                            }
                        }

                    } // end for
                }

                DataTable dbTime = ds.Tables[1];
                if (dbTime != null && dbTime.Rows != null && dbTime.Rows.Count > 0)
                {
                    dateNowDBTime = ODBCWrapper.Utils.GetDateSafeVal(dbTime.Rows[0]["DATE_NOW"]);
                }
                if (dateNowDBTime.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
                    dateNowDBTime = DateTime.UtcNow;
            }

            return res;
        }

        public static bool Get_AllDomainsPPVUsesUsingCollections(List<int> lstDomainUsers,
            int nGroupID, int nMediaFileID, List<int> lstCollections, ref DateTime dbTimeNow, ref Dictionary<int, DateTime> initializedDictCollectionUses)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_AllDomainsPPVUsesUsingCollections");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@Users", lstDomainUsers, "ID");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@MediaFileID", nMediaFileID);
            sp.AddIDListParameter("@Collections", lstCollections, "ID");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int collectionID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["rel_box_set"]);
                        DateTime latestCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["LATEST_CREATE_DATE"]);
                        if (!initializedDictCollectionUses.ContainsKey(collectionID) && !latestCreateDate.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
                        {
                            initializedDictCollectionUses.Add(collectionID, latestCreateDate);
                        }
                    }
                }

                DataTable dtTimeTable = ds.Tables[1];
                if (dtTimeTable != null && dtTimeTable.Rows != null && dtTimeTable.Rows.Count > 0)
                {
                    dbTimeNow = ODBCWrapper.Utils.GetDateSafeVal(dtTimeTable.Rows[0]["DATE_NOW"]);
                }
                if (dbTimeNow.Equals(ODBCWrapper.Utils.FICTIVE_DATE))
                {
                    dbTimeNow = DateTime.UtcNow;
                }
            }

            return res;

        }

        public static Dictionary<string, string[]> Get_MultipleWSCredentials(int groupID, List<string> services)
        {
            Dictionary<string, string[]> res = new Dictionary<string, string[]>();
            for (int i = 0; i < services.Count; i++)
            {
                if (!res.ContainsKey(services[i]))
                {
                    res.Add(services[i], new string[2] { string.Empty, string.Empty });
                }
            }
            StoredProcedure sp = new StoredProcedure("Get_MultipleWSCredentials");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddIDListParameter("@Services", services, "ID");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string serviceName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["ws_name"]);
                        string serviceUser = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["username"]);
                        string servicePass = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["password"]);
                        res[serviceName][0] = serviceUser;
                        res[serviceName][1] = servicePass;
                    }
                }
            }


            return res;
        }

        public static DataTable Get_AllSubscriptionPurchasesByUserIDsAndSubscriptionCode(int nSubscriptionCode, List<int> UserIDs, int nGroupID, int domainID = 0)
        {
            if (UserIDs == null)
            {
                UserIDs = new List<int>();
            }
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionPurchasesByUserIDsAndSubscriptionCode");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@SubscriptionCode", nSubscriptionCode);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@DomainID", domainID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllCollectionPurchasesByUserIDsAndCollectionCode(int nCollectionCode, List<int> UserIDs, int nGroupID, int domainID = 0)
        {
            if (UserIDs == null)
            {
                UserIDs = new List<int>();
            }
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllCollectionPurchasesByUserIDsAndCollectionCode");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@CollectionCode", nCollectionCode);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@DomainID", domainID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static string Get_LicensedLinkSecretCode(long groupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_LicensedLinkSecretCode");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null &&
                ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                return ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0]["SECRET_CODE"]);
            }

            return string.Empty;
        }

        public static bool Get_GroupSecretAndCountryCode(long groupID, ref string secretCode, ref string countryCode)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_GroupSecretAndCountryCode");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                res = true;
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    secretCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["GROUP_SECRET_CODE"]);
                    countryCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["GROUP_COUNTRY_CODE"]);
                }
            }

            return res;
        }

        public static int Get_MediaFileStreamingCoID(string mediaFileIDStr, bool isCoGuid)
        {
            int nStreamingCoID = 0;

            ODBCWrapper.StoredProcedure Get_MediaFileStreamingCoID = new ODBCWrapper.StoredProcedure("Get_MediaFileStreamingCoID");
            Get_MediaFileStreamingCoID.SetConnectionKey("MAIN_CONNECTION_STRING");
            Get_MediaFileStreamingCoID.AddParameter("@MediaFileID", mediaFileIDStr);
            Get_MediaFileStreamingCoID.AddParameter("@IsCoGuid", isCoGuid);

            System.Data.DataSet ds = Get_MediaFileStreamingCoID.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
            {
                nStreamingCoID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
            }

            return nStreamingCoID;
        }

        public static bool Get_BasicLinkData(long mediaFileID, ref string baseUrl, ref string streamingCode, ref int streamingCompanyID, ref string fileType)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_BasicLinkData");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaFileID", mediaFileID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                res = true;
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    baseUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["VIDEO_BASE_URL"]);
                    streamingCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["STREAMING_CODE"]);
                    streamingCompanyID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["STREAMING_SUPLIER_ID"]);
                    fileType = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["DESCRIPTION"]);
                }
            }

            return res;
        }

        public static bool Get_IsLastViewData(long ppvPurchaseID, ref int numOfUses, ref int maxNumOfUses, ref DateTime endDate)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_IsLastViewData");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@PPVPurchaseID", ppvPurchaseID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    endDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["END_DATE"]);
                    numOfUses = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["NUM_OF_USES"]);
                    maxNumOfUses = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["MAX_NUM_OF_USES"]);
                }
            }

            return res;
        }

        public static bool Update_PPVNumOfUses(long ppvPurchaseID, DateTime? newEndDate)
        {
            StoredProcedure sp = new StoredProcedure("Update_PPVNumOfUses");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@PPVPurchaseID", ppvPurchaseID);
            if (newEndDate == null)
            {
                sp.AddParameter("@NewEndDate", DBNull.Value);
            }
            else
            {
                sp.AddParameter("@NewEndDate", newEndDate.Value);
            }

            return sp.ExecuteReturnValue<bool>();
        }

        public static DataTable Get_AllPPVPurchasesByUserIDsAndMediaFileIDs(long groupID, List<int> relatedMediaFileIDs,
            List<int> userIDs, int domainID = 0)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllPPVPurchasesByUserIDsAndMediaFileIDs");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@RelatedMediaFileIDs", relatedMediaFileIDs, "Id");
            sp.AddIDListParameter<int>("@UserIDs", userIDs, "Id");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@DomainID", domainID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllDomainPPVUsesByMediaFiles(long groupID, List<int> usersInDomain, List<int> relatedMediaFileIDs, string sPPVMCd)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllDomainPPVUsesByMediaFiles");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@UsersInDomain", usersInDomain, "Id");
            sp.AddIDListParameter<int>("@RelatedMediaFileIDs", relatedMediaFileIDs, "Id");
            sp.AddParameter("@GroupID", groupID);
            if (!string.IsNullOrEmpty(sPPVMCd))
            {
                sp.AddParameter("@PPVModuleCode", sPPVMCd);
            }
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool Get_LatestMediaFilesUse(List<int> usersList, List<int> mediaFileIDs, ref string ppvModuleCode,
            ref bool isOfflineStatus, ref DateTime dateNow, ref DateTime purchaseDate)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_LatestMediaFilesUse");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter<int>("@UsersList", usersList, "Id");
            sp.AddIDListParameter<int>("@MediaFileIDs", mediaFileIDs, "Id");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    ppvModuleCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["ppvmodule_code"]);
                    isOfflineStatus = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["offline_status"]) == 1;
                    dateNow = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["dNow"]);
                    purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["CREATE_DATE"]);
                }
            }

            return res;
        }

        public static long Insert_NewPPVUse(long groupID, long mediaFileID, string ppvModuleCode, string siteGuid, bool isCreditDownloaded,
            string countryCode, string langCode, string deviceName, int relPp, int relBoxSet)
        {
            StoredProcedure sp = new StoredProcedure("Insert_NewPPVUse");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@PPVModuleCode", ppvModuleCode);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@IsCreditDownloaded", isCreditDownloaded ? 1 : 0);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);
            if (countryCode == null)
            {
                sp.AddParameter("@CountryCode", string.Empty);
            }
            else
            {
                sp.AddParameter("@CountryCode", countryCode);
            }
            if (langCode == null)
            {
                sp.AddParameter("@LangCode", string.Empty);
            }
            else
            {
                sp.AddParameter("@LangCode", langCode);
            }
            if (deviceName == null)
            {
                sp.AddParameter("@DeviceName", string.Empty);
            }
            else
            {
                sp.AddParameter("@DeviceName", deviceName);
            }
            sp.AddParameter("@RelPP", relPp);
            sp.AddParameter("@RelBoxSet", relBoxSet);

            return sp.ExecuteReturnValue<long>();
        }

        public static long Insert_NewSubscriptionUse(long groupID, string subCode, long mediaFileID, string siteGuid, bool isCreditDownloaded,
            string countryCode, string langCode, string deviceName, int relPP)
        {
            StoredProcedure sp = new StoredProcedure("Insert_NewSubscriptionUse");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@SubCode", subCode);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@IsCreditDownloaded", isCreditDownloaded ? 1 : 0);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);
            if (countryCode == null)
            {
                sp.AddParameter("@CountryCode", string.Empty);
            }
            else
            {
                sp.AddParameter("@CountryCode", countryCode);
            }
            if (langCode == null)
            {
                sp.AddParameter("@LangCode", string.Empty);
            }
            else
            {
                sp.AddParameter("@LangCode", langCode);
            }
            if (deviceName == null)
            {
                sp.AddParameter("@DeviceName", string.Empty);
            }
            else
            {
                sp.AddParameter("@DeviceName", deviceName);
            }
            sp.AddParameter("@RelPP", relPP);

            return sp.ExecuteReturnValue<long>();
        }

        public static bool Update_SubPurchaseNumOfUses(long groupID, string siteGuid, string subCode)
        {
            StoredProcedure sp = new StoredProcedure("Update_SubPurchaseNumOfUses");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@SubCode", subCode);

            return sp.ExecuteReturnValue<bool>();
        }

        public static long Insert_NewCollectionUse(long groupID, string collCode, long mediaFileID, string siteGuid, bool isCreditDownloaded,
            string countryCode, string langCode, string deviceName)
        {
            StoredProcedure sp = new StoredProcedure("Insert_NewCollectionUse");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@CollectionCode", collCode);
            sp.AddParameter("@MediaFileID", mediaFileID);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@IsCreditDownloaded", isCreditDownloaded ? 1 : 0);
            if (countryCode != null)
                sp.AddParameter("@CountryCode", countryCode);
            else
                sp.AddParameter("@CountryCode", string.Empty);
            if (langCode != null)
                sp.AddParameter("@LangCode", langCode);
            else
                sp.AddParameter("@LangCode", string.Empty);
            if (deviceName != null)
                sp.AddParameter("@DeviceName", deviceName);
            else
                sp.AddParameter("@DeviceName", string.Empty);

            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);

            return sp.ExecuteReturnValue<long>();
        }

        public static bool Update_ColPurchaseNumOfUses(string colCode, string siteGuid, long groupID)
        {
            StoredProcedure sp = new StoredProcedure("Update_ColPurchaseNumOfUses");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@ColCode", colCode);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@GroupID", groupID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static int GetStreamingUrlType(int fileMainStreamingCoID, ref string CdnStrID)
        {
            int nUrlType = 0;
            StoredProcedure sp = new StoredProcedure("Get_StreamingCoUrlType");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@StreamingCoID", fileMainStreamingCoID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    nUrlType = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "URL_TYPE"); // type of url dynamic or static
                    CdnStrID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "CDN_STR_ID"); // streaming provider name 
                }
            }

            return nUrlType;
        }        

        public static bool GetFileUrlLinks(int mediaFileID, string siteGuid, int groupID, ref string mainUrl, ref string altUrl, ref int mainStreamingCoID, ref int altStreamingCoID, ref int mediaID)
        {
            bool success = false;

            StoredProcedure sp = new StoredProcedure("Get_FileUrlLinks");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@MediaFileID", mediaFileID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    mainUrl = ODBCWrapper.Utils.GetSafeStr(dr, "mainUrl");
                    altUrl = ODBCWrapper.Utils.GetSafeStr(dr, "altUrl");
                    mainStreamingCoID = ODBCWrapper.Utils.GetIntSafeVal(dr, "CdnID");
                    altStreamingCoID = ODBCWrapper.Utils.GetIntSafeVal(dr, "AltCdnID");
                    mediaID = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_id");
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// For a given co guid, find the corresponding media file Id. Returns true if found, false if not
        /// </summary>
        /// <param name="coGuid"></param>
        /// <param name="groupID"></param>
        /// <param name="mediaFileID"></param>
        /// <returns></returns>
        public static bool Get_MediaFileIDByCoGuid(string coGuid, int groupID, ref int mediaFileID)
        {
            bool success = false;

            StoredProcedure sp = new StoredProcedure("Get_MediaFileIDByCoGuid");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@CoGuid", coGuid);
            sp.AddParameter("@GroupID", groupID);

            mediaFileID = Convert.ToInt32(sp.ExecuteReturnValue());

            if (mediaFileID > 0)
            {
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Returns a table of all subscription purchases made by the list of users and with the given subscription code
        /// </summary>
        /// <param name="p_lstUsers"></param>
        /// <param name="p_sSubscriptionCode"></param>
        /// <returns></returns>
        public static DataTable Get_UsersSubscriptionPurchases(List<int> p_lstUsers, string p_sSubscriptionCode, int domainID = 0)
        {
            DataTable dtUserPurchases = null;
            StoredProcedure spStoredProcedure = new StoredProcedure("Get_UsersSubscriptionPurchases");
            spStoredProcedure.SetConnectionKey("CA_CONNECTION_STRING");
            spStoredProcedure.AddIDListParameter<int>("@UserIDs", p_lstUsers, "Id");
            spStoredProcedure.AddParameter("@SubscriptionCode", p_sSubscriptionCode);
            spStoredProcedure.AddParameter("@DomainID", domainID);

            DataSet dsStoredProcedureResult = spStoredProcedure.ExecuteDataSet();

            // If stored procedure was succesful, get the first one
            if (dsStoredProcedureResult != null && dsStoredProcedureResult.Tables.Count == 1)
            {
                dtUserPurchases = dsStoredProcedureResult.Tables[0];
            }

            return (dtUserPurchases);
        }

        public static long Get_LastDomainDLM(int groupID, long domainID)
        {
            long dlmID = 0;

            StoredProcedure sp = new StoredProcedure("Get_LastDomainDLM");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@DomainID", domainID);

            object value = sp.ExecuteReturnValue<long>();

            // If stored procedure was succesful, get the first one
            if (value != null)
            {
                dlmID = (long)value;
            }

            return dlmID;
        }

        public static List<int> GetFileIdsByEpgProgramId(int epgProgramId, long groupId)
        {
            List<int> fileIds = new List<int>();

            ODBCWrapper.StoredProcedure spGet_MediaFileByID = new ODBCWrapper.StoredProcedure("Get_FilesByEpgProgramId");
            spGet_MediaFileByID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_MediaFileByID.AddParameter("@GroupId", groupId);
            spGet_MediaFileByID.AddParameter("@ProgramId", epgProgramId);

            DataSet ds = spGet_MediaFileByID.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        fileIds.Add(ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["ID"]));
                }
            }

            return fileIds;
        }

        public static long Insert_NewMPPPurchase(int groupID, string subscriptionCode, string siteGUID, double price, string currency, string customData, 
            string country, string deviceName, int maxNumOfUses, int viewLifeCycle,
            bool isRecurring, long billingTransactionID, long previewModuleID, DateTime subscriptionStartDate, DateTime subscriptionEndDate, 
            DateTime createAndUpdateDate, long householdId, string billingGuid, int purchaseStatus = 0, string coupon = null)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewMPPPurchase");
            sp.SetConnectionKey("CA_CONNECTION_STRING");

            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@SubscriptionCode", subscriptionCode);
            sp.AddParameter("@SiteGuid", siteGUID);
            sp.AddParameter("@CustomData", customData);
            sp.AddParameter("@MaxNumOfUses", maxNumOfUses);
            sp.AddParameter("@ViewLifeCycleSecs", viewLifeCycle);
            sp.AddParameter("@LastViewDate", DBNull.Value); // make sure it is correct
            sp.AddParameter("@StartDate", subscriptionStartDate);
            sp.AddParameter("@EndDate", subscriptionEndDate);
            sp.AddParameter("@IsRecurringStatus", isRecurring ? 1 : 0);
            sp.AddParameter("@BillingTransactionID", billingTransactionID);
            sp.AddParameter("@Price", price);
            sp.AddParameter("@CurrencyCode", currency);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", createAndUpdateDate);
            sp.AddParameter("@CreateDate", createAndUpdateDate);
            sp.AddParameter("@CountryCode", country);
            sp.AddParameter("@DeviceName", deviceName);
            sp.AddParameter("@PreviewModuleID", previewModuleID);
            sp.AddParameter("@domainID", householdId);
            sp.AddParameter("@billingGuid", billingGuid);
            sp.AddParameter("@PurchaseStatus", purchaseStatus);
            sp.AddParameter("@CouponCode", coupon);

            return sp.ExecuteReturnValue<long>();
        }

        public static long Insert_NewMColPurchase(int groupID, string collectionCode, string siteGUID, double price, string currency, string customData,
                                                  string country, string deviceName, int maxNumOfUses, int viewLifeCycle, long billingTransactionID,
                                                  DateTime collectionStartDate, DateTime collectionEndDate, DateTime createAndUpdateDate, long householdId,
                                                  string billingGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewColPurchase");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@CollectionCode", collectionCode);
            sp.AddParameter("@SiteGuid", siteGUID);
            sp.AddParameter("@CustomData", customData);
            sp.AddParameter("@MaxNumOfUses", maxNumOfUses);
            sp.AddParameter("@ViewLifeCycleSecs", viewLifeCycle);
            sp.AddParameter("@StartDate", collectionStartDate);
            sp.AddParameter("@EndDate", collectionEndDate);
            sp.AddParameter("@BillingTransactionID", billingTransactionID);
            sp.AddParameter("@Price", price);
            sp.AddParameter("@CurrencyCode", currency);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", createAndUpdateDate);
            sp.AddParameter("@CreateDate", createAndUpdateDate);
            sp.AddParameter("@CountryCode", country);
            sp.AddParameter("@DeviceName", deviceName);
            sp.AddParameter("@domainID", householdId);
            sp.AddParameter("@billingGuid", billingGuid);

            return sp.ExecuteReturnValue<long>();
        }

        public static bool UpdatePPVPurchaseActiveStatus(string billingGuid, int isActive)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PPVPurchaseActiveStatus");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@is_active", isActive);
            sp.AddParameter("@billing_guid", billingGuid);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool UpdateSubscriptionPurchaseActiveStatus(string billingGuid, int isActive, int isRecurringStatus)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PPVPurchaseActiveStatus");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@is_active", isActive);
            sp.AddParameter("@billing_guid", billingGuid);
            sp.AddParameter("@is_recurring_status", isRecurringStatus);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool UpdateCollectionPurchaseActiveStatus(string billingGuid, int isActive)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PPVPurchaseActiveStatus");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@is_active", isActive);
            sp.AddParameter("@billing_guid", billingGuid);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static DataRow Get_RenewDetails(int groupId, long subscriptionPurchaseId, string billingGuid)
        {
            ODBCWrapper.StoredProcedure spLastBillingTransactions = new ODBCWrapper.StoredProcedure("Get_RenewDetails");
            spLastBillingTransactions.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLastBillingTransactions.AddParameter("@GroupID", groupId);
            spLastBillingTransactions.AddParameter("@PurchaseId", subscriptionPurchaseId);
            spLastBillingTransactions.AddParameter("@BillingGuid", billingGuid);

            DataSet ds = spLastBillingTransactions.ExecuteDataSet();
            if (ds != null &&
                ds.Tables != null &&
                ds.Tables.Count > 0 &&
                ds.Tables[0].Rows != null &&
                ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0];
            }

            return null;
        }

        public static DataRow Get_SubscriptionPurchaseForRenewal(int groupId, long subscriptionPurchaseId, string billingGuid)
        {
            ODBCWrapper.StoredProcedure spLastBillingTransactions = new ODBCWrapper.StoredProcedure("Get_SubscriptionPurchaseForRenewal");
            spLastBillingTransactions.SetConnectionKey("CA_CONNECTION_STRING");
            spLastBillingTransactions.AddParameter("@GroupID", groupId);
            spLastBillingTransactions.AddParameter("@PurchaseId", subscriptionPurchaseId);
            spLastBillingTransactions.AddParameter("@BillingGuid", billingGuid);

            DataSet ds = spLastBillingTransactions.ExecuteDataSet();
            if (ds != null &&
                ds.Tables != null &&
                ds.Tables.Count > 0 &&
                ds.Tables[0].Rows != null &&
                ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0];
            }

            return null;
        }

        public static bool Update_SubscriptionPurchaseRenewalActiveStatus(int groupId, long subscriptionPurchaseId, string billingGuid, int isActive)
        {
            ODBCWrapper.StoredProcedure spLastBillingTransactions = new ODBCWrapper.StoredProcedure("Update_SubscriptionPurchaseRenewalActiveStatus");
            spLastBillingTransactions.SetConnectionKey("CA_CONNECTION_STRING");
            spLastBillingTransactions.AddParameter("@GroupID", groupId);
            spLastBillingTransactions.AddParameter("@PurchaseId", subscriptionPurchaseId);
            spLastBillingTransactions.AddParameter("@BillingGuid", billingGuid);
            spLastBillingTransactions.AddParameter("@IsActive", isActive);

            return spLastBillingTransactions.ExecuteReturnValue<int>() > 0;
        }

        public static Dictionary<string, EntitlementObject> Get_AllUsersEntitlements(int domainID, List<int> lstUserIds)
        {
            Dictionary<string, EntitlementObject> allEntitlments = new Dictionary<string, EntitlementObject>();
            DataTable dt = null;
            StoredProcedure spGet_AllUsersEntitlements = new ODBCWrapper.StoredProcedure("Get_AllUsersEntitlements");
            spGet_AllUsersEntitlements.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllUsersEntitlements.AddIDListParameter<int>("@UserIDs", lstUserIds, "Id");
            spGet_AllUsersEntitlements.AddParameter("@DomainID", domainID);
            dt = spGet_AllUsersEntitlements.Execute();

            if (dt != null && dt.Rows.Count > 0)
            {
                int ppvmTagLength = 6;
                foreach (DataRow dr in dt.Rows)
                {
                    int ppvCode = 0;
                    string customData = Utils.GetSafeStr(dr["CUSTOMDATA"]);
                    int mediaFileID = Utils.GetIntSafeVal(dr["MEDIA_FILE_ID"]);
                    int ppvTagStart = customData.IndexOf("<ppvm>") + ppvmTagLength;
                    int ppvTagEnd = customData.IndexOf("</ppvm>");
                    if (int.TryParse(customData.Substring(ppvTagStart, ppvTagEnd - ppvTagStart), out ppvCode))
                    {
                        string entitlementKey = mediaFileID + "_" + ppvCode;
                        if (!allEntitlments.ContainsKey(entitlementKey))
                        {
                            EntitlementObject entitlement = new EntitlementObject(Utils.GetIntSafeVal(dr["ID"]), Utils.GetSafeStr(dr["subscription_code"]), Utils.GetSafeStr(dr["rel_pp"]),
                                                    Utils.GetIntSafeVal(dr, "WAIVER"), Utils.GetSafeStr(dr["SITE_USER_GUID"]), mediaFileID,
                                                    ppvCode, Utils.GetDateSafeVal(dr, "CREATE_DATE"), Utils.ExtractNullableDateTime(dr, "START_DATE"), Utils.ExtractNullableDateTime(dr, "END_DATE"));
                            allEntitlments.Add(entitlementKey, entitlement);
                        }
                    }
                }
            }

            return allEntitlments;
        }

        public static Dictionary<string, Dictionary<string, int>> Get_AllMediaIdGroupFileTypesMappings(int[] mediaIDs)
        {
            Dictionary<string, Dictionary<string, int>> result = new Dictionary<string, Dictionary<string, int>>();
            DataTable dt = null;
            string resultKey = string.Empty;

            StoredProcedure spGet_AllMediaFilesMappings = new ODBCWrapper.StoredProcedure("Get_AllMediaFilesTypesMappings");
            spGet_AllMediaFilesMappings.AddIDListParameter<int>("@MediaIDs", mediaIDs.ToList(), "ID");
            spGet_AllMediaFilesMappings.SetConnectionKey("MAIN_CONNECTION_STRING");
            dt = spGet_AllMediaFilesMappings.Execute();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    int mediaFileID = Utils.GetIntSafeVal(dr["MEDIA_FILE_ID"]);
                    string mediaID = Utils.GetSafeStr(dr["MEDIA_ID"]);
                    string groupFileType = Utils.GetSafeStr(dr["GROUP_FILE_TYPE"]);
                    string key = mediaID + "_" + groupFileType;
                                       
                    resultKey = UtilsDal.MediaIdGroupFileTypesKey(Utils.GetIntSafeVal(dr["MEDIA_ID"]));

                    if (result.Keys.Contains(resultKey))
                    {
                        result[resultKey].Add(key, mediaFileID); 
                    }
                    else
                    {
                        result.Add(resultKey, new Dictionary<string, int>() { {key, mediaFileID}});
                    }

                    
                }
            }

            return result;
        }

        public static DataSet Get_AllBundlesInfoByUserIDsOrDomainID(int domainID, List<int> lstUsers, int nGroupID)
        {
            StoredProcedure spGet_AllBundlesInfoByUserIDsOrDomainID = new StoredProcedure("Get_AllBundlesInfoByUserIDsOrDomainID");
            spGet_AllBundlesInfoByUserIDsOrDomainID.SetConnectionKey("CA_CONNECTION_STRING");
            spGet_AllBundlesInfoByUserIDsOrDomainID.AddIDListParameter("@Users", lstUsers, "ID");
            spGet_AllBundlesInfoByUserIDsOrDomainID.AddParameter("@DomainID", domainID);
            spGet_AllBundlesInfoByUserIDsOrDomainID.AddParameter("@Group_id", nGroupID);

            return spGet_AllBundlesInfoByUserIDsOrDomainID.ExecuteDataSet();
        }

        public static DataTable Get_AllSubscriptionsPurchasesByUsersIDsOrDomainID(int domainID, List<int> lstUsers, int nGroupID)
        {
            StoredProcedure sp= new StoredProcedure("Get_AllSubscriptionsPurchasesByUsersIDsOrDomainID");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@usersList", lstUsers, "ID");
            sp.AddParameter("@DomainID", domainID);
            sp.AddParameter("@Group_id", nGroupID);

            return sp.Execute();
        }

        public static bool Delete_PPVPurchases(List<int> ppvIds)
        {
            StoredProcedure sp = new StoredProcedure("Delete_PPVPurchases");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@ppv_purchase_ids", ppvIds, "ID");
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool Update_PPVPurchaseDates(long ppvPurchaseId, DateTime startDate, DateTime endDate)
        {
            StoredProcedure sp = new StoredProcedure("Update_PPVPurchaseDates");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@ppv_purchase_id", ppvPurchaseId);
            sp.AddParameter("@start_date", startDate);
            sp.AddParameter("@end_date", endDate); 
 
            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool Update_SubscriptionPurchaseDates(long subscriptionPurchaseId, DateTime startDate, DateTime endDate)
        {
            StoredProcedure sp = new StoredProcedure("Update_SubscriptionPurchaseDates");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@subscription_purchase_id", subscriptionPurchaseId);
            sp.AddParameter("@start_date", startDate);
            sp.AddParameter("@end_date", endDate);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public static bool Delete_SubscriptionPurchases(List<int> subscriptionIds)
        {
            StoredProcedure sp = new StoredProcedure("Delete_SubscriptionPurchases");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddIDListParameter("@subscription_purchase_ids", subscriptionIds, "ID");
            return sp.ExecuteReturnValue<int>() > 0;
        }
        
        public static DataSet Get_RecurringSubscriptiosAndPendingPurchasesByPaymentMethod(int groupId, int domainId, int paymentMethodId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_RecurringSubscriptiosAndPendingPurchasesByPaymentMethod");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@DomainID", domainId);
            sp.AddParameter("@PaymentMethodID", paymentMethodId);

            return sp.ExecuteDataSet();            
        }

        public static Dictionary<long, List<int>> GetEpgsToFileIdsMap(int groupId, List<long> epgIds)
        {
            Dictionary<long, List<int>> epgToFileIdsMap = new Dictionary<long, List<int>>();
            ODBCWrapper.StoredProcedure spGet_Get_FilesByEpgIds = new ODBCWrapper.StoredProcedure("Get_FilesByEpgIds");
            spGet_Get_FilesByEpgIds.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGet_Get_FilesByEpgIds.AddParameter("@GroupId", groupId);
            if (epgIds.Count == 1)
            {
                spGet_Get_FilesByEpgIds.AddIDListParameter<long>("@EpgIds", new List<long>(), "ID");
                spGet_Get_FilesByEpgIds.AddParameter("@EpgId", epgIds.First());
            }
            else
            {
                spGet_Get_FilesByEpgIds.AddIDListParameter<long>("@EpgIds", epgIds, "ID");
            }

            DataTable dt = spGet_Get_FilesByEpgIds.Execute();

            if (dt != null && dt.Rows != null)
            {                
                foreach (DataRow dr in dt.Rows)
                {
                    int fileId = ODBCWrapper.Utils.GetIntSafeVal(dr, "media_file_id", 0);
                    long epgId = ODBCWrapper.Utils.GetIntSafeVal(dr, "epg_id", 0);
                    if (fileId > 0 && epgId > 0)
                    {
                        if (epgToFileIdsMap.ContainsKey(epgId))
                        {
                            epgToFileIdsMap[epgId].Add(fileId);
                        }
                        else
                        {
                            epgToFileIdsMap.Add(epgId, new List<int>() { fileId });
                        }
                    }
                }
            }

            return epgToFileIdsMap;
        }

        private static CDVRAdapter CreateCDVRAdapter(DataSet ds)
        {
            CDVRAdapter adapterResponse = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                adapterResponse = new CDVRAdapter();
                adapterResponse.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "adapter_url");
                adapterResponse.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "external_identifier");
                adapterResponse.ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                int is_Active = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_active");
                adapterResponse.IsActive = is_Active == 1 ? true : false;
                adapterResponse.Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "name");
                adapterResponse.SharedSecret = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "shared_secret");
                adapterResponse.DynamicLinksSupport = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "dynamic_links_support") == 1 ? true : false;

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                        string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                        if (adapterResponse.Settings == null)
                        {
                            adapterResponse.Settings = new List<CDVRAdapterSettings>();
                        }
                        adapterResponse.Settings.Add(new CDVRAdapterSettings(key, value));
                    }
                }
            }

            return adapterResponse;
        }

        private static DataTable CreateDataTable(List<CDVRAdapterSettings> list)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            resultTable.Columns.Add("idkey", typeof(string));
            resultTable.Columns.Add("value", typeof(string));

            foreach (CDVRAdapterSettings item in list)
            {
                DataRow row = resultTable.NewRow();
                row["idkey"] = item.key;
                row["value"] = item.value;
                resultTable.Rows.Add(row);
            }

            return resultTable;
        }

        public static CDVRAdapter InsertCDVRAdapter(int groupID, CDVRAdapter adapter)
        {
            CDVRAdapter adapterResponse = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_CDVRAdapter");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@name", adapter.Name);
                sp.AddParameter("@adapter_url", adapter.AdapterUrl);
                sp.AddParameter("@external_identifier", adapter.ExternalIdentifier);
                sp.AddParameter("@dynamic_links_support", adapter.DynamicLinksSupport);
                sp.AddParameter("@shared_secret", adapter.SharedSecret);
                sp.AddParameter("@isActive", adapter.IsActive);

                DataTable dt = CreateDataTable(adapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDVRAdapter(ds);
            }

            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        public static CDVRAdapter GetCDVRAdapterByExternalId(int groupID, string externalIdentifier)
        {
            CDVRAdapter adapterResponse = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDVRAdpterByExternalD");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@external_identifier", externalIdentifier);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDVRAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return adapterResponse;
        }

        public static CDVRAdapter GetCDVRAdapter(int groupID, int adapterId, int? isActive = null, int status = 1)
        {
            CDVRAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDVRAdapter");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@id", adapterId);
                sp.AddParameter("@status", status);
                if (isActive.HasValue)
                {
                    sp.AddParameter("@isActive", isActive.Value);
                }

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDVRAdapter(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return adapterResponse;
        }

        public static List<CDVRAdapter> GetCDVRAdapterList(int groupID, int status = 1, bool? isActive = null)
        {
            List<CDVRAdapter> res = new List<CDVRAdapter>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CDVRAdapters");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@status", status);
                if (isActive.HasValue)
                {
                    sp.AddParameter("@isActive", isActive.Value);
                }
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtResult = ds.Tables[0];
                    if (dtResult != null && dtResult.Rows != null && dtResult.Rows.Count > 0)
                    {
                        CDVRAdapter adapter = null;
                        foreach (DataRow dr in dtResult.Rows)
                        {
                            adapter = new CDVRAdapter()
                            {
                                ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id"),
                                Name = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                                AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url"),
                                ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier"),
                                IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active") == 0 ? false : true,
                                SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret"),
                                DynamicLinksSupport = ODBCWrapper.Utils.GetIntSafeVal(dr, "dynamic_links_support") == 1 ? true : false

                            };
                            res.Add(adapter);
                        }

                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            Dictionary<int, List<CDVRAdapterSettings>> settingsDict = new Dictionary<int, List<CDVRAdapterSettings>>();
                            foreach (DataRow dr in ds.Tables[1].Rows)
                            {
                                string key = ODBCWrapper.Utils.GetSafeStr(dr, "key");
                                string value = ODBCWrapper.Utils.GetSafeStr(dr, "value");
                                int adapterId = ODBCWrapper.Utils.GetIntSafeVal(dr, "adapter_id");
                                if (!settingsDict.ContainsKey(adapterId))
                                {
                                    settingsDict.Add(adapterId, new List<CDVRAdapterSettings>());
                                }
                                settingsDict[adapterId].Add(new CDVRAdapterSettings(key, value));
                            }

                            foreach (var adapterRes in res)
                            {
                                if (settingsDict.ContainsKey(adapterRes.ID))
                                {
                                    adapterRes.Settings = settingsDict[adapterRes.ID];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                res = new List<CDVRAdapter>();
            }
            return res;
        }

        public static CDVRAdapter SetCDVRAdapterSharedSecret(int groupID, int adapterId, string sharedSecret)
        {
            CDVRAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_CDVRAdapterSharedSecret");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@id", adapterId);
                sp.AddParameter("@sharedSecret", sharedSecret);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDVRAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        public static bool DeleteCDVRAdapter(int groupID, int adapterId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_CDVRAdapter");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@ID", adapterId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch 
            {
                return false;
            }
        }

        public static CDVRAdapter SetCDVRAdapter(int groupID, CDVRAdapter adapter)
        {
            CDVRAdapter adapterResponse = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_CDVRAdapter");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@ID", adapter.ID);
                sp.AddParameter("@name", adapter.Name);
                sp.AddParameter("@external_identifier", adapter.ExternalIdentifier);
                sp.AddParameter("@shared_secret", adapter.SharedSecret);
                sp.AddParameter("@dynamic_links_support", adapter.DynamicLinksSupport);
                sp.AddParameter("@adapter_url", adapter.AdapterUrl);
                sp.AddParameter("@isActive", adapter.IsActive);
                DataTable dt = CreateDataTable(adapter.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);
                sp.AddParameter("@keysValuesIsExists", adapter.Settings.Count);

                DataSet ds = sp.ExecuteDataSet();

                adapterResponse = CreateCDVRAdapter(ds);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return adapterResponse;
        }

        public static int GetDefaultQuotaInSeconds(int groupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_QuotaMinutes");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);

            return sp.ExecuteReturnValue<int>() * 60;
        }
        
        public static int GetTimeShiftedTVAdapterId(int groupId)
        {
            int adapterId = 0;

            object result =
                ODBCWrapper.Utils.GetTableSingleVal("time_shifted_tv_settings", "adapter_id", "group_id", "=", groupId, 1440, "MAIN_CONNECTION_STRING");

            if (result != DBNull.Value)
            {
                adapterId = Convert.ToInt32(result);
            }

            return adapterId;
        }

        private static QuotaManagementModel BuildQuotaManagementModelFromRow(DataRow row)
        {
            QuotaManagementModel model = new QuotaManagementModel();

            model.Id = ODBCWrapper.Utils.ExtractInteger(row, "ID");
            model.Minutes = ODBCWrapper.Utils.ExtractInteger(row, "MINUTES");

            return model;
        }

        public static QuotaManagementModel GetQuotaManagementModel(int groupId, int quotaManagementModelId)
        {
            QuotaManagementModel model = new QuotaManagementModel();

            if (quotaManagementModelId == 0)
            {
                // Get the default model of the gruop from the table time_shifted_tv_settings in DB TVinci. 
                //Also put it in cache for 10 minutes
                object defaultModelId =
                    ODBCWrapper.Utils.GetTableSingleVal("time_shifted_tv_settings", "quota_id", "group_id", "=", groupId, 600, "MAIN_CONNECTION_STRING");

                if (defaultModelId != null && defaultModelId != DBNull.Value)
                {
                    quotaManagementModelId = Convert.ToInt32(defaultModelId);
                }
            }

            // TODO: Verify table name
            DataRow row = ODBCWrapper.Utils.GetTableSingleRow("quota_modules", quotaManagementModelId, string.Empty, 30, true);

            if (row != null)
            {
                model = BuildQuotaManagementModelFromRow(row);
            }

            return model;
        }

        public static DataTable GetRenewSubscriptionsToRecover(int groupId, DateTime endDateStartRange, DateTime endDateEndRange)
        {
            DataTable dt = null;
            StoredProcedure spGetRenewSubscriptionsToRecover = new StoredProcedure("GetRenewSubscriptionsToRecover");
            spGetRenewSubscriptionsToRecover.SetConnectionKey("CA_CONNECTION_STRING");
            spGetRenewSubscriptionsToRecover.AddParameter("@GroupId", groupId);
            spGetRenewSubscriptionsToRecover.AddParameter("@EndDateStartRange", endDateStartRange);
            spGetRenewSubscriptionsToRecover.AddParameter("@EndDateEndRange", endDateEndRange);
            dt = spGetRenewSubscriptionsToRecover.Execute();

            return dt;
        }

        public static DataTable GetChannelByMediaFileId(int groupId, int mediaFileId)
        {
            DataTable dt = null;
            StoredProcedure spGetRenewSubscriptionsToRecover = new StoredProcedure("GetChannelByMediaFileId");
            spGetRenewSubscriptionsToRecover.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetRenewSubscriptionsToRecover.AddParameter("@GroupId", groupId);
            spGetRenewSubscriptionsToRecover.AddParameter("@MediaFileId", mediaFileId);
            dt = spGetRenewSubscriptionsToRecover.Execute();

            return dt;
        }

        #region Couchbase

        public static bool InsertOrSetCachedEntitlementResults(string version, long domainId, int mediaFileId, CachedEntitlementResults cachedEntitlementResults)
        {
            bool result = false;            
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.MEMCACHED);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string cachedEntitlementKey = UtilsDal.GetCachedEntitlementResultsKey(version, domainId, mediaFileId);
            if (string.IsNullOrEmpty(cachedEntitlementKey))
            {
                log.ErrorFormat("Failed getting cachedEntitlementKey for domainId: {0}, mediaFileId: {1}", domainId, mediaFileId);
            }
            else
            {
                try
                {
                    int numOfRetries = 0;
                    while (!result && numOfRetries < limitRetries)
                    {
                        ulong docVersion;
                        Couchbase.IO.ResponseStatus status;
                        CachedEntitlementResults currentCachedEntitlementResults = cbClient.GetWithVersion<CachedEntitlementResults>(cachedEntitlementKey, out docVersion, out status);
                        if (status == Couchbase.IO.ResponseStatus.Success || status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            result = cbClient.SetWithVersion<CachedEntitlementResults>(cachedEntitlementKey, cachedEntitlementResults, docVersion, CACHED_ENTITLEMENT_RESULTS_TTL_SEC);
                        }

                        if (!result)
                        {
                            numOfRetries++;
                            log.ErrorFormat("Error while updating CachedEntitlementResults. domainId: {0}, mediaFileId: {1}", domainId, mediaFileId, cachedEntitlementResults);
                            System.Threading.Thread.Sleep(r.Next(50));
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Error on InsertOrSetCachedEntitlementResults, domainId: {0}, mediaFileId: {1}, cachedEntitlementResults: {2}",
                                             domainId, mediaFileId, cachedEntitlementResults.ToString()), ex);
                }
            }

            return result;
        }

        public static CachedEntitlementResults GetCachedEntitlementResults(string version, long domainId, int mediaFileId)
        {
            CachedEntitlementResults response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.MEMCACHED);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string cachedEntitlementKey = UtilsDal.GetCachedEntitlementResultsKey(version, domainId, mediaFileId);
            if (string.IsNullOrEmpty(cachedEntitlementKey))
            {
                log.ErrorFormat("Failed getting domainQuotaKey for domainId: {0}", domainId);
                return response;
            }

            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<CachedEntitlementResults>(cachedEntitlementKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        log.ErrorFormat("Error while trying to get CachedEntitlementResults, KeyNotFound. key: {1}", cachedEntitlementKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {                        
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving CachedEntitlementResults with key {0} failed with status: {1}, retryAttempt: {2}, maxRetries: {3}", cachedEntitlementKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on GetCachedEntitlementResults, domainId: {0}, mediaFileId: {1}", domainId, mediaFileId), ex); ;
            }

            return response;
        }

        #endregion

        public static int DeleteHouseholdSubscriptions(int householdId, int subscriptionPurchaseStatus)
        {
            int result;
            StoredProcedure sp = new StoredProcedure("DeleteHouseholdSubscriptions");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@householdId", householdId);
            sp.AddParameter("@SubscriptionPurchaseStatus", subscriptionPurchaseStatus);
            if (sp.ExecuteReturnValue<int>(out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

        public static int DeleteHouseholdPPVs(int householdId)
        {
            int result;
            StoredProcedure sp = new StoredProcedure("DeleteHouseholdPPVs");
            sp.SetConnectionKey("CA_CONNECTION_STRING");
            sp.AddParameter("@householdId", householdId);
            if (sp.ExecuteReturnValue<int>(out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }


        public static DataRow GetPurchaseByID(int purchaseID)
        {            
            try
            {
                StoredProcedure sp = new StoredProcedure("Get_SubscriptionsByPurchaseID");
                sp.SetConnectionKey("CA_CONNECTION_STRING");
                sp.AddParameter("@ID", purchaseID);
                DataTable dt = sp.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
               // return ODBCWrapper.Utils.GetTableSingleRowByValue("subscriptions_purchases", "ID", purchaseID, true, "CA_CONNECTION_STRING");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail in GetPurchaseByID (DAL) purchaseID={0} , ex={1}", purchaseID, ex.Message);
                return null;
            }
            return null;
        }
    }
}