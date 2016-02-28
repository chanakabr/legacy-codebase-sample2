using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ODBCWrapper;

namespace DAL
{
    public class ConditionalAccessDAL
    {
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

        public static bool InsertPPVPurchase(int nGroupID, string sSubCode, int nMediaFileID, string sSiteGUID, double dPrice, string sCurrency, int nNumOfUses, string sCustomData, int transactionID, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int maxNumOfUses, int nIsActive, int nStatus, DateTime? dtEndDate)
        {
            bool res = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("ppv_purchases");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);

                if (sSubCode != null)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCode);
                }

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMediaFileID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
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
            //throw new NotImplementedException();
        }

        public static int GetPPVPurchaseID(int m_nGroupID, string subCode, int nMediaFileID, string sSiteGUID, double dPrice, string sCurrency, int nNumOfUses, int nMaxNumOfUses, int nIsActive, int nStatus)
        {
            int purchaseID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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


        public static DataView GetUserBillingHistory(string[] arrGroupIDs, string sUserGUID, int nTopNum, DateTime dStartDate, DateTime dEndDate)
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
                selectQuery += " ORDER BY ID DESC";

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

        public static bool UpdateSubPurchase(int nGroupID, string sSiteGUID, string sSubscriptionCode, int nIsRecurring, int nSubscriptionPurchaseID = 0)
        {
            bool updated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
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

        public static bool InsertSubPurchase(int m_nGroupID, string sSubscriptionCode, string sSiteGUID, double dPrice, string sCurrency, string sCustomData, int numOfUses, string sCountryCd,
            string sLANGUAGE_CODE, string sDEVICE_NAME, int nMaxNumberOfViews, int nViewLifeCycleSec, int nIsRecurringStatus, int nReceiptCode, int nIsActive, int nStatus, DateTime? dtEndDate)
        {

            bool inserted = false;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("subscriptions_purchases");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", numOfUses);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfViews);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", nViewLifeCycleSec);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", nIsRecurringStatus);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_TRANSACTION_ID", "=", nReceiptCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);

                if (dtEndDate.HasValue)
                {
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("END_DATE", "=", dtEndDate.Value);
                }

                inserted = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return inserted;
        }

        public static int GetSubPurchaseID(int m_nGroupID, string sSubscriptionCode, string sSiteGUID, double dPrice, string sCurrency, int numOfUses, int nMaxNumberOfViews, int nViewLifeCycleSec,
                                        int nIsRecurringStatus, int nIsActive, int nStatus)
        {
            int nPurchaseID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " SELECT ID FROM SUBSCRIPTIONS_PURCHASES WITH (NOLOCK) WHERE ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubscriptionCode);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CD", "=", sCurrency);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUM_OF_USES", "=", numOfUses);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MAX_NUM_OF_USES", "=", nMaxNumberOfViews);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("VIEW_LIFE_CYCLE_SECS", "=", nViewLifeCycleSec);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING_STATUS", "=", nIsRecurringStatus);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", nIsActive);
                selectQuery += " AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", nStatus);
                selectQuery += " ORDER BY ID DESC";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nPurchaseID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nPurchaseID;
        }

        public static bool CancelSubPurchase(int m_nGroupID, string sSubscriptionCode, int nPurchaseID, string sSiteGUID, int nIsActive = 2, int nStatus = 2)
        {
            bool updated = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                //updateQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
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

        public static DataTable Get_All_Users_PPV_modules(List<int> usersIds, bool isExpired)
        {
            ODBCWrapper.StoredProcedure spGet_All_Users_PPV_modules = new ODBCWrapper.StoredProcedure("Get_UsersPermittedItems");
            spGet_All_Users_PPV_modules.SetConnectionKey("CONNECTION_STRING");
            spGet_All_Users_PPV_modules.AddIDListParameter<int>("@UserIDs", usersIds, "Id");
            spGet_All_Users_PPV_modules.AddParameter("@isExpired", isExpired);

            DataSet ds = spGet_All_Users_PPV_modules.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UsersPermittedSubscriptions(List<int> usersIds, bool isExpired)
        {
            ODBCWrapper.StoredProcedure spGet_All_Users_Permitted_Subscriptions = new ODBCWrapper.StoredProcedure("Get_UsersPermittedSubscriptions");
            spGet_All_Users_Permitted_Subscriptions.SetConnectionKey("CONNECTION_STRING");
            spGet_All_Users_Permitted_Subscriptions.AddIDListParameter<int>("@UserIDs", usersIds, "Id");
            spGet_All_Users_Permitted_Subscriptions.AddParameter("@isExpired", isExpired);

            DataSet ds = spGet_All_Users_Permitted_Subscriptions.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UsersPermittedCollections(List<int> usersIds, bool isExpired)
        {
            ODBCWrapper.StoredProcedure spGet_UsersPermittedCollections = new ODBCWrapper.StoredProcedure("Get_UsersPermittedCollections");
            spGet_UsersPermittedCollections.SetConnectionKey("CONNECTION_STRING");
            spGet_UsersPermittedCollections.AddIDListParameter<int>("@UserIDs", usersIds, "Id");
            spGet_UsersPermittedCollections.AddParameter("@isExpired", isExpired);

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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@IDInAdyenTransactions", lAdyenTransactionsID);
            sp.AddParameter("@IsActive", bIsActive ? 1 : 0);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@PurchaseType", nPurchaseType);
            return sp.ExecuteReturnValue<bool>();

        }

        public static DataTable Get_allDomainsPPVUsesUsingCollection(List<int> usersList, int groupID, int MediaFileID, int nBoxsetID)
        {
            ODBCWrapper.StoredProcedure spGet_allDomainsPPVUsesUsingCollection = new ODBCWrapper.StoredProcedure("Get_allDomainsPPVUsesUsingCollection");
            spGet_allDomainsPPVUsesUsingCollection.SetConnectionKey("CONNECTION_STRING");
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
            spGet_AllUsersPurchases.SetConnectionKey("CONNECTION_STRING");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@FileIDs", FileIds, "Id");
            spGet_AllUsersPurchases.AddParameter("@nMediaFileID", fileID);

            DataSet ds = spGet_AllUsersPurchases.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static bool Get_AllUsersPurchases(List<int> p_lstUserIDs, List<int> p_lstFileIds, int p_nFileID, string p_sPPVCode, ref int p_nPPVID, ref string p_sSubCode,
            ref string p_sPPCode, ref int p_nWaiver, ref DateTime p_dCreateDate, ref string p_sPurchasedBySiteGuid, ref int p_nPurchasedAsMediaFileID, ref DateTime? p_dtStartDate)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure spGet_AllUsersPurchases = new ODBCWrapper.StoredProcedure("Get_AllUsersPurchases");
            spGet_AllUsersPurchases.SetConnectionKey("CONNECTION_STRING");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@UserIDs", p_lstUserIDs, "Id");
            spGet_AllUsersPurchases.AddIDListParameter<int>("@FileIDs", p_lstFileIds, "Id");
            spGet_AllUsersPurchases.AddParameter("@nMediaFileID", p_nFileID);

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
                            DataRow drSource = dt.Rows[0];

                            p_nPPVID = ODBCWrapper.Utils.GetIntSafeVal(drSource["ID"]);
                            p_sSubCode = ODBCWrapper.Utils.GetSafeStr(drSource["subscription_code"]);
                            p_sPPCode = ODBCWrapper.Utils.GetSafeStr(drSource["rel_pp"]);
                            p_sPurchasedBySiteGuid = ODBCWrapper.Utils.GetSafeStr(drSource["SITE_USER_GUID"]);
                            p_nPurchasedAsMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(drSource["MEDIA_FILE_ID"]);
                            //cancellation window 
                            p_nWaiver = ODBCWrapper.Utils.GetIntSafeVal(drSource, "WAIVER");
                            p_dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(drSource, "CREATE_DATE");
                            p_dtStartDate = ODBCWrapper.Utils.ExtractNullableDateTime(drSource, "START_DATE");
                        }
                    }
                }
            }

            return res;
        }



        public static DataTable Get_AllSubscriptionInfoByUsersIDs(List<int> UserIDs, List<int> nFileTypes)
        {
            ODBCWrapper.StoredProcedure spGet_AllSubscriptionInfoByUsersIDs = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionInfoByUsersIDs");
            spGet_AllSubscriptionInfoByUsersIDs.SetConnectionKey("CONNECTION_STRING");
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
            spGet_AllCollectionInfoByUsersIDs.SetConnectionKey("CONNECTION_STRING");
            spGet_AllCollectionInfoByUsersIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");


            DataSet ds = spGet_AllCollectionInfoByUsersIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllSubscriptionInfoByUsersIDs(List<int> UserIDs)
        {
            ODBCWrapper.StoredProcedure spGet_AllSubscriptionInfoByUsersIDs = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionInfoByUsersIDs");
            spGet_AllSubscriptionInfoByUsersIDs.SetConnectionKey("CONNECTION_STRING");
            spGet_AllSubscriptionInfoByUsersIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");

            DataSet ds = spGet_AllSubscriptionInfoByUsersIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_SubscriptionBySubscriptionCodeAndUserIDs(List<int> UserIDs, string subscriptionCode)
        {
            ODBCWrapper.StoredProcedure spGet_SubscriptionBySubscriptionCodeAndUserIDs = new ODBCWrapper.StoredProcedure("Get_SubscriptionBySubscriptionCodeAndUserIDs");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.SetConnectionKey("CONNECTION_STRING");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_SubscriptionBySubscriptionCodeAndUserIDs.AddParameter("@subscriptionCode", subscriptionCode);

            DataSet ds = spGet_SubscriptionBySubscriptionCodeAndUserIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_CollectionByCollectionCodeAndUserIDs(List<int> UserIDs, string collectionCode)
        {
            ODBCWrapper.StoredProcedure spGet_CollectionByCollectionCodeAndUserIDs = new ODBCWrapper.StoredProcedure("Get_CollectionByCollectionCodeAndUserIDs");
            spGet_CollectionByCollectionCodeAndUserIDs.SetConnectionKey("CONNECTION_STRING");
            spGet_CollectionByCollectionCodeAndUserIDs.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_CollectionByCollectionCodeAndUserIDs.AddParameter("@collectionCode", collectionCode);

            DataSet ds = spGet_CollectionByCollectionCodeAndUserIDs.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllPPVPurchasesByUserIDsAndMediaFileID(int nMediaFileID, List<int> UserIDs, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllPPVPurchasesByUserIDsAndMediaFileID");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@nMediaFileID", nMediaFileID);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_SubUsesByUserListFileIDAndSubCode(List<int> UserIDs, string subCode, int nMediaFileID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGet_PPVUsesByUserListFileIDAndSubCode = new ODBCWrapper.StoredProcedure("Get_SubUsesByUserListFileIDAndSubCode");
            spGet_PPVUsesByUserListFileIDAndSubCode.SetConnectionKey("CONNECTION_STRING");
            spGet_PPVUsesByUserListFileIDAndSubCode.AddIDListParameter<int>("@usersList", UserIDs, "Id");
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@subscriptionCode", subCode);
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@mediaFileID", nMediaFileID);
            spGet_PPVUsesByUserListFileIDAndSubCode.AddParameter("@groupID", nGroupID);


            DataSet ds = spGet_PPVUsesByUserListFileIDAndSubCode.ExecuteDataSet();

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_PreviewModuleDataForEntitlementCalc(int nGroupID, string sSiteGuid, string sSubCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PreviewModuleDataForEntitlementCalc");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@SubscriptionCode", sSubCode);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static long Insert_NewPPVPurchase(long lGroupID, long lMediaFileID, string sSiteGuid, double dPrice,
       string sCurrencyCode, long lMaxNumOfUses, string sCustomData, string sSubscriptionCode,
       long lBillingTransactionID, DateTime dtStartDate, DateTime dtEndDate, DateTime dtCreateAndUpdateDate,
       string sCountryCode, string sLanguageCode, string sDeviceName, string sConnKey, int domainID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewPPVPurchase");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@MediaFileID", lMediaFileID);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@Price", dPrice);
            sp.AddParameter("@CurrencyCode", sCurrencyCode);
            sp.AddParameter("@NumOfUses", 0);
            sp.AddParameter("@MaxNumOfUses", lMaxNumOfUses);
            sp.AddParameter("@CustomData", sCustomData);
            sp.AddParameter("@LastViewDate", DBNull.Value);
            if (!string.IsNullOrEmpty(sSubscriptionCode))
                sp.AddParameter("@SubscriptionCode", sSubscriptionCode);
            else
                sp.AddParameter("@SubscriptionCode", DBNull.Value);
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);
            sp.AddParameter("@StartDate", dtStartDate);
            sp.AddParameter("@IsActive", 1);
            sp.AddParameter("@Status", 1);
            sp.AddParameter("@EndDate", dtEndDate);
            sp.AddParameter("@UpdaterID", 0);
            sp.AddParameter("@UpdateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@CreateDate", dtCreateAndUpdateDate);
            sp.AddParameter("@PublishDate", DBNull.Value);
            sp.AddParameter("@CountryCode", sCountryCode);
            sp.AddParameter("@LanguageCode", sLanguageCode);
            sp.AddParameter("@DeviceName", sDeviceName);
            sp.AddParameter("@RelPp", DBNull.Value);
            sp.AddParameter("@domainID", domainID);

            return sp.ExecuteReturnValue<long>();
        }

        public static long Insert_NewMPPPurchase(long lGroupID, string sSubscriptionCode, string sSiteGuid,
            double dPrice, string sCurrencyCode, string sCustomData, string sCountryCode, string sLanguageCode,
            string sDeviceName, long lMaxNumOfUses, long lViewLifeCycleSecs, bool bIsRecurringStatus,
            long lBillingTransactionID, long lPreviewModuleID, DateTime dtSubscriptionStartDate, DateTime dtSubscriptionEndDate,
            DateTime dtCreateAndUpdateDate, string sConnKey, int domainID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewMPPPurchase");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
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
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
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
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@GroupID", lGroupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static int Get_GroupFailCount(long lGroupID)
        {
            return Get_GroupFailCount(lGroupID, string.Empty);
        }

        public static void Update_MPPRenewalData(long lPurchaseID, bool bIsRecurringStatus, DateTime dtNewEndDate, long lNumOfUses, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPRenewalData");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@IsRecurringStatus", bIsRecurringStatus ? 1 : 0);
            sp.AddParameter("@EndDate", dtNewEndDate);
            sp.AddParameter("@NumOfUses", lNumOfUses);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            sp.ExecuteNonQuery();
        }

        public static void Update_MPPFailCountByPurchaseID(long lPurchaseID, bool bTrueForIncrementingByOneFalseForSettingNewValue, int nNewValue, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPFailCountByPurchaseID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@nSubPurchaseID", nSubscriptionsPurchasesID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static long CancelSubscription(int nSubscriptionsPurchasesID, int nGroupID, string sSiteGUID, string nSubscriptionCode)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelSubscription");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@nID", nSubscriptionsPurchasesID);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@SiteGUID", sSiteGUID);
            sp.AddParameter("@SubscriptionCode", nSubscriptionCode);
            

            return sp.ExecuteReturnValue<long>();
        }

        public static void Update_MPPIsRecurringStatus(bool bIsSwitchOnRecurringStatus, long lGroupID, string sSubCode,
            string sSiteGuid, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_MPPIsRecurringStatus");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            DateTime dtCreateAndUpdateDate, string sConnKey, int domainID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewColPurchase");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
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


        public static bool CancelPPVPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelPPVPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid",sSiteGuid );
            sp.AddParameter("@AssetID", nAssetID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool CancelSubscriptionPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelSubscriptionPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool CancelCollectionPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("CancelCollectionPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@CancellationDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverPPVPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverPPVPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverSubscriptionPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverSubscriptionPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);

            return sp.ExecuteReturnValue<bool>();
        }

        public static bool WaiverCollectionPurchaseTransaction(string sSiteGuid, int nAssetID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("WaiverCollectionPurchaseTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@WaiverDate", DateTime.UtcNow);
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@AssetID", nAssetID);

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
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@SubCode", sSubCode);
            sp.AddParameter("@GroupID", nGroupID);

            return sp.ExecuteReturnValue<int>();
        }

        public static int Get_PPVPurchaseCount(int nGroupID, string sSiteGuid, string sSubCode, long lMediaFileID)
        {
            StoredProcedure sp = new StoredProcedure("Get_PPVPurchaseCount");
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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

        public static DataSet Get_AllBundlesInfoByUserIDs(List<int> lstUsers, List<int> lstFileTypes)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllBundlesInfoByUserIDs");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddIDListParameter("@Users", lstUsers, "ID");
            sp.AddIDListParameter("@FileTypes", lstFileTypes, "ID");

            return sp.ExecuteDataSet();
        }

        public static bool Get_LatestCreateDateOfBundlesUses(List<string> subscriptionsIDs, List<string> collectionsIDs,
            List<string> domainUserIDs, List<int> relatedMediaFileIDs, int nGroupID, ref Dictionary<string, DateTime> subsToCreateDateMapping,
            ref Dictionary<string, DateTime> colsToCreateDateMapping, ref DateTime dateNowDBTime)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_LatestCreateDateOfBundlesUses");
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
     


        public static DataTable Get_AllSubscriptionPurchasesByUserIDsAndSubscriptionCode(int nSubscriptionCode, List<int> UserIDs, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllSubscriptionPurchasesByUserIDsAndSubscriptionCode");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@SubscriptionCode", nSubscriptionCode);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllCollectionPurchasesByUserIDsAndCollectionCode(int nCollectionCode, List<int> UserIDs, int nGroupID)
        {
            ODBCWrapper.StoredProcedure spGet_AllPPVPurchasesByUserIDsAndMediaFileID = new ODBCWrapper.StoredProcedure("Get_AllCollectionPurchasesByUserIDsAndCollectionCode");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.SetConnectionKey("CONNECTION_STRING");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@CollectionCode", nCollectionCode);
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddIDListParameter<int>("@UserIDs", UserIDs, "Id");
            spGet_AllPPVPurchasesByUserIDsAndMediaFileID.AddParameter("@groupID", nGroupID);


            DataSet ds = spGet_AllPPVPurchasesByUserIDsAndMediaFileID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }
        

        public static string Get_LicensedLinkSecretCode(long groupID)
        {
            StoredProcedure sp = new StoredProcedure("Get_LicensedLinkSecretCode");
            sp.SetConnectionKey("CONNECTION_STRING");
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

        public static bool Get_BasicLinkData(long mediaFileID, ref string baseUrl, ref string streamingCode, ref int streamingCompanyID) 
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
                }
            }

            return res;
        }

        public static bool Get_IsLastViewData(long ppvPurchaseID, ref int numOfUses, ref int maxNumOfUses, ref DateTime endDate)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_IsLastViewData");
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            List<int> userIDs)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllPPVPurchasesByUserIDsAndMediaFileIDs");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddIDListParameter<int>("@RelatedMediaFileIDs", relatedMediaFileIDs, "Id");
            sp.AddIDListParameter<int>("@UserIDs", userIDs, "Id");
            sp.AddParameter("@GroupID", groupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AllDomainPPVUsesByMediaFiles(long groupID, List<int> usersInDomain, List<int> relatedMediaFileIDs, string sPPVMCd)
        {
            StoredProcedure sp = new StoredProcedure("Get_AllDomainPPVUsesByMediaFiles");
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupID);
            sp.AddParameter("@SiteGuid", siteGuid);
            sp.AddParameter("@SubCode", subCode);

            return sp.ExecuteReturnValue<bool>();
        }

        public static long Insert_NewCollectionUse(long groupID, string collCode, long mediaFileID, string siteGuid, bool isCreditDownloaded,
            string countryCode, string langCode, string deviceName)
        {
            StoredProcedure sp = new StoredProcedure("Insert_NewCollectionUse");
            sp.SetConnectionKey("CONNECTION_STRING");
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
            sp.SetConnectionKey("CONNECTION_STRING");
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
        public static DataTable Get_UsersSubscriptionPurchases(List<int> p_lstUsers, string p_sSubscriptionCode)
        {
            DataTable dtUserPurchases = null;
            StoredProcedure spStoredProcedure = new StoredProcedure("Get_UsersSubscriptionPurchases");
            spStoredProcedure.SetConnectionKey("CONNECTION_STRING");
            spStoredProcedure.AddIDListParameter<int>("@UserIDs", p_lstUsers, "Id");
            spStoredProcedure.AddParameter("@SubscriptionCode", p_sSubscriptionCode);

            DataSet dsStoredProcedureResult = spStoredProcedure.ExecuteDataSet();

            // If stored procedure was succesful, get the first one
            if (dsStoredProcedureResult != null && dsStoredProcedureResult.Tables.Count == 1)
            {
                dtUserPurchases = dsStoredProcedureResult.Tables[0];
            }

            return (dtUserPurchases);
        }

        public static long Get_LastDomainDLM(int groupID, int domainID)
        {
            long dlmID = 0;

            StoredProcedure sp = new StoredProcedure("Get_LastDomainDLM");
            sp.SetConnectionKey("CONNECTION_STRING");
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
    }
}
