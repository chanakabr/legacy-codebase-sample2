using ApiObjects.Billing;
using KLogMonitor;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DAL
{
    public class BillingDAL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string BILLING_CONNECTION_STRING = "CONNECTION_STRING";
        private const string SP_IS_DOUBLE_ADYEN_TRANSACTION = "IsDoubleAdyenTransaction";

        public static DataTable Get_UserToken(int nGroupID, string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spUserToken = new ODBCWrapper.StoredProcedure("Get_UserToken");
            spUserToken.SetConnectionKey("CONNECTION_STRING");
            spUserToken.AddParameter("@GroupID", nGroupID);
            spUserToken.AddParameter("@SiteGuid", sSiteGuid);

            DataSet ds = spUserToken.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable IsUserMultiCC(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spUserToken = new ODBCWrapper.StoredProcedure("IsUserMultiCC");
            spUserToken.SetConnectionKey("CONNECTION_STRING");
            spUserToken.AddParameter("@GroupID", nGroupID);

            DataSet ds = spUserToken.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_UserDigits(int nGroupID, string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure spUserToken = new ODBCWrapper.StoredProcedure("Get_UserDigits");
            spUserToken.SetConnectionKey("CONNECTION_STRING");
            spUserToken.AddParameter("@GroupID", nGroupID);
            spUserToken.AddParameter("@SiteGuid", sSiteGuid);

            DataSet ds = spUserToken.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool IsDoubleAdyenTransaction(int nGroupID, string sSiteGuid, string sPspReference, string sStatus)
        {
            ODBCWrapper.StoredProcedure spIsDoubleAdyenTransaction = new ODBCWrapper.StoredProcedure(SP_IS_DOUBLE_ADYEN_TRANSACTION);
            spIsDoubleAdyenTransaction.SetConnectionKey("CONNECTION_STRING");
            spIsDoubleAdyenTransaction.AddParameter("@GroupID", nGroupID);
            spIsDoubleAdyenTransaction.AddParameter("@SiteGuid", sSiteGuid.Trim());
            spIsDoubleAdyenTransaction.AddParameter("@PspReference", sPspReference.Trim());
            spIsDoubleAdyenTransaction.AddParameter("@AdyenStatus", sStatus.Trim().ToUpper());
            return spIsDoubleAdyenTransaction.ExecuteReturnValue<bool>();

        }

        public static int InsertBillingTransactionDB(string sSITE_GUID, string sLAST_FOUR_DIGITS, double dPRICE, string sPRICE_CODE, string sCURRENCY_CODE, string sCUSTOMDATA, int nBILLING_STATUS, string sBILLING_REASON, bool bIS_RECURRING, int nMEDIA_FILE_ID, int nMEDIA_ID, string sPPVMODULE_CODE, string sSUBSCRIPTION_CODE, string sCELL_PHONE, int ngroup_id, int nBILLING_PROVIDER, int nBILLING_PROVIDER_REFFERENCE, double dPAYMENT_METHOD_ADDITION, double dTOTAL_PRICE, int nPAYMENT_NUMBER, int nNUMBER_OF_PAYMENTS, string sEXTRA_PARAMS, string sCountryCd, string sLanguageCode, string sDeviceName, int nBILLING_PROCESSOR, int nBILLING_METHOD, string sPrePaidCode, long lPreviewModuleID)
        {
            int ret = 0;

            try
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("billing_transactions");
                insertQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSITE_GUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sLAST_FOUR_DIGITS);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPRICE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE", "=", sPRICE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCURRENCY_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCUSTOMDATA);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_STATUS", "=", nBILLING_STATUS);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_REASON", "=", sBILLING_REASON);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCELL_PHONE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_NUMBER", "=", nPAYMENT_NUMBER);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("EXTRA_PARAMS", "=", sEXTRA_PARAMS);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("NUMBER_OF_PAYMENTS", "=", nNUMBER_OF_PAYMENTS);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROCESSOR", "=", nBILLING_PROCESSOR);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_METHOD", "=", nBILLING_METHOD);

                Int32 nIS_RECURRING = (bIS_RECURRING == true) ? 1 : 0;
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING", "=", nIS_RECURRING);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMEDIA_FILE_ID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_ID", "=", nMEDIA_ID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVMODULE_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSUBSCRIPTION_CODE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", ngroup_id);
                DateTime dtToWriteToDB = DateTime.UtcNow;
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREATE_DATE", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", nBILLING_PROVIDER);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER_REFFERENCE", "=", nBILLING_PROVIDER_REFFERENCE);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_METHOD_ADDITION", "=", dPAYMENT_METHOD_ADDITION);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TOTAL_PRICE", "=", dTOTAL_PRICE);

                if (String.IsNullOrEmpty(sCountryCd) == false)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                if (String.IsNullOrEmpty(sLanguageCode) == false)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLanguageCode);
                if (String.IsNullOrEmpty(sDeviceName) == false)
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDeviceName);

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("pre_paid_code", "=", sPrePaidCode);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Preview_Module_ID", "=", lPreviewModuleID);
                bool insertRes = insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;

                if (!insertRes)
                {
                    return ret;
                }


                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select id from billing_transactions where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSITE_GUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sLAST_FOUR_DIGITS);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPRICE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE", "=", sPRICE_CODE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCURRENCY_CODE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCUSTOMDATA);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_STATUS", "=", nBILLING_STATUS);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_REASON", "=", sBILLING_REASON);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_RECURRING", "=", nIS_RECURRING);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MEDIA_FILE_ID", "=", nMEDIA_FILE_ID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PPVMODULE_CODE", "=", sPPVMODULE_CODE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSUBSCRIPTION_CODE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCELL_PHONE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", ngroup_id);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER", "=", nBILLING_PROVIDER);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER_REFFERENCE", "=", nBILLING_PROVIDER_REFFERENCE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_METHOD_ADDITION", "=", dPAYMENT_METHOD_ADDITION);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TOTAL_PRICE", "=", dTOTAL_PRICE);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_NUMBER", "=", nPAYMENT_NUMBER);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("NUMBER_OF_PAYMENTS", "=", nNUMBER_OF_PAYMENTS);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Preview_Module_ID", "=", lPreviewModuleID);
                if (String.IsNullOrEmpty(sCountryCd) == false)
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                }
                if (String.IsNullOrEmpty(sLanguageCode) == false)
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLanguageCode);
                }
                if (String.IsNullOrEmpty(sDeviceName) == false)
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDeviceName);
                }
                selectQuery += "order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                return ret;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }

        public static int GetModuleImplementationID(int nGroupID, int nModuleID)
        {
            int nImplID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from groups_modules_implementations with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", nModuleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return nImplID;
        }

        public static DataTable Get_PurchaseMailData(string sPSPReference)
        {
            ODBCWrapper.StoredProcedure spGetPurchaseMailData = new ODBCWrapper.StoredProcedure("Get_PurchaseMailData");
            spGetPurchaseMailData.SetConnectionKey("CONNECTION_STRING");
            spGetPurchaseMailData.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = spGetPurchaseMailData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool Get_PurchaseMailData(string sPSPReference, ref long billingID, ref int groupID,
            ref string currencyCode, ref string siteGuid, ref double realPrice, ref double totalPrice, ref int billingMethod,
            ref string last4Digits, ref string customData, ref string ppvModuleCode, ref string subCode, ref string ppCode)
        {
            //ODBCWrapper.StoredProcedure spGetPurchaseMailData = new ODBCWrapper.StoredProcedure("Get_PurchaseMailData");
            //spGetPurchaseMailData.SetConnectionKey("CONNECTION_STRING");
            //spGetPurchaseMailData.AddParameter("@PSPReference", sPSPReference);
            //DataSet ds = spGetPurchaseMailData.ExecuteDataSet();

            //if (ds != null)
            //    return ds.Tables[0];
            //return null;
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_PurchaseMailData");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = true;
                    billingID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0]["billing_id"]);
                    groupID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["group_id"]);
                    currencyCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["currency_code"]);
                    siteGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["site_guid"]);
                    realPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dt.Rows[0]["real_price"]);
                    totalPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dt.Rows[0]["total_price"]);
                    billingMethod = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["billing_method"]);
                    last4Digits = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["last_four_digits"]);
                    customData = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["customdata"]);
                    ppvModuleCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["ppvmodule_code"]);
                    subCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["subscription_code"]);
                    ppCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["pre_paid_code"]);
                }
            }

            return res;

        }

        public static DataTable Get_M1_PurchaseMailData(int nM1TransactionID)
        {
            ODBCWrapper.StoredProcedure spGetPurchaseMailData = new ODBCWrapper.StoredProcedure("Get_M1_PurchaseMailData");
            spGetPurchaseMailData.SetConnectionKey("CONNECTION_STRING");
            spGetPurchaseMailData.AddParameter("@M1TransactionID", nM1TransactionID);
            DataSet ds = spGetPurchaseMailData.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }


        public static void Insert_AdyenNotification(string sPSPReference, string sEventCode, string sSuccess, string sLast4Digits, string sReason)
        {
            ODBCWrapper.StoredProcedure spInsertAdyenNotification = new ODBCWrapper.StoredProcedure("Insert_AdyenNotification");
            spInsertAdyenNotification.SetConnectionKey("CONNECTION_STRING");
            spInsertAdyenNotification.AddParameter("@PSPReference", sPSPReference);
            spInsertAdyenNotification.AddParameter("@EventCode", sEventCode);
            spInsertAdyenNotification.AddParameter("@AdyenSuccess", sSuccess);
            spInsertAdyenNotification.AddParameter("@Last4Digits", sLast4Digits != null && sLast4Digits.Length < 5 ? sLast4Digits : sLast4Digits.Substring(0, 4));
            spInsertAdyenNotification.AddParameter("@Reason", sReason);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            spInsertAdyenNotification.AddParameter("@CreateDate", dtToWriteToDB);
            spInsertAdyenNotification.AddParameter("@UpdateDate", dtToWriteToDB);
            spInsertAdyenNotification.ExecuteNonQuery();
        }

        public static void Update_AdyenNotification(string sPSPReference, bool bMarkToDelete)
        {
            ODBCWrapper.StoredProcedure spUpdateAdyenNotification = new ODBCWrapper.StoredProcedure("Update_AdyenNotification");
            spUpdateAdyenNotification.SetConnectionKey("CONNECTION_STRING");
            spUpdateAdyenNotification.AddParameter("@PSPReference", sPSPReference);
            spUpdateAdyenNotification.AddParameter("@MarkToDelete", bMarkToDelete ? 1 : 0);
            spUpdateAdyenNotification.AddParameter("@UpdateDate", DateTime.UtcNow);
            spUpdateAdyenNotification.ExecuteNonQuery();
        }


        public static bool Get_DataForAdyenNotification(string sPSPReference, ref long lIDInAdyenTransactions,
            ref long lIDInBillingTransactions, ref bool bIsCreatedByAdyenCallback, ref int nPurchaseType,
            ref long lIDInRelevantCATable, ref bool bIsPurchasedWithPreviewModule)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure spGetDataForAdyenNotification = new ODBCWrapper.StoredProcedure("Get_DataForAdyenNotification");
            spGetDataForAdyenNotification.SetConnectionKey("CONNECTION_STRING");
            spGetDataForAdyenNotification.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = spGetDataForAdyenNotification.ExecuteDataSet();
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    short siIsCreatedByCallback = 0;
                    long lPreviewModuleID = 0;
                    if (dt.Rows[0]["ID_In_Adyen_Transactions"] != DBNull.Value && dt.Rows[0]["ID_In_Adyen_Transactions"] != null)
                        Int64.TryParse(dt.Rows[0]["ID_In_Adyen_Transactions"].ToString(), out lIDInAdyenTransactions);
                    if (dt.Rows[0]["ID_In_Billing_Transactions"] != DBNull.Value && dt.Rows[0]["ID_In_Billing_Transactions"] != null)
                        Int64.TryParse(dt.Rows[0]["ID_In_Billing_Transactions"].ToString(), out lIDInBillingTransactions);
                    if (dt.Rows[0]["is_created_by_callback"] != DBNull.Value && dt.Rows[0]["is_created_by_callback"] != null && Int16.TryParse(dt.Rows[0]["is_created_by_callback"].ToString(), out siIsCreatedByCallback))
                        bIsCreatedByAdyenCallback = siIsCreatedByCallback > 0;
                    if (dt.Rows[0]["Purchase_Type"] != DBNull.Value && dt.Rows[0]["Purchase_Type"] != null)
                        Int32.TryParse(dt.Rows[0]["Purchase_Type"].ToString(), out nPurchaseType);
                    if (dt.Rows[0]["Purchase_ID"] != DBNull.Value && dt.Rows[0]["Purchase_ID"] != null)
                        Int64.TryParse(dt.Rows[0]["Purchase_ID"].ToString(), out lIDInRelevantCATable);
                    if (dt.Rows[0]["Preview_Module_ID"] != DBNull.Value && dt.Rows[0]["Preview_Module_ID"] != null)
                        Int64.TryParse(dt.Rows[0]["Preview_Module_ID"].ToString(), out lPreviewModuleID);
                    bIsPurchasedWithPreviewModule = lPreviewModuleID > 0;
                    res = true;

                }
            }

            return res;
        }

        public static bool Get_DataForAdyenNotification_And_HandleMail(string sPSPReference, ref long lIDInAdyenTransactions,
            ref long lIDInBillingTransactions, ref bool bIsCreatedByAdyenCallback, ref int nPurchaseType,
            ref long lIDInRelevantCATable, ref bool bIsPurchasedWithPreviewModule, ref bool shouldSendMail)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure spGetDataForAdyenNotification = new ODBCWrapper.StoredProcedure("Get_DataForAdyenNotification_And_HandleMail");
            spGetDataForAdyenNotification.SetConnectionKey("CONNECTION_STRING");
            spGetDataForAdyenNotification.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = spGetDataForAdyenNotification.ExecuteDataSet();
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    short siIsCreatedByCallback = 0;
                    long lPreviewModuleID = 0;
                    if (dt.Rows[0]["ID_In_Adyen_Transactions"] != DBNull.Value && dt.Rows[0]["ID_In_Adyen_Transactions"] != null)
                        Int64.TryParse(dt.Rows[0]["ID_In_Adyen_Transactions"].ToString(), out lIDInAdyenTransactions);
                    if (dt.Rows[0]["ID_In_Billing_Transactions"] != DBNull.Value && dt.Rows[0]["ID_In_Billing_Transactions"] != null)
                        Int64.TryParse(dt.Rows[0]["ID_In_Billing_Transactions"].ToString(), out lIDInBillingTransactions);
                    if (dt.Rows[0]["is_created_by_callback"] != DBNull.Value && dt.Rows[0]["is_created_by_callback"] != null && Int16.TryParse(dt.Rows[0]["is_created_by_callback"].ToString(), out siIsCreatedByCallback))
                        bIsCreatedByAdyenCallback = siIsCreatedByCallback > 0;
                    if (dt.Rows[0]["Purchase_Type"] != DBNull.Value && dt.Rows[0]["Purchase_Type"] != null)
                        Int32.TryParse(dt.Rows[0]["Purchase_Type"].ToString(), out nPurchaseType);
                    if (dt.Rows[0]["Purchase_ID"] != DBNull.Value && dt.Rows[0]["Purchase_ID"] != null)
                        Int64.TryParse(dt.Rows[0]["Purchase_ID"].ToString(), out lIDInRelevantCATable);
                    if (dt.Rows[0]["Preview_Module_ID"] != DBNull.Value && dt.Rows[0]["Preview_Module_ID"] != null)
                        Int64.TryParse(dt.Rows[0]["Preview_Module_ID"].ToString(), out lPreviewModuleID);
                    bIsPurchasedWithPreviewModule = lPreviewModuleID > 0;
                    res = true;

                }

                if (ds.Tables.Count > 1)
                {
                    DataTable secondTable = ds.Tables[1];

                    if (secondTable != null && secondTable.Rows != null && secondTable.Rows.Count > 0)
                    {
                        bool wasMailSend = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(secondTable.Rows[0], "IS_MAIL_SENT"));

                        shouldSendMail = bIsCreatedByAdyenCallback && !wasMailSend;
                    }
                }
            }

            return res;
        }

        public static void Update_AdyenTransactionStatusReasonLast4Digits(long lIDInAdyenTransactions, string sAdyenStatus, string sAdyenReason, string sLast4Digits)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_AdyenTransactionStatusReasonLast4Digits");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@ID", lIDInAdyenTransactions);
            sp.AddParameter("@AdyenStatus", sAdyenStatus);
            sp.AddParameter("@AdyenReason", sAdyenReason);
            sp.AddParameter("@Last4Digits", sLast4Digits);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.ExecuteNonQuery();
        }

        public static void Update_AdyenCancelOrRefundRequestStatus(string sCancelOrRefundPSPReference, int nCancelOrRefundStatus)
        {
            ODBCWrapper.StoredProcedure spUpdateCancelOrRefundRequestStatus = new ODBCWrapper.StoredProcedure("Update_AdyenCancelOrRefundRequestStatus");
            spUpdateCancelOrRefundRequestStatus.SetConnectionKey("CONNECTION_STRING");
            spUpdateCancelOrRefundRequestStatus.AddParameter("@CancelOrRefundPSPRef", sCancelOrRefundPSPReference);
            spUpdateCancelOrRefundRequestStatus.AddParameter("@RequestStatus", nCancelOrRefundStatus);
            spUpdateCancelOrRefundRequestStatus.AddParameter("@UpdateDate", DateTime.UtcNow);
            spUpdateCancelOrRefundRequestStatus.ExecuteNonQuery();

        }

        public static void Insert_NewAdyenCancelOrRefund(string sCancelOrRefundPSPRef, string sOriginalPSPRef, int nCancelOrRefundRequestStatus, int nRequestType, string sSiteGuid, double? price, string sCurrencyCode, int nGroupID, long lPurchaseID, int nType, string sReason, int nNumOfCancelOrRefundAttempts)
        {

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewAdyenCancelOrRefund");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", sSiteGuid);
            sp.AddParameter("@Price", price);
            sp.AddParameter("@CurrencyCode", sCurrencyCode);
            sp.AddParameter("@CancelOrRefundPSPReference", sCancelOrRefundPSPRef);
            sp.AddParameter("@OriginalPSPReference", sOriginalPSPRef);
            sp.AddParameter("@CancelOrRefundRequestStatus", nCancelOrRefundRequestStatus);
            sp.AddParameter("@RequestType", nRequestType);
            sp.AddParameter("@GroupID", nGroupID);
            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@CreateDate", dtToWriteToDB);
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@PurchaseType", nType);
            sp.AddParameter("@AdyenReason", sReason);
            sp.AddParameter("@NumOfCancelOrRefundAttempts", nNumOfCancelOrRefundAttempts);
            sp.ExecuteNonQuery();

        }

        public static bool Get_DataForAdyenCancelOrRefund(string sCancelOrRefundPSPReference, ref string sOriginalPSPReference, ref int nNumOfCancelOrRefundAttempts, ref bool bIsCancelOrRefundResultOfPreviewModule)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DataForAdyenCancelOrRefund");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@PSPReference", sCancelOrRefundPSPReference);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    long lPreviewModuleID = 0;
                    if (dt.Rows[0]["original_psp_reference"] != DBNull.Value && dt.Rows[0]["original_psp_reference"] != null)
                        sOriginalPSPReference = dt.Rows[0]["original_psp_reference"].ToString();
                    if (dt.Rows[0]["num_of_cancel_or_refund_attempts"] != DBNull.Value && dt.Rows[0]["num_of_cancel_or_refund_attempts"] != null)
                        Int32.TryParse(dt.Rows[0]["num_of_cancel_or_refund_attempts"].ToString(), out nNumOfCancelOrRefundAttempts);
                    if (dt.Rows[0]["preview_module_id"] != DBNull.Value && dt.Rows[0]["preview_module_id"] != null)
                        Int64.TryParse(dt.Rows[0]["preview_module_id"].ToString(), out lPreviewModuleID);
                    bIsCancelOrRefundResultOfPreviewModule = lPreviewModuleID > 0;
                    res = true;
                }
            }

            return res;

        }

        public static bool Get_DataForResendingAdyenCancelOrRefund(string sPSPReference, ref string sSiteGuid, ref int nGroupID, ref long lPurchaseID, ref int nType, ref double dChargePrice, ref string sCurrencyCode)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DataForResendingAdyenCancelOrRefund");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["site_guid"] != DBNull.Value && dt.Rows[0]["site_guid"] != null)
                        sSiteGuid = dt.Rows[0]["site_guid"].ToString();
                    if (dt.Rows[0]["group_id"] != DBNull.Value && dt.Rows[0]["group_id"] != null)
                        Int32.TryParse(dt.Rows[0]["group_id"].ToString(), out nGroupID);
                    if (dt.Rows[0]["purchase_id"] != DBNull.Value && dt.Rows[0]["purchase_id"] != null)
                        Int64.TryParse(dt.Rows[0]["purchase_id"].ToString(), out lPurchaseID);
                    if (dt.Rows[0]["type"] != DBNull.Value && dt.Rows[0]["type"] != null)
                        Int32.TryParse(dt.Rows[0]["type"].ToString(), out nType);
                    if (dt.Rows[0]["price"] != DBNull.Value && dt.Rows[0]["price"] != null)
                        Double.TryParse(dt.Rows[0]["price"].ToString(), out dChargePrice);
                    if (dt.Rows[0]["currency_code"] != DBNull.Value && dt.Rows[0]["currency_code"] != null)
                        sCurrencyCode = dt.Rows[0]["currency_code"].ToString();

                    res = true;
                }

            }

            return res;
        }

        public static bool Get_DataOfAdyenNotificationForAdyenCallback(string sPSPReference, ref string sEventCode, ref string sAdyenSuccess, ref string sLast4Digits, ref string sAdyenReason)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DataOfAdyenNotificationForAdyenCallback");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@PSPReference", sPSPReference);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Event_Code"] != DBNull.Value && dt.Rows[0]["Event_Code"] != null)
                        sEventCode = dt.Rows[0]["Event_Code"].ToString();
                    if (dt.Rows[0]["Adyen_Success"] != DBNull.Value && dt.Rows[0]["Adyen_Success"] != null)
                        sAdyenSuccess = dt.Rows[0]["Adyen_Success"].ToString();
                    if (dt.Rows[0]["Last_4_Digits"] != DBNull.Value && dt.Rows[0]["Last_4_Digits"] != null)
                        sLast4Digits = dt.Rows[0]["Last_4_Digits"].ToString();
                    if (dt.Rows[0]["Adyen_Reason"] != DBNull.Value && dt.Rows[0]["Adyen_Reason"] != null)
                        sAdyenReason = dt.Rows[0]["Adyen_Reason"].ToString();

                    res = true;

                }
            }

            return res;

        }

        public static DataSet Get_M1GroupParameters(int? nGroupID, string appID)
        {
            ODBCWrapper.StoredProcedure spGetM1GroupParameters = new ODBCWrapper.StoredProcedure("Get_M1GroupParameters");
            spGetM1GroupParameters.SetConnectionKey("BILLING_CONNECTION_STRING");
            spGetM1GroupParameters.AddNullableParameter<int?>("@GroupID", nGroupID);
            spGetM1GroupParameters.AddNullableParameter<string>("@AppID", appID);

            DataSet ds = spGetM1GroupParameters.ExecuteDataSet();
            return ds;
        }

        public static DataTable Get_M1Transactions(int nGroupID, int nTransactionStatus)
        {
            ODBCWrapper.StoredProcedure spGetM1Transactions = new ODBCWrapper.StoredProcedure("Get_M1Transactions");
            spGetM1Transactions.SetConnectionKey("BILLING_CONNECTION_STRING");
            spGetM1Transactions.AddParameter("@GroupID", nGroupID);
            spGetM1Transactions.AddParameter("@Status", nTransactionStatus);

            DataSet ds = spGetM1Transactions.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int Insert_M1FileHistoryRecord(int nGroupID, int nItemType, int nFileCounter, string sFileName)
        {
            ODBCWrapper.StoredProcedure spInsertM1File = new ODBCWrapper.StoredProcedure("Insert_M1FileHistoryRecord");
            spInsertM1File.SetConnectionKey("BILLING_CONNECTION_STRING");
            spInsertM1File.AddParameter("@GroupID", nGroupID);
            spInsertM1File.AddParameter("@ItemType", nItemType);
            spInsertM1File.AddParameter("@FileCounter", nFileCounter);
            spInsertM1File.AddParameter("@FileName", sFileName);

            int newFileID = spInsertM1File.ExecuteReturnValue<int>();
            return newFileID;
        }

        public static int Insert_M1Transaction(int nGroupID, string sSiteGUID, int nItemType, string sChargedMobileNumber, string sCustomerServiceID, double dPrice, int nCustomDataID, int nStatus)
        {
            ODBCWrapper.StoredProcedure spInsertM1Transaction = new ODBCWrapper.StoredProcedure("Insert_M1Transaction");
            spInsertM1Transaction.SetConnectionKey("CONNECTION_STRING");
            spInsertM1Transaction.AddParameter("@GroupID", nGroupID);
            spInsertM1Transaction.AddParameter("@SiteGuid", sSiteGUID);
            spInsertM1Transaction.AddParameter("@ItemType", nItemType);
            spInsertM1Transaction.AddParameter("@ChargedMobileNumber", sChargedMobileNumber);
            spInsertM1Transaction.AddParameter("@CustomerServiceID", sCustomerServiceID);
            spInsertM1Transaction.AddParameter("@Price", dPrice);
            spInsertM1Transaction.AddParameter("@CustomDataID", nCustomDataID);
            spInsertM1Transaction.AddParameter("@Status", nStatus);

            int newM1TransactionID = spInsertM1Transaction.ExecuteReturnValue<int>();
            return newM1TransactionID;
        }

        public static void UpdateM1Transactions(int nGroupID, List<int> transactionIDs, int nStatus, int nFileRefID)
        {
            ODBCWrapper.StoredProcedure spUpdateM1Transactions = new ODBCWrapper.StoredProcedure("Update_M1Transactions");
            spUpdateM1Transactions.SetConnectionKey("BILLING_CONNECTION_STRING");
            spUpdateM1Transactions.AddParameter("@GroupID", nGroupID);
            spUpdateM1Transactions.AddIDListParameter<int>("@M1TransactionsIDs", transactionIDs, "Id");
            spUpdateM1Transactions.AddParameter("@Status", nStatus);
            spUpdateM1Transactions.AddParameter("@FileRefID", nFileRefID);

            spUpdateM1Transactions.ExecuteNonQuery();
        }

        public static DataTable Get_M1CustomerServiceType(int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_M1CustomerServiceType");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static long Insert_NewCustomData(string sCustomData, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewCustomData");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@CustomData", sCustomData);
            sp.AddParameter("@CreateDate", DateTime.UtcNow);
            return sp.ExecuteReturnValue<long>();
        }

        public static long Insert_NewCustomData(string sCustomData)
        {
            return Insert_NewCustomData(sCustomData, string.Empty);
        }

        public static long Get_LatestCustomDataID(string sCustomData, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_LatestCustomDataID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@CustomData", sCustomData);
            return sp.ExecuteReturnValue<long>();
        }

        public static long Get_LatestCustomDataID(string sCustomData)
        {
            return Get_LatestCustomDataID(sCustomData, string.Empty);
        }

        public static bool Get_CustomDataByID(long lCustomDataID, ref string sCustomData, string sConnKey = BILLING_CONNECTION_STRING)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CustomDataByID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : BILLING_CONNECTION_STRING);
            sp.AddParameter("@CustomDataID", lCustomDataID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["customdata"] != DBNull.Value && dt.Rows[0]["customdata"] != null)
                    {
                        sCustomData = dt.Rows[0]["customdata"].ToString();
                        res = true;
                    }
                }
            }

            return res;

        }

        public static long Insert_NewCinepolisTransaction(long lSiteGuid, double dPrice, string sCurrencyCode,
            string sBankAuthorisationID, byte bytCinepolisTransactionStatus, long lCinepolisCustomDataID, long lGroupID,
            bool bIsActive, byte bytStatus, int? nUpdaterID, long lPurchaseID, int nBusinessModuleType, byte bytConfirmationSuccess,
            int nInternalCode, string sConfirmationMsg, string sKlicOperationID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_NewCinepolisTransaction");
            sp.SetConnectionKey("CONNECTION_STRING");

            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@Price", dPrice);
            if (sCurrencyCode == null)
                sp.AddParameter("@CurrencyCode", DBNull.Value);
            else
            {
                string sCurrencyCodeToWriteToDB = string.Empty;
                if (sCurrencyCode.Length < 4)
                    sCurrencyCodeToWriteToDB = sCurrencyCode;
                else
                    sCurrencyCodeToWriteToDB = sCurrencyCode.Substring(0, 3);
                sp.AddParameter("@CurrencyCode", sCurrencyCodeToWriteToDB);
            }
            sp.AddParameter("@BankAuthorisationID", sBankAuthorisationID);
            sp.AddParameter("@TransactionStatus", bytCinepolisTransactionStatus);
            sp.AddParameter("@CinepolisCustomDataID", lCinepolisCustomDataID);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@IsActive", bIsActive);
            sp.AddParameter("@Status", bytStatus);

            DateTime dtToWriteToDB = DateTime.UtcNow;
            sp.AddParameter("@CreateDate", dtToWriteToDB);
            sp.AddParameter("@UpdateDate", dtToWriteToDB);
            if (nUpdaterID.HasValue)
                sp.AddParameter("@UpdaterID", nUpdaterID.Value);
            else
                sp.AddParameter("@UpdaterID", DBNull.Value);

            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@BusinessModuleType", nBusinessModuleType);
            sp.AddParameter("@ConfirmationSuccess", bytConfirmationSuccess);
            sp.AddParameter("@ConfirmationInternalCode", nInternalCode);
            sp.AddParameter("@ConfirmationMsg", sConfirmationMsg);
            sp.AddParameter("@KlicOperationID", sKlicOperationID);

            return sp.ExecuteReturnValue<long>();

        }

        public static long Get_CinepolisTransactionID(long lSiteGuid, double dPrice, string sBankAuthorisationID,
            byte bytCinepolisTransactionStatus, long lCinepolisCustomDataID, long lGroupID, bool bIsActive,
            byte bytStatus, int nBusinessModuleType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CinepolisTransactionID");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@Price", dPrice);
            sp.AddParameter("@BankAuthorisationID", sBankAuthorisationID);
            sp.AddParameter("@TransactionStatus", bytCinepolisTransactionStatus);
            sp.AddParameter("@CustomDataID", lCinepolisCustomDataID);
            sp.AddParameter("@GroupID", lGroupID);
            sp.AddParameter("@IsActive", bIsActive);
            sp.AddParameter("@Status", bytStatus);
            sp.AddParameter("@BusinessModuleType", nBusinessModuleType);


            return sp.ExecuteReturnValue<long>();

        }

        public static bool Update_PurchaseIDInCinepolisTransactions(long lCinepolisTransactionID, long lPurchaseID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PurchaseIDInCinepolisTransactions");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@PurchaseID", lPurchaseID);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@CinepolisTransactionID", lCinepolisTransactionID);
            return sp.ExecuteReturnValue<bool>();

        }

        public static bool Update_CinepolisConfirmationData(long lCinepolisTransactionID, byte bytConfirmationSuccess,
            int nInternalCode, string sMessage, long lPurchaseID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_CinepolisConfirmationData");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@CinepolisTransactionID", lCinepolisTransactionID);
            sp.AddParameter("@ConfirmationSuccess", bytConfirmationSuccess);
            sp.AddParameter("@InternalCode", nInternalCode);
            sp.AddParameter("@Msg", sMessage);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            return sp.ExecuteReturnValue<bool>();

        }

        public static bool Update_CinepolisConfirmationDataByBillingID(long lBillingTransactionID, byte bytConfirmationSuccess,
            int nInternalCode, string sMessage, string sConnKey)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_CinepolisConfirmationDataByBillingID");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            sp.AddParameter("@BillingTransactionID", lBillingTransactionID);
            sp.AddParameter("@ConfirmationSuccess", bytConfirmationSuccess);
            sp.AddParameter("@InternalCode", nInternalCode);

            sp.AddParameter("@Msg", sMessage);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);

            return sp.ExecuteReturnValue<bool>();

        }

        public static bool Update_CinepolisTransactionStatus(byte bytTransactionStatus, long lCinepolisTransactionID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_CinepolisTransactionStatus");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@TransactionStatus", bytTransactionStatus);
            sp.AddParameter("@UpdateDate", DateTime.UtcNow);
            sp.AddParameter("@CinepolisTransactionID", lCinepolisTransactionID);

            return sp.ExecuteReturnValue<bool>();
        }


        public static bool Get_CinepolisSecret(string sConnKey, ref string sSecret)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CinepolisSecret");
            sp.SetConnectionKey(!string.IsNullOrEmpty(sConnKey) ? sConnKey : "CONNECTION_STRING");
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
            {
                string temp = string.Empty;
                object o = ds.Tables[0].Rows[0]["group_secret"];
                if (o != DBNull.Value && o != null)
                    temp = o.ToString();
                if (temp.Length > 0)
                {
                    sSecret = temp;
                    res = true;
                }
            }

            return res;

        }

        public static bool Get_IsSendMail(string pspReference, byte currentMailType, ref bool isSendMail, out List<string[]> deletedData)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_IsSendMail");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@CurrentMailType", currentMailType);
            sp.AddParameter("@PSPReference", pspReference);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    isSendMail = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["is_send_mail"]) == 1;
                }

                DataTable deleted = ds.Tables[1];
                if (deleted != null && deleted.Rows != null && deleted.Rows.Count > 0)
                {
                    deletedData = new List<string[]>(deleted.Rows.Count);
                    for (int i = 0; i < deleted.Rows.Count; i++)
                    {
                        string id = ODBCWrapper.Utils.GetSafeStr(deleted.Rows[i]["ID"]);
                        string pspRef = ODBCWrapper.Utils.GetSafeStr(deleted.Rows[i]["psp_reference"]);
                        string lastMailType = ODBCWrapper.Utils.GetSafeStr(deleted.Rows[i]["last_mail_type"]);
                        deletedData.Add(new string[3] { id, pspRef, lastMailType });
                    }
                }
                else
                {
                    deletedData = new List<string[]>(0);
                }
            }
            else
            {
                deletedData = new List<string[]>(0);
            }

            return res;
        }

        public static bool Get_InitialAdyenCallbackData(string skinCode, long customDataID, ref int groupID, ref string baseRedirectUrl,
            ref string customDataXml)
        {
            bool res = false;
            StoredProcedure sp = new StoredProcedure("Get_InitialAdyenCallbackData");
            sp.SetConnectionKey("CONNECTION_STRING");
            sp.AddParameter("@SkinCode", skinCode);
            sp.AddParameter("@CustomDataID", customDataID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                res = true;
                DataTable agp = ds.Tables[0];
                if (agp != null && agp.Rows != null && agp.Rows.Count > 0)
                {
                    groupID = ODBCWrapper.Utils.GetIntSafeVal(agp.Rows[0]["group_id"]);
                    baseRedirectUrl = ODBCWrapper.Utils.GetSafeStr(agp.Rows[0]["base_redirect_url"]);
                }
                DataTable cd = ds.Tables[1];
                if (cd != null && cd.Rows != null && cd.Rows.Count > 0)
                {
                    customDataXml = ODBCWrapper.Utils.GetSafeStr(cd.Rows[0]["CUSTOMDATA"]);
                }
            }

            return res;
        }

        /// <summary>
        /// Insert a new record to offline_transactions with given parameters. Returns the new transaction Id
        /// </summary>
        /// <param name="p_nSiteGuid"></param>
        /// <param name="p_dPrice"></param>
        /// <param name="p_sCurrencyCode"></param>
        /// <param name="p_nGroupId"></param>
        /// <param name="p_sOfflineCustomData"></param>
        /// <param name="p_nUpdaterId"></param>
        /// <returns></returns>
        public static long Insert_NewOfflineTransaction(long p_nSiteGuid, double p_dPrice, string p_sCurrencyCode, int p_nGroupId, string p_sOfflineCustomData, int? p_nUpdaterId)
        {
            long lTransactionId = 0;

            object oUpdaterId = DBNull.Value;

            if (p_nUpdaterId.HasValue)
            {
                oUpdaterId = p_nUpdaterId.Value;
            }

            StoredProcedure spInsertStoredProcedure = new StoredProcedure("Insert_NewOfflineTransaction");
            spInsertStoredProcedure.SetConnectionKey("CONNECTION_STRING");
            spInsertStoredProcedure.AddParameter("SiteGuid", p_nSiteGuid);
            spInsertStoredProcedure.AddParameter("Price", p_dPrice);
            spInsertStoredProcedure.AddParameter("CurrencyCode", p_sCurrencyCode);
            spInsertStoredProcedure.AddParameter("GroupID", p_nGroupId);
            spInsertStoredProcedure.AddParameter("OfflineCustomData", p_sOfflineCustomData);
            spInsertStoredProcedure.AddParameter("UpdaterID", oUpdaterId);

            lTransactionId = spInsertStoredProcedure.ExecuteReturnValue<long>();

            return (lTransactionId);
        }

        public static string getEmailDateFormat(int groupId)
        {
            string dateFormat = string.Empty;
            StoredProcedure sp = new StoredProcedure("Get_EmailDateFormat");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0] != null)
            {
                if (ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    dateFormat = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "date_email_format");
                }
            }
            return dateFormat;
        }

        public static int Insert_SmartSunTransaction(int nGroupID, string sSiteGUID, int nItemType, string sMsisdn, double dPrice, int nCustomDataID,
            int nReferenceTransactionID, int nStatus)
        {
            ODBCWrapper.StoredProcedure spInsertSmartSunTransaction = new ODBCWrapper.StoredProcedure("Insert_SmartSunTransaction");
            spInsertSmartSunTransaction.SetConnectionKey("CONNECTION_STRING");
            spInsertSmartSunTransaction.AddParameter("@GroupID", nGroupID);
            spInsertSmartSunTransaction.AddParameter("@SiteGuid", sSiteGUID);
            spInsertSmartSunTransaction.AddParameter("@ItemType", nItemType);
            spInsertSmartSunTransaction.AddParameter("@Msisdn", sMsisdn);
            spInsertSmartSunTransaction.AddParameter("@ReferenceTransactionID", nReferenceTransactionID);
            spInsertSmartSunTransaction.AddParameter("@Price", dPrice);
            spInsertSmartSunTransaction.AddParameter("@CustomDataID", nCustomDataID);
            spInsertSmartSunTransaction.AddParameter("@Status", nStatus);

            int newTransactionID = spInsertSmartSunTransaction.ExecuteReturnValue<int>();
            return newTransactionID;
        }

        public static void UpdateSmartSunTransactionStatus(int nGroupID, int nTransactionID, int nStatus)
        {
            ODBCWrapper.StoredProcedure spUpdateSmartSunTransactions = new ODBCWrapper.StoredProcedure("Update_SmartSunTransaction");
            spUpdateSmartSunTransactions.SetConnectionKey("BILLING_CONNECTION_STRING");
            spUpdateSmartSunTransactions.AddParameter("@GroupID", nGroupID);
            spUpdateSmartSunTransactions.AddParameter("@TransactionID", nTransactionID);
            spUpdateSmartSunTransactions.AddParameter("@Status", nStatus);

            spUpdateSmartSunTransactions.ExecuteNonQuery();
        }

        public static string GetClientIDFromGroupParams(int groupID)
        {
            string sRet = String.Empty;

            ODBCWrapper.DataSetSelectQuery selectQuery = null;

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "SELECT ID, CLIENT_ID FROM GROUPS_OPERATORS WITH (NOLOCK) WHERE IS_ACTIVE=1 AND STATUS=1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupID);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sRet = selectQuery.Table("query").DefaultView[0].Row["CLIENT_ID"].ToString();
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

            return sRet;
        }

        public static List<PaymentGateway> GetPaymentGatewaySettingsList(int groupID, int paymentGatewayId = 0, int status = 1, int isActive = 1)
        {
            List<PaymentGateway> res = new List<PaymentGateway>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGWSettingsList");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@paymentGWId", paymentGatewayId);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    DataTable dtConfig = ds.Tables[1];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        PaymentGateway pgw = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            pgw = new PaymentGateway();
                            pgw.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            pgw.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            pgw.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                            pgw.PendingInterval = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_interval");
                            pgw.PendingRetries = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_retries");
                            pgw.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            pgw.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            pgw.TransactUrl = ODBCWrapper.Utils.GetSafeStr(dr, "transact_url");
                            pgw.StatusUrl = ODBCWrapper.Utils.GetSafeStr(dr, "status_url");
                            pgw.RenewUrl = ODBCWrapper.Utils.GetSafeStr(dr, "renew_url");
                            pgw.RenewalIntervalMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_interval_minutes");
                            pgw.RenewalStartMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_start_minutes");
                            pgw.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            int isDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default");
                            pgw.IsDefault = isDefault == 1;
                            pgw.SupportPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_payment_method_support") == 1;


                            if (dtConfig != null)
                            {
                                DataRow[] drpc = dtConfig.Select("payment_gateway_id =" + pgw.ID);

                                foreach (DataRow drp in drpc)
                                {
                                    string key = ODBCWrapper.Utils.GetSafeStr(drp, "key");
                                    string value = ODBCWrapper.Utils.GetSafeStr(drp, "value");
                                    if (pgw.Settings == null)
                                    {
                                        pgw.Settings = new List<PaymentGatewaySettings>();
                                    }
                                    pgw.Settings.Add(new PaymentGatewaySettings(key, value));
                                }
                            }
                            res.Add(pgw);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<PaymentGateway>();
            }
            return res;
        }

        public static List<PaymentGateway> GetPaymentGatewaySettingsList(int groupID, string paymentGWName = "", int status = 1, int isActive = 1)
        {
            List<PaymentGateway> res = new List<PaymentGateway>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGWSettingsListByName");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@paymentGWName", paymentGWName);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    DataTable dtConfig = ds.Tables[1];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        PaymentGateway pgw = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            pgw = new PaymentGateway();
                            pgw.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            pgw.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            pgw.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                            pgw.PendingInterval = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_interval");
                            pgw.PendingRetries = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_retries");
                            pgw.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            pgw.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            pgw.TransactUrl = ODBCWrapper.Utils.GetSafeStr(dr, "transact_url");
                            pgw.StatusUrl = ODBCWrapper.Utils.GetSafeStr(dr, "status_url");
                            pgw.RenewUrl = ODBCWrapper.Utils.GetSafeStr(dr, "renew_url");
                            pgw.RenewalIntervalMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_interval_minutes");
                            pgw.RenewalStartMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_start_minutes");
                            pgw.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            int isDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default");
                            pgw.IsDefault = isDefault == 1 ? true : false;

                            if (dtConfig != null)
                            {
                                DataRow[] drpc = dtConfig.Select("payment_gateway_id =" + pgw.ID);

                                foreach (DataRow drp in drpc)
                                {
                                    string key = ODBCWrapper.Utils.GetSafeStr(drp, "key");
                                    string value = ODBCWrapper.Utils.GetSafeStr(drp, "value");
                                    if (pgw.Settings == null)
                                    {
                                        pgw.Settings = new List<PaymentGatewaySettings>();
                                    }
                                    pgw.Settings.Add(new PaymentGatewaySettings(key, value));
                                }
                            }
                            res.Add(pgw);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<PaymentGateway>();
            }
            return res;
        }

        public static bool SetPaymentGWSettings(int groupID, int paymentGWID, List<PaymentGatewaySettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_PaymentGateway_Settings");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", paymentGWID);

                DataTable dt = CreateDataTable(settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        private static DataTable CreateDataTable(List<PaymentGatewaySettings> list)
        {
            DataTable resultTable = new DataTable("resultTable"); ;
            try
            {
                resultTable.Columns.Add("key", typeof(string));
                resultTable.Columns.Add("value", typeof(string));

                foreach (PaymentGatewaySettings item in list)
                {
                    DataRow row = resultTable.NewRow();
                    row["key"] = item.key;
                    row["value"] = item.value;
                    resultTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return null;
            }

            return resultTable;
        }

        public static bool DeletePaymentGateway(int groupID, int paymentGatewayId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_PaymentGateway");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", paymentGatewayId);
                bool isDelete = sp.ExecuteReturnValue<bool>();
                return isDelete;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static List<PaymentGateway> GetPaymentGWList(int groupID, int status = 1, int isActive = 1)
        {
            List<PaymentGateway> res = new List<PaymentGateway>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayList");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSetWithListParam();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        PaymentGateway paymentGateway = null;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            paymentGateway = new PaymentGateway();

                            paymentGateway.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            paymentGateway.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            paymentGateway.Selected = ODBCWrapper.Utils.GetIntSafeVal(dr, "selected");
                            int isDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default");
                            paymentGateway.IsDefault = isDefault == 1 ? true : false;
                            paymentGateway.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(dr, "external_identifier");
                            paymentGateway.PendingInterval = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_interval");
                            paymentGateway.PendingRetries = ODBCWrapper.Utils.GetIntSafeVal(dr, "pending_retries");
                            paymentGateway.SharedSecret = ODBCWrapper.Utils.GetSafeStr(dr, "shared_secret");
                            paymentGateway.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(dr, "adapter_url");
                            paymentGateway.TransactUrl = ODBCWrapper.Utils.GetSafeStr(dr, "transact_url");
                            paymentGateway.StatusUrl = ODBCWrapper.Utils.GetSafeStr(dr, "status_url");
                            paymentGateway.RenewUrl = ODBCWrapper.Utils.GetSafeStr(dr, "renew_url");
                            paymentGateway.Status = ODBCWrapper.Utils.GetIntSafeVal(dr, "status");
                            paymentGateway.RenewalIntervalMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_interval_minutes");
                            paymentGateway.RenewalStartMinutes = ODBCWrapper.Utils.GetIntSafeVal(dr, "renewal_start_minutes");
                            paymentGateway.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                            int supportPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_payment_method_support");
                            paymentGateway.SupportPaymentMethod = supportPaymentMethod == 1;

                            res.Add(paymentGateway);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<PaymentGateway>();
            }
            return res;
        }

        public static List<PaymentGatewaySelectedBy> GetHouseholdPaymentGateways(int groupID, long householdId, int? selected)
        {
            List<PaymentGatewaySelectedBy> res = new List<PaymentGatewaySelectedBy>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayConfiguredList");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@houseHoldID", householdId);
                if (selected.HasValue)
                {
                    sp.AddParameter("@selected", selected.Value);
                }
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dtPG = ds.Tables[0];
                    if (dtPG != null && dtPG.Rows != null && dtPG.Rows.Count > 0)
                    {
                        PaymentGatewaySelectedBy paymentGateway = null;
                        int isDefault = 0;
                        foreach (DataRow dr in dtPG.Rows)
                        {
                            paymentGateway = new PaymentGatewaySelectedBy();
                            paymentGateway.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                            paymentGateway.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                            int household = ODBCWrapper.Utils.GetIntSafeVal(dr, "house_hold_id");
                            paymentGateway.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.None;
                            isDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default");

                            if (isDefault == 1)
                            {
                                paymentGateway.IsDefault = true;
                                paymentGateway.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.Account;
                            }

                            if (household > 0)
                            {
                                paymentGateway.IsDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "selected") == 1 ? true : false;
                                paymentGateway.By = ApiObjects.eHouseholdPaymentGatewaySelectedBy.Household;
                            }

                            res.Add(paymentGateway);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                res = new List<PaymentGatewaySelectedBy>();
            }
            return res;
        }

        public static PaymentGateway GetSelectedHouseholdPaymentGateway(int groupID, long householdId, ref string chargeId)
        {
            PaymentGateway paymentGateway = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SelectedHouseholdPaymentGateway");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@houseHoldID", householdId);
                sp.AddParameter("@status", 1);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    paymentGateway = new PaymentGateway();
                    paymentGateway.ID = ODBCWrapper.Utils.GetIntSafeVal(dr, "ID");
                    paymentGateway.Name = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                    paymentGateway.Selected = ODBCWrapper.Utils.GetIntSafeVal(dr, "selected");
                    int isDefault = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_default");
                    paymentGateway.IsDefault = isDefault == 1 ? true : false;
                    paymentGateway.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "external_identifier");
                    paymentGateway.PendingInterval = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "pending_interval");
                    paymentGateway.PendingRetries = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "pending_retries");
                    paymentGateway.SharedSecret = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "shared_secret");
                    paymentGateway.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "adapter_url");
                    paymentGateway.TransactUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "transact_url");
                    paymentGateway.StatusUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "status_url");
                    paymentGateway.RenewUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "renew_url");
                    paymentGateway.Status = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "status");
                    paymentGateway.RenewalIntervalMinutes = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "renewal_interval_minutes");
                    paymentGateway.RenewalStartMinutes = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "renewal_start_minutes");
                    paymentGateway.IsActive = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_active");
                    int supportPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_payment_method_support");
                    paymentGateway.SupportPaymentMethod = supportPaymentMethod == 1;
                    chargeId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "charge_Id");
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return paymentGateway;
        }

        public static DataRow GetLatestPaymentGatewayTransaction(int groupId, long householdId, string billingGuid)
        {
            DataRow dataRow = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_LatestPaymentGatewayTransaction");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@domain_id", householdId);
                sp.AddParameter("@billing_guid", billingGuid);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return ds.Tables[0].Rows[0];
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return dataRow;
        }

        public static bool SetPaymentGatewayHousehold(int groupID, int paymentGwID, int householdID, int? selected, string chargeID = null, int status = 1)
        {
            bool res = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_PaymentGateway_Household");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@PaymentGWID", paymentGwID);
                sp.AddParameter("@householdID", householdID);
                sp.AddParameter("@status", status);
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@charge_id", chargeID);
                if (selected.HasValue)
                {
                    sp.AddParameter("@selected", selected.Value);
                }
                res = sp.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return res;
        }

        public static bool DeletePaymentGW(int groupID, int paymentGwID, List<PaymentGatewaySettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_PaymentGateway_Settings");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", paymentGwID);
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

        public static bool SetPaymentGateway(int groupID, PaymentGateway paymentGateway)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_PaymentGateway");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", paymentGateway.ID);
                sp.AddParameter("@name", paymentGateway.Name);
                sp.AddParameter("@external_identifier", paymentGateway.ExternalIdentifier);
                sp.AddParameter("@pending_interval", paymentGateway.PendingInterval);
                sp.AddParameter("@pending_retries", paymentGateway.PendingRetries);
                sp.AddParameter("@shared_secret", paymentGateway.SharedSecret);
                sp.AddParameter("@adapter_url", paymentGateway.AdapterUrl);
                sp.AddParameter("@transact_url", paymentGateway.TransactUrl);
                sp.AddParameter("@status_url", paymentGateway.StatusUrl);
                sp.AddParameter("@renew_url", paymentGateway.RenewUrl);
                sp.AddParameter("@isDefault", paymentGateway.IsDefault);
                sp.AddParameter("@isActive", paymentGateway.IsActive);
                sp.AddParameter("@renewal_interval", paymentGateway.RenewalIntervalMinutes);
                sp.AddParameter("@renewal_start", paymentGateway.RenewalStartMinutes);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return false;
            }
        }

        public static PaymentGateway InsertPaymentGW(int groupID, PaymentGateway pgw)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_PaymentGW");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@name", pgw.Name);
                sp.AddParameter("@adapter_url", pgw.AdapterUrl);
                sp.AddParameter("@transact_url", pgw.TransactUrl);
                sp.AddParameter("@status_url", pgw.StatusUrl);
                sp.AddParameter("@renew_url", pgw.RenewUrl);
                sp.AddParameter("@external_identifier", pgw.ExternalIdentifier);
                sp.AddParameter("@pending_interval", pgw.PendingInterval);
                sp.AddParameter("@pending_retries", pgw.PendingRetries);
                sp.AddParameter("@shared_secret", pgw.SharedSecret);
                sp.AddParameter("@isDefault", pgw.IsDefault);
                sp.AddParameter("@isActive", pgw.IsActive);
                sp.AddParameter("@renewal_interval", pgw.RenewalIntervalMinutes);
                sp.AddParameter("@renewal_start", pgw.RenewalStartMinutes);

                DataTable dt = CreateDataTable(pgw.Settings);
                sp.AddDataTableParameter("@KeyValueList", dt);

                DataSet ds = sp.ExecuteDataSet();

                return CreatePaymentGateway(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
            }

            return null;
        }

        public static bool InsertPaymentGatewaySettings(int groupID, int paymentGWID, List<PaymentGatewaySettings> settings)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_PaymentGWSettings");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@GroupID", groupID);
                sp.AddParameter("@ID", paymentGWID);

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

        public static bool DeletePaymentGatewayHousehold(int groupID, int paymentGwID, int householdId)
        {
            bool res = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_PaymentGateway_Household");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@paymentGatewayId", paymentGwID);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@groupId", groupID);
                res = sp.ExecuteReturnValue<bool>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return res;
        }

        public static int InsertPaymentGWPending(int groupID, PaymentGatewayPending paymentGatewayPending)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_PaymentGWPending");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupID);
                sp.AddParameter("@next_retry_date", paymentGatewayPending.NextRetryDate);
                sp.AddParameter("@adapter_retry_count", paymentGatewayPending.AdapterRetryCount);
                sp.AddParameter("@payment_gateway_transaction_id", paymentGatewayPending.PaymentGatewayTransactionId);
                sp.AddParameter("@billing_guid", paymentGatewayPending.BillingGuid);

                int newPaymentGWPending = sp.ExecuteReturnValue<int>();
                return newPaymentGWPending;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static int InsertPaymentGatewayTransaction(int groupID, long domainId, long siteGuid, PaymentGatewayTransaction paymentGateway)
        {
            try
            {
                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Insert_PaymentGatewayTransaction");
                storedProcedure.SetConnectionKey("BILLING_CONNECTION_STRING");
                storedProcedure.AddParameter("@group_id", groupID);
                storedProcedure.AddParameter("@domain_id", domainId);
                storedProcedure.AddParameter("@site_guid", siteGuid);
                storedProcedure.AddParameter("@payment_gateway_id", paymentGateway.PaymentGatewayID);
                storedProcedure.AddParameter("@external_transaction_id", paymentGateway.ExternalTransactionId);
                storedProcedure.AddParameter("@external_status", paymentGateway.ExternalStatus);
                storedProcedure.AddParameter("@product_type", paymentGateway.ProductType);
                storedProcedure.AddParameter("@product_id", paymentGateway.ProductId);
                storedProcedure.AddParameter("@billing_guid", paymentGateway.BillingGuid);
                storedProcedure.AddParameter("@content_id", paymentGateway.ContentId);
                storedProcedure.AddParameter("@message", paymentGateway.Message);
                storedProcedure.AddParameter("@state", paymentGateway.State);
                storedProcedure.AddParameter("@failReason", paymentGateway.FailReason);
                storedProcedure.AddParameter("@paymentDetails", paymentGateway.PaymentDetails);
                storedProcedure.AddParameter("@paymentMethod", paymentGateway.PaymentMethod);
                storedProcedure.AddParameter("@paymentMethodId", paymentGateway.PaymentMethodId);

                int newTransactionID = storedProcedure.ExecuteReturnValue<int>();

                return newTransactionID;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static int GetPaymentGWInternalID(int groupID, string externaIdentifier)
        {
            int paymentGWID = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayByExternalD");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupID", groupID);
                sp.AddParameter("@external_identifier", externaIdentifier);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    paymentGWID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return paymentGWID;
        }

        public static string GetPaymentGWChargeID(int paymentGWID, long householdID, ref bool isPaymentGWHouseholdExist)
        {
            string chargeID = string.Empty;
            isPaymentGWHouseholdExist = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayChargeId");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@payment_gateway_id", paymentGWID);
                sp.AddParameter("@household_Id", householdID);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    chargeID = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "charge_id");
                    isPaymentGWHouseholdExist = true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return chargeID;
        }

        public static string GetPaymentGWChargeID(int paymentGWID, long householdID)
        {
            bool isPaymentGWHouseholdExist = false;

            return GetPaymentGWChargeID(paymentGWID, householdID, ref isPaymentGWHouseholdExist);

        }

        public static bool UpdatePaymentGatewayPendingTransaction(string billingGuid, int adapterTransactionState, string externalStatus, string externalMessage, int failReason)
        {
            int rows = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PaymentGatewayPendingTransaction");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@billing_guid", billingGuid);
                sp.AddParameter("@adapter_transaction_state", adapterTransactionState);
                sp.AddParameter("@fail_reason", failReason);
                sp.AddParameter("@external_status", externalStatus);
                sp.AddParameter("@external_message", externalMessage);

                rows = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating payment Gateway pending transaction: ex = {0}, billingGuid = {1}", ex, billingGuid);
                return false;
            }

            return rows > 0;
        }

        public static bool GetPendingPaymentGatewayTransactionDetails(int paymentGatewayId, string externalTransactionId, out string billingGuid, out int productType,
            out int transactionState, out int pendingTransactionState)
        {
            billingGuid = string.Empty;
            productType = 0;
            transactionState = 0;
            pendingTransactionState = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PendingPaymentGatewayTransactionDetails");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@payment_gateway_id", paymentGatewayId);
                sp.AddParameter("@external_transaction_id", externalTransactionId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        billingGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["billing_guid"]);
                        productType = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["product_type"]);
                        transactionState = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["state"]);
                        pendingTransactionState = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["pgpState"]);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Getting payment gateway transaction: ex = {0}, paymentGatewayId = {1}, externalTransactionId = {2}", ex, paymentGatewayId, externalTransactionId);
            }
            return false;
        }

        public static HouseholdPaymentGateway GetHouseholdPaymentGateway(int groupID, int paymentGatewayId, long householdId, int status = 1)
        {
            HouseholdPaymentGateway res = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayHousehold");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@status", status);
                DataSet ds = sp.ExecuteDataSet();


                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    res = new HouseholdPaymentGateway()
                    {
                        PaymentGatewayId = paymentGatewayId,
                        Selected = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "selected"),
                        ChargeId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "charge_id"),
                        HouseholdId = householdId
                    };
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return res;
        }


        public static PaymentGateway GetPaymentGateway(int groupID, int paymentGatewayId, int? isActive = 1, int status = 1)
        {
            PaymentGateway res = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGateway");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@status", status);
                if (isActive.HasValue)
                {
                    sp.AddParameter("@isActive", isActive.Value);
                }

                DataSet ds = sp.ExecuteDataSet();

                res = CreatePaymentGateway(ds);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return res;
        }

        public static bool GetPaymentGatewayFailReason(int failReasonCode, out bool failReasonCodeExist)
        {
            int createTransaction = 0;
            string description = string.Empty;
            failReasonCodeExist = false;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayFailReason");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@failReason", failReasonCode);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    createTransaction = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "CREATE_TRANSACTION");
                    description = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "DESCRIPTION");
                    failReasonCodeExist = true;
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return createTransaction == 1;
        }

        public static PaymentGatewayTransaction GetPaymentGatewayTransactionByID(long id, string connectionKey = BILLING_CONNECTION_STRING)
        {
            PaymentGatewayTransaction response = null;

            DataRow row = ODBCWrapper.Utils.GetTableSingleRow("payment_gateway_transactions", id, connectionKey);

            if (row != null)
            {
                response = new PaymentGatewayTransaction()
                {
                    ID = (int)id,
                    BillingGuid = ODBCWrapper.Utils.ExtractString(row, "billing_guid"),
                    ContentId = ODBCWrapper.Utils.ExtractInteger(row, "content_id"),
                    ExternalStatus = ODBCWrapper.Utils.ExtractString(row, "external_status"),
                    ExternalTransactionId = ODBCWrapper.Utils.ExtractString(row, "external_transaction_id"),
                    FailReason = ODBCWrapper.Utils.ExtractInteger(row, "fail_reason"),
                    Message = ODBCWrapper.Utils.ExtractString(row, "message"),
                    PaymentDetails = ODBCWrapper.Utils.ExtractString(row, "payment_details"),
                    PaymentGatewayID = ODBCWrapper.Utils.ExtractInteger(row, "payment_gateway_id"),
                    PaymentMethod = ODBCWrapper.Utils.ExtractString(row, "payment_method"),
                    ProductId = ODBCWrapper.Utils.ExtractInteger(row, "product_id"),
                    ProductType = ODBCWrapper.Utils.ExtractInteger(row, "product_type"),
                    State = ODBCWrapper.Utils.ExtractInteger(row, "state")
                };
            }

            return response;
        }

        public static int UpdatePaymentGatewayPending(int groupID, PaymentGatewayPending pending)
        {
            int result = 0;

            try
            {
                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Update_PaymentGatewayPending");
                storedProcedure.SetConnectionKey("BILLING_CONNECTION_STRING");
                storedProcedure.AddParameter("@billing_guid", pending.BillingGuid);
                storedProcedure.AddParameter("@next_retry_date", pending.NextRetryDate);
                storedProcedure.AddParameter("@adapter_retry_count", pending.AdapterRetryCount);

                result = storedProcedure.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating payment Gateway pending pending: ex = {0}, billingGuid = {1}", ex, pending.BillingGuid);
            }

            return result;
        }

        public static PaymentGateway SetPaymentGatewaySharedSecret(int groupID, int paymentGatewayId, string sharedSecret)
        {
            PaymentGateway result = null;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_PaymentGatewaySharedSecret");
            sp.SetConnectionKey("BILLING_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupID);
            sp.AddParameter("@id", paymentGatewayId);
            sp.AddParameter("@sharedSecret", sharedSecret);

            DataSet ds = sp.ExecuteDataSet();

            result = CreatePaymentGateway(ds);

            return result;
        }

        private static PaymentGateway CreatePaymentGateway(DataSet ds)
        {
            PaymentGateway result = null;

            if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                result = new PaymentGateway();
                result.ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                result.Name = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "name");
                result.ExternalIdentifier = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "external_identifier");
                result.PendingInterval = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "pending_interval");
                result.PendingRetries = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "pending_retries");
                result.SharedSecret = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "shared_secret");
                result.AdapterUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "adapter_url");
                result.TransactUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "transact_url");
                result.StatusUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "status_url");
                result.RenewUrl = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "renew_url");
                result.Status = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "status");
                result.RenewalIntervalMinutes = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "renewal_interval_minutes");
                result.RenewalStartMinutes = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "renewal_start_minutes");
                result.IsActive = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_active");
                int DefaultPaymentGateway = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "DEFAULT_PAYMENT_GATEWAY");
                result.IsDefault = DefaultPaymentGateway == result.ID ? true : false;
                int supportPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "is_payment_method_support");
                result.SupportPaymentMethod = supportPaymentMethod == 1 ? true : false;
            }

            return result;

        }

        public static PaymentGatewayHouseholdPaymentMethod GetSelectedHouseholdPaymentGatewayPaymentMethod(int groupID, long householdId, int paymentGatewayId)
        {
            PaymentGatewayHouseholdPaymentMethod pghhpm = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SelectedHouseholdPaymentGatewayPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pghhpm = new PaymentGatewayHouseholdPaymentMethod();
                    pghhpm.PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_EXTERNAL_ID");
                    pghhpm.Id = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSelectedHouseholdPaymentGatewayPaymentMethod household {0}, payment gateway {1}, error {2}", householdId, paymentGatewayId, ex);
            }

            return pghhpm;
        }

        public static PaymentGatewayHouseholdPaymentMethod GetPaymentGatewayHouseholdPaymentMethod(int groupID, int paymentGatewayId, long householdId, int id)
        {
            PaymentGatewayHouseholdPaymentMethod pghhpm = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayHouseholdPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@Id", id);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pghhpm = new PaymentGatewayHouseholdPaymentMethod();
                    pghhpm.PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_EXTERNAL_ID");
                    pghhpm.PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "PAYMENT_METHOD_ID");
                    pghhpm.Id = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                    pghhpm.Selected = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "SELECTED") == 1 ? true : false;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetPaymentGatewayHouseholdPaymentMethod household: {0}, payment gateway: {1}, payment method:{2}, error {3}",
                    householdId, paymentGatewayId, id, ex);
            }

            return pghhpm;
        }

        public static PaymentGatewayHouseholdPaymentMethod GetPaymentGatewayHouseholdPaymentMethod(int groupID, int paymentGatewayId, long householdId, string paymentMethodExternalId)
        {
            PaymentGatewayHouseholdPaymentMethod pghhpm = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayHouseholdPaymentMethodByExternalId");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@paymentMethodExternalId", paymentMethodExternalId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pghhpm = new PaymentGatewayHouseholdPaymentMethod();
                    pghhpm.PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_EXTERNAL_ID");
                    pghhpm.Id = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetPaymentGatewayHouseholdPaymentMethod household: {0}, payment gateway: {1}, payment method:{2}, error {3}",
                    householdId, paymentGatewayId, paymentMethodExternalId, ex);
            }

            return pghhpm;
        }

        public static PaymentGatewayHouseholdPaymentMethod GetPaymentGatewayHouseholdPaymentMethod(int groupID, string externalIdentifier, int householdId,
            string paymentMethodName, string paymentMethodExternalId)
        {
            PaymentGatewayHouseholdPaymentMethod pghhpm = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayHouseholdByExternalId");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@externalIdentifier", externalIdentifier);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentMethodName", paymentMethodName);
                sp.AddParameter("@paymentMethodExternalId", paymentMethodExternalId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pghhpm = new PaymentGatewayHouseholdPaymentMethod();

                    pghhpm.PaymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "PAYMENT_GATEWAY_ID");
                    pghhpm.PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "PAYMENT_METHOD_ID");
                    pghhpm.HouseholdId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "HOUSEHOLD_ID");
                    pghhpm.PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_EXTERNAL_ID");
                    pghhpm.PaymentDetails = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_DETAILS");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetPaymentGatewayHouseholdPaymentMethod household: {0}, payment gateway: {1}, error {2}", householdId,
                    !string.IsNullOrEmpty(externalIdentifier) ? externalIdentifier : string.Empty, ex);
            }

            return pghhpm;
        }

        public static bool GetPaymentMethod(int groupID, int paymentGatewayId, int paymentMethodId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@paymentMethodId", paymentMethodId);

                bool isExist = sp.ExecuteReturnValue<bool>();
                return isExist;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static PaymentMethod GetPaymentMethod(int groupID, int paymentGatewayId, string paymentMethodName)
        {
            PaymentMethod paymentMethod = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayPaymentMethodByName");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@paymentMethodName", paymentMethodName);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        paymentMethod = new PaymentMethod();
                        paymentMethod.ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "Id");
                        paymentMethod.PaymentGatewayId = paymentGatewayId;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetPaymentMethod paymentGatewayId: {0}, paymentMethodName: {1}, error {2}", paymentGatewayId,
                 !string.IsNullOrEmpty(paymentMethodName) ? paymentMethodName : string.Empty, ex);
            }

            return paymentMethod;
        }

        public static int SetPaymentGatewayHouseholdPaymentMethod(int groupID, int paymentGatewayId, int householdId, int paymentMethodId, string paymentDetails,
            int? selected, string paymentMethodExternalId = null)
        {
            int pghhpm = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Set_PaymentGatewayHouseholdPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentMethodId", paymentMethodId);
                sp.AddParameter("@paymentMethodExternalId", paymentMethodExternalId);
                sp.AddParameter("@paymentDetails", paymentDetails);
                if (selected.HasValue)
                {
                    sp.AddParameter("@selected", selected.Value);
                }

                pghhpm = sp.ExecuteReturnValue<int>();

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed calling to Set_PaymentGatewayHouseholdPaymentMethod. GID:{0},PGID:{1}. Household:{2}. Exception: {3}", groupID, paymentGatewayId, householdId, ex);
            }
            return pghhpm;
        }

        public static bool RemovePaymentGatewayHouseholdPaymentMethod(int paymentMethodId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Remove_PaymentGatewayHouseholdPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@ID", paymentMethodId);

                bool isSet = sp.ExecuteReturnValue<bool>();
                return isSet;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static PaymentMethod Insert_PaymentGatewayPaymentMethod(int groupId, int paymentGatewayId, string name, bool allowMultiInstance)
        {
            PaymentMethod response = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_PaymentGatewayPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@payment_gateway_id", paymentGatewayId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@allow_multi_instance", allowMultiInstance ? 1 : 0);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        response = new PaymentMethod()
                        {
                            ID = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "ID"),
                            PaymentGatewayId = paymentGatewayId,
                            Name = name,
                            AllowMultiInstance = allowMultiInstance,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Insert_PaymentGatewayPaymentMethod. groupId: {0}, paymentGatewayId: {1}, name {2}", groupId, paymentGatewayId,
                    !string.IsNullOrEmpty(name) ? name : string.Empty, ex);
                response = null;
            }

            return response;
        }

        public static PaymentMethod Update_PaymentMethod(int paymentMethodId, string name, bool allowMultiInstance)
        {
            PaymentMethod response = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PaymentGatewayPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@payment_method_id", paymentMethodId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@allow_multi_instance", allowMultiInstance ? 1 : 0);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        response = new PaymentMethod()
                        {
                            ID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                            PaymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(row, "PAYMENT_GATEWAY_ID"),
                            Name = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                            AllowMultiInstance = ODBCWrapper.Utils.GetIntSafeVal(row, "ALLOW_MULTI_INSTANCE") == 1
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Update_PaymentGatewayPaymentMethod. paymentMethodId: {0}, name = {1}", paymentMethodId, !string.IsNullOrEmpty(name) ? name : string.Empty, ex);
            }

            return response;
        }

        public static bool Update_PaymentGatewayPaymentMethod(int paymentMethodId, string name, bool allowMultiInstance)
        {
            int rowCount = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PaymentGatewayPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@payment_method_id", paymentMethodId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@allow_multi_instance", allowMultiInstance ? 1 : 0);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Update_PaymentGatewayPaymentMethod. paymentMethodId: {0}, name = {1}", paymentMethodId, !string.IsNullOrEmpty(name) ? name : string.Empty, ex);
            }

            return rowCount > 0;
        }

        public static bool Delete_PaymentGatewayPaymentMethod(int paymentMethodId)
        {
            int rowCount = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Delete_PaymentGatewayPaymentMethod");
                sp.AddParameter("@payment_method_id", paymentMethodId);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Delete_PaymentGatewayPaymentMethod. paymentMethodId: {0}", paymentMethodId, ex);
            }

            return rowCount > 0;
        }

        public static List<PaymentMethod> Get_PaymentGatewayPaymentMethods(int groupId, int paymentGatewayId)
        {
            List<PaymentMethod> response = new List<PaymentMethod>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayPaymentMethods");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@payment_gateway_id", paymentGatewayId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        PaymentMethod method = null;
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            method = new PaymentMethod()
                            {
                                ID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                            PaymentGatewayId = paymentGatewayId,
                                Name = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                                AllowMultiInstance = ODBCWrapper.Utils.GetIntSafeVal(row, "ALLOW_MULTI_INSTANCE") == 1
                            };

                            response.Add(method);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Get_PaymentGatewayPaymentMethods. groupId: {0}, paymentGatewayId: {1}", groupId, paymentGatewayId, ex);
                response = null;
            }

            return response;
        }

        public static bool UpdatePaymentGatewayTransaction(int groupId, int paymentGatewayId, string externalTransactionId, string paymentDetails, string paymentMethod, int paymentMethodId)
        {
            int rowCount = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_PaymentGatewayTransaction");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@externalTransactionId", externalTransactionId);
                sp.AddParameter("@paymentDetails", paymentDetails);
                sp.AddParameter("@paymentMethod", paymentMethod);
                sp.AddParameter("@paymentMethodId", paymentMethodId);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at Update_PaymentGatewayTransaction. groupId: {0}, paymentGatewayId: {1}, paymentMethodId: {2}",
                    groupId, paymentGatewayId, paymentMethodId, ex);
            }

            return rowCount > 0;

        }

        public static DataSet GetAllPaymentGatewaysWithPaymentMethods(int groupId, long householdId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_AllPaymentGatewaysWithPaymentMethods");
            sp.SetConnectionKey("BILLING_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupId);
            sp.AddParameter("@houseHoldID", householdId);

            DataSet ds = sp.ExecuteDataSet();
            return ds;
        }

        public static HouseholdPaymentMethod AddPaymentGatewayHouseholdPaymentMethod(int groupID, int householdId, int paymentGatewayId, int paymentMethodId, string paymentMethodExternalId, string paymentDetails)
        {
            HouseholdPaymentMethod householdPatmentMethod = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("AddPaymentGatewayHouseholdPaymentMethod");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentMethodId", paymentMethodId);
                sp.AddParameter("@paymentMethodExternalId", paymentMethodExternalId);
                sp.AddParameter("@paymentDetails", paymentDetails);
                
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = ds.Tables[0].Rows[0];
                        householdPatmentMethod = new HouseholdPaymentMethod()
                        {
                            ID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                            ExternalId = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                            Details = ODBCWrapper.Utils.GetSafeStr(row, "PAYMENT_DETAILS"),
                            PaymentGatewayId = paymentGatewayId,
                            PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(row, "PAYMENT_METHOD_ID"),
                            Selected = false
                        };
                    }
                }


            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed calling to AddPaymentGatewayHouseholdPaymentMethod. GID:{0},PGID:{1}. Household:{2}. Exception: {3}", groupID, paymentGatewayId, householdId, ex);
            }
            return householdPatmentMethod;
        }

        public static PaymentGatewayHouseholdPaymentMethod GetPaymentGatewayHouseholdByPaymentGatewayId(int groupID, int paymentGatewayId, int householdId,
            int paymentMethodId, string paymentMethodExternalId)
        {
            PaymentGatewayHouseholdPaymentMethod pghhpm = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PaymentGatewayHouseholdByPaymentGatewayId");
                sp.SetConnectionKey("BILLING_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@paymentGatewayId", paymentGatewayId);
                sp.AddParameter("@householdId", householdId);
                sp.AddParameter("@paymentMethodId", paymentMethodId);
                sp.AddParameter("@paymentMethodExternalId", paymentMethodExternalId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    pghhpm = new PaymentGatewayHouseholdPaymentMethod();

                    pghhpm.PaymentGatewayId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "PAYMENT_GATEWAY_ID");
                    pghhpm.PaymentMethodId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "PAYMENT_METHOD_ID");
                    pghhpm.HouseholdId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "HOUSEHOLD_ID");
                    pghhpm.PaymentMethodExternalId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_EXTERNAL_ID");
                    pghhpm.PaymentDetails = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "PAYMENT_METHOD_DETAILS");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetPaymentGatewayHouseholdByPaymentGatewayId household: {0}, payment gateway: {1}, error {2}", householdId, paymentGatewayId, ex);
            }

            return pghhpm;
        }
    }
}