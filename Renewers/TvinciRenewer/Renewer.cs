using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DAL;
using ConditionalAccess;
/// ******************************************************
///Tvinci L.T.D
///Auto Renewer Schedualed Tasks
///
///Ver Date : 13/03/2013
///Unit : Media Store 
/// ******************************************************
namespace TvinciRenewer
{
    /// <summary>
    /// Scheduled tasks Renewer Class renew subscription purchase expires in 
    /// the next 24 hours the number of times a failed renewal does not exceed the number of attempts allowed to 
    /// renew subscription purchase.
    /// </summary>
    public class Renewer : ScheduledTasks.BaseTask
    {
        #region members

        //create customer group id var
        protected Int32 m_nGroupID;
        //create billing providers delimited string param
        protected string m_sBillingProviders;
        //subscriptions Purchases according to delimited string "1012;1013;1014"
        protected string m_sPurchasesIDs;
        // fail count for group id
        protected int m_nFailCount = -1;

        #endregion

        #region ctr and override base class nethods

        /// <summary>
        /// class Constractor base BaseTask Class
        /// </summary>
        /// <param name="nTaskID">set Int32 Task ID</param>
        /// <param name="nIntervalInSec">set Int32 interval in secound</param>
        /// <param name="sParameters">set extra string parameters</param>
        public Renewer(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            m_nGroupID = 0;
            m_sPurchasesIDs = string.Empty;

            string[] seperator = { "||" };
            string[] splited = sParameters.Split(seperator, StringSplitOptions.None);

            if (splited.Length >= 2)
            {
                m_nGroupID = int.Parse(splited[0].ToString());
                m_sBillingProviders = splited[1].ToString();


                if (splited.Length == 3)
                {
                    m_sPurchasesIDs = splited[2];
                }
                m_nFailCount = GetGroupFAILCOUNT();
            }

        }
        /// <summary>
        /// Get Instance Scheduled Tasks renewer calss object.
        /// </summary>
        /// <param name="nTaskID">set Int32 Task ID</param>
        /// <param name="nIntervalInSec">set Int32 interval in secound</param>
        /// <param name="sParameters">set extra string parameters</param>
        /// <returns>return ScheduledTasks.BaseTask instance object</returns>
        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new Renewer(nTaskID, nIntervalInSec, sParameters);
        }
        /// <summary>
        /// Override Do the task inner methods, call DoTheJob and execute the job.
        /// </summary>
        /// <returns>return execute call DoTheJob status</returns>
        protected override bool DoTheTaskInner()
        {
            if (!string.IsNullOrEmpty(m_sPurchasesIDs))
            {
                return DoTheJobTest();
            }
            else
            {
                return DoTheJob();
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Execute Job Task
        /// </summary>
        /// <returns></returns>
        public bool DoTheJob()
        {
            //Start Job write to log
            StringBuilder strlog = new StringBuilder();
            List<long> subscriptionsPurchasesList = null;
            List<int> billingProvidersList = null;
            List<long> totalsubscriptionsPurchasesList = new List<long>();
            RenewalList rl = null;
            bool bRunning = true;


            strlog.Append(string.Format("Job start :"));
            strlog.Append(string.Format(" | group: {0}", m_nGroupID.ToString()));
            strlog.Append(string.Format(" | billing providers: {0}.", m_sBillingProviders));
            Logger.Logger.Log("Tvinci multi usage module renewal : ******* Start ******** ", strlog.ToString(), "TvinciRenewer");

            billingProvidersList = GetBillingProvidersListFromDelimitedString(m_sBillingProviders);

            while (bRunning)
            {
                rl = GetRenewalList();
                subscriptionsPurchasesList = rl.GetMPPPurchasesList();

                bRunning = (subscriptionsPurchasesList != null && subscriptionsPurchasesList.Count > 0);

                if (bRunning)
                {
                    totalsubscriptionsPurchasesList.AddRange(subscriptionsPurchasesList);

                    DataTable dtTransactions = ApiDAL.Get_LastBillingTransactions(m_nGroupID, subscriptionsPurchasesList, billingProvidersList);

                    if (dtTransactions != null)
                    {
                        int nCount = dtTransactions.Rows.Count;
                        for (int i = 0; i < nCount; i++)
                        {
                            int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["group_id"]);
                            int nBillingMethod = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["BILLING_METHOD"]);
                            string sSiteGUID = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["SITE_GUID"]);
                            double dPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dtTransactions.Rows[i]["PRICE"]);
                            string sCurrency = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["CURRENCY_CODE"]);
                            string sSubscriptionCode = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["SUBSCRIPTION_CODE"]);

                            if (!string.IsNullOrEmpty(sSubscriptionCode))
                            {
                                string sExtraParams = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["EXTRA_PARAMS"]);
                                int nPurchaseID = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["purchase_id"]);
                                int nPaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["PAYMENT_NUMBER"]);
                                int nNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["number_of_payments"]);
                                int nTotalNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["total_number_of_payments"]);
                                DateTime dtCurrentEndDate = rl.GetEndDateByPurchaseID(nPurchaseID);

                                bool bIsPurchasedWithPreviewModule = ApiDAL.Get_IsPurchasedWithPreviewModule(nGroupID, sSiteGUID, nPurchaseID);
                                int nTransactionBillingProvider = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["billing_provider"]);

                                if (nBillingMethod == 7) // gift. we need to grab last non gift billing method in order to charge the user.
                                {
                                    nBillingMethod = (int)ApiDAL.Get_LastNonGiftBillingMethod(sSiteGUID, nGroupID, nTransactionBillingProvider);
                                }

                                nPaymentNumber = CalcPaymentNumber(nNumOfPayments, nPaymentNumber, bIsPurchasedWithPreviewModule);

                                if (nNumOfPayments == 0 || nPaymentNumber <= nNumOfPayments)
                                {
                                    nPaymentNumber++;

                                    string sCountryCd = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["COUNTRY_CODE"]);
                                    string sDeviceName = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["DEVICE_NAME"]);
                                    string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["LANGUAGE_CODE"]);

                                    ConditionalAccess.BaseConditionalAccess t = null;
                                    ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");

                                    eBillingProvider billingProvider = (eBillingProvider)nTransactionBillingProvider;

                                    if ((billingProvider == eBillingProvider.Adyen) ||
                                        (billingProvider == eBillingProvider.Cinepolis) ||
                                        (billingProvider == eBillingProvider.M1) ||
                                        (billingProvider == eBillingProvider.Offline))
                                    {
                                        try
                                        {
                                            ConditionalAccess.TvinciBilling.BillingResponse resp = t.DD_BaseRenewMultiUsageSubscription(sSiteGUID, sSubscriptionCode, "1.1.1.1", sExtraParams,
                                            nPurchaseID, nBillingMethod, nPaymentNumber, nTotalNumOfPayments, sCountryCd, sLanguageCode, sDeviceName, nNumOfPayments, bIsPurchasedWithPreviewModule, dtCurrentEndDate, billingProvider);

                                            StringBuilder strLog = new StringBuilder();
                                            strLog.Append(string.Format("SiteGUID : {0}", sSiteGUID));
                                            strLog.Append(string.Format(" | price: {0}{1}", dPrice.ToString(), sCurrency));
                                            strLog.Append(string.Format(" | Subscription Code : {0}", sSubscriptionCode));
                                            strLog.Append(string.Format(" | Extra params : {0}", sExtraParams));
                                            strLog.Append(string.Format(" | Prchase ID : {0}", nPurchaseID));
                                            strLog.Append(string.Format(" | Payment number : {0}", nPaymentNumber));
                                            strLog.Append(string.Format(" | Billing response status: {0}", resp.m_oStatus.ToString()));
                                            strLog.Append(string.Format(" | Billing response description : {0}", resp.m_sStatusDescription));
                                            strLog.Append(string.Format(" | Billing response reciept: {0}.", resp.m_sRecieptCode));

                                            Logger.Logger.Log("Tvinci multi usage module renewal : DoTheJob ", strLog.ToString(), "TvinciRenewer");
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Logger.Log("Error (CA)", string.Format("u:{0}, s:{1}, pid:{2}, ex:{3}, st:{4}", sSiteGUID, sSubscriptionCode, nPurchaseID, ex.Message, ex.StackTrace), "TvinciRenewer");
                                        }
                                    }
                                }

                                System.Threading.Thread.Sleep(10);
                            }
                        }
                    }
                }
            }
            UpdateRecurringRunTime(totalsubscriptionsPurchasesList, 0);
            Logger.Logger.Log("Tvinci multi usage module renewal : ******* End ******** ", "Job End.", "TvinciRenewer");

            return true;
        }


        public bool DoTheJobTest()
        {
            //Start Job write to log
            StringBuilder strlog = new StringBuilder();
            List<long> subscriptionsPurchasesList = null;
            List<int> billingProvidersList = null;
            List<long> totalsubscriptionsPurchasesList = new List<long>();

            bool bRunning = true;


            strlog.Append(string.Format("Job start :"));
            strlog.Append(string.Format(" | group: {0}", m_nGroupID.ToString()));
            strlog.Append(string.Format(" | billing providers: {0}.", m_sBillingProviders));
            Logger.Logger.Log("Tvinci multi usage module renewal : ******* Start ******** ", strlog.ToString(), "TvinciRenewer");

            subscriptionsPurchasesList = GetSubscriptionsPurchasesListFromDelimitedString(m_sPurchasesIDs);
            billingProvidersList = GetBillingProvidersListFromDelimitedString(m_sBillingProviders);

            bRunning = (subscriptionsPurchasesList != null && subscriptionsPurchasesList.Count > 0);

            if (bRunning)
            {
                totalsubscriptionsPurchasesList.AddRange(subscriptionsPurchasesList);

                DataTable dtTransactions = ApiDAL.Get_LastBillingTransactions(m_nGroupID, subscriptionsPurchasesList, billingProvidersList);

                if (dtTransactions != null)
                {
                    int nCount = dtTransactions.Rows.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["group_id"]);
                        int nBillingMethod = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["BILLING_METHOD"]);
                        string sSiteGUID = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["SITE_GUID"]);
                        double dPrice = ODBCWrapper.Utils.GetDoubleSafeVal(dtTransactions.Rows[i]["PRICE"]);
                        string sCurrency = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["CURRENCY_CODE"]);
                        string sSubscriptionCode = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["SUBSCRIPTION_CODE"]);

                        if (!string.IsNullOrEmpty(sSubscriptionCode))
                        {
                            string sExtraParams = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["EXTRA_PARAMS"]);
                            int nPurchaseID = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["purchase_id"]);
                            int nPaymentNumber = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["PAYMENT_NUMBER"]);
                            int nNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["number_of_payments"]);
                            int nTotalNumOfPayments = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["total_number_of_payments"]);
                            DateTime dtCurrentEndDate = GetPurchaseEndDate(nPurchaseID);
                            if (dtCurrentEndDate == DateTime.MaxValue)
                            {
                                Logger.Logger.Log("Tvinci multi usage module renewal : DoTheJobTest ", "No end date found for purchase id:" + nPurchaseID.ToString(), "TvinciRenewer");
                                break; // No purchase end date found.
                            }

                            bool bIsPurchasedWithPreviewModule = ApiDAL.Get_IsPurchasedWithPreviewModule(nGroupID, sSiteGUID, nPurchaseID);
                            int nTransactionBillingProvider = ODBCWrapper.Utils.GetIntSafeVal(dtTransactions.Rows[i]["billing_provider"]);

                            if (nBillingMethod == 7) // gift. we need to grab last non gift billing method in order to charge the user.
                            {
                                nBillingMethod = (int)ApiDAL.Get_LastNonGiftBillingMethod(sSiteGUID, nGroupID, nTransactionBillingProvider);
                            }

                            #region Calculate the next payment number except subscriptions unlimited purchase renewal
                            nPaymentNumber = CalcPaymentNumber(nNumOfPayments, nPaymentNumber, bIsPurchasedWithPreviewModule);
                            #endregion

                            #region Renew subscription
                            if (nNumOfPayments == 0 || nPaymentNumber <= nNumOfPayments)
                            {
                                nPaymentNumber++;

                                string sCountryCd = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["COUNTRY_CODE"]);
                                string sDeviceName = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["DEVICE_NAME"]);
                                string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dtTransactions.Rows[i]["LANGUAGE_CODE"]);

                                ConditionalAccess.BaseConditionalAccess t = null;
                                ConditionalAccess.Utils.GetBaseConditionalAccessImpl(ref t, nGroupID, "CA_CONNECTION_STRING");

                                eBillingProvider eBillingProvider = (eBillingProvider)(nTransactionBillingProvider);

                                if ((eBillingProvider == ConditionalAccess.eBillingProvider.Adyen || 
                                    eBillingProvider == ConditionalAccess.eBillingProvider.Cinepolis || 
                                    eBillingProvider == ConditionalAccess.eBillingProvider.M1 ||
                                    eBillingProvider == ConditionalAccess.eBillingProvider.Offline))
                                {
                                    try
                                    {
                                        Logger.Logger.Log("MPP Renewal : DoTheJobTest ", "purchase_id:" + nPurchaseID, "TvinciRenewer");

                                        ConditionalAccess.TvinciBilling.BillingResponse oBillingResponse = t.DD_BaseRenewMultiUsageSubscription(sSiteGUID, sSubscriptionCode, "1.1.1.1", sExtraParams,
                                            nPurchaseID, nBillingMethod, nPaymentNumber, nTotalNumOfPayments, sCountryCd, sLanguageCode, sDeviceName, nNumOfPayments, bIsPurchasedWithPreviewModule, dtCurrentEndDate, eBillingProvider);

                                        StringBuilder strLog = new StringBuilder();
                                        strLog.Append(string.Format("SiteGUID : {0}", sSiteGUID));
                                        strLog.Append(string.Format(" | price: {0}{1}", dPrice.ToString(), sCurrency));
                                        strLog.Append(string.Format(" | Subscription Code : {0}", sSubscriptionCode));
                                        strLog.Append(string.Format(" | Extra params : {0}", sExtraParams));
                                        strLog.Append(string.Format(" | Prchase ID : {0}", nPurchaseID));
                                        strLog.Append(string.Format(" | Payment number : {0}", nPaymentNumber));
                                        strLog.Append(string.Format(" | Billing response status: {0}", oBillingResponse.m_oStatus.ToString()));
                                        strLog.Append(string.Format(" | Billing response description : {0}", oBillingResponse.m_sStatusDescription));
                                        strLog.Append(string.Format(" | Billing response reciept: {0}.", oBillingResponse.m_sRecieptCode));

                                        Logger.Logger.Log("Tvinci multi usage module renewal : DoTheJobTest ", strLog.ToString(), "TvinciRenewer");

                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Logger.Log("Error (CA)", string.Format("u:{0}, s:{1}, pid:{2}, ex:{3}, st:{4}", sSiteGUID, sSubscriptionCode, nPurchaseID, ex.Message, ex.StackTrace), "TvinciRenewer");
                                    }
                                }
                            }
                            #endregion

                            System.Threading.Thread.Sleep(10);
                        }
                    }
                }
            }

            UpdateRecurringRunTime(totalsubscriptionsPurchasesList, 0);
            Logger.Logger.Log("Tvinci multi usage module renewal : ******* End ******** ", "Job End.", "TvinciRenewer");

            return true;
        }

        protected RenewalList GetRenewalList()
        {

            DataTable dt = ConditionalAccessDAL.Get_MPPsToRenew(DateTime.UtcNow.AddHours(24), m_nGroupID, m_nFailCount, "CA_CONNECTION_STRING");
            RenewalList res = new RenewalList(dt);
            List<long> retList = res.GetMPPPurchasesList();
            //update subscription purchase runtime recurring status as processing.
            if (retList != null && retList.Count > 0)
            {
                string sList = string.Join(",", retList.Select(x => x.ToString()).ToArray());
                string strLog = string.Format("subscriptions purchases ID's ({0}) expire in the next 24 hours", sList);
                Logger.Logger.Log("Tvinci multi usage module renewal : GetList ", strLog, "TvinciRenewer");
                UpdateRecurringRunTime(retList, 1);
            }
            return res;
        }


        /// <summary>
        /// Get subscription purchases list according to delimited string
        /// </summary>
        /// <param name="sDelimitedPurchasesIDs"></param>
        /// <returns></returns>
        protected List<long> GetSubscriptionsPurchasesListFromDelimitedString(string sDelimitedPurchasesIDs)
        {
            List<long> retList = new List<long>();

            if (!string.IsNullOrEmpty(m_sPurchasesIDs))
            {
                char[] splitter = new char[] { ';' };
                string[] purchasesIDsArray = sDelimitedPurchasesIDs.Split(splitter);

                if (purchasesIDsArray != null && purchasesIDsArray.Length > 0)
                {
                    for (int i = 0; i < purchasesIDsArray.Length; i++)
                    {
                        long nPurchaseID = 0;
                        bool result = long.TryParse(purchasesIDsArray[i], out nPurchaseID);
                        if (result == true)
                        {
                            retList.Add(nPurchaseID);
                        }
                    }
                }
            }
            return retList;
        }

        /// <summary>
        /// Get billing providers list according to delimited string
        /// </summary>
        /// <param name="sDelimitedPurchasesIDs"></param>
        /// <returns></returns>
        protected List<int> GetBillingProvidersListFromDelimitedString(string sDelimitedBillingProvidersIDs)
        {
            List<int> retList = new List<int>();

            if (!string.IsNullOrEmpty(sDelimitedBillingProvidersIDs))
            {
                char[] splitter = new char[] { ';' };
                string[] billingProviderIDsArray = sDelimitedBillingProvidersIDs.Split(splitter);

                if (billingProviderIDsArray != null && billingProviderIDsArray.Length > 0)
                {
                    for (int i = 0; i < billingProviderIDsArray.Length; i++)
                    {
                        int nBillingProviderID = 0;
                        bool result = int.TryParse(billingProviderIDsArray[i], out nBillingProviderID);
                        if (result == true)
                        {
                            retList.Add(nBillingProviderID);
                        }
                    }
                }
            }
            return retList;
        }


        /// <summary>
        /// Get group fail count definition
        /// </summary>
        /// <returns>retun the max fail count to tray renew subscription</returns>
        private int GetGroupFAILCOUNT()
        {

            int res = ConditionalAccessDAL.Get_GroupFailCount(m_nGroupID, "CA_CONNECTION_STRING");
            return res > 0 ? res : ConditionalAccess.Utils.DEFAULT_MPP_RENEW_FAIL_COUNT;
        }

        /// <summary>
        /// Update subscription purchase runtime recurring status
        /// </summary>
        /// <param name="subscriptionList">set subscription purchase ID sperate comma, Example : 192, 300, 102 ...etc</param>
        /// <param name="RunttimeStatus">set 1 will subscription purchase during renewal process, set 0 will the run time recurring process stopped.</param>
        /// <returns>return execute update status</returns>
        private bool UpdateRecurringRunTime(List<long> nSubscriptionPurchaseIDs, int RunttimeStatus)
        {
            bool res = false;
            if (nSubscriptionPurchaseIDs != null && nSubscriptionPurchaseIDs.Count > 0)
            {
                string sSubscriptionPurchaseIDs = string.Join(",", nSubscriptionPurchaseIDs.Select(x => x.ToString()).ToArray());
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("subscriptions_purchases");
                updateQuery.SetConnectionKey("CA_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RECURRING_RUNTIME_STATUS", "=", RunttimeStatus);
                updateQuery += " where IS_ACTIVE=1 and STATUS=1 and";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                if (!string.IsNullOrEmpty(sSubscriptionPurchaseIDs))
                {
                    updateQuery += " and id in (" + sSubscriptionPurchaseIDs + ")";
                }
                res = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                string strLog = string.Format("update subscriptions purchases ID's ({0}) runtime recurring proccess status to '{1}'.", sSubscriptionPurchaseIDs, RunttimeStatus);
                Logger.Logger.Log("Tvinci multi usage module renewal : UpdateRecurringRunTime ", strLog, "TvinciRenewer");
            }
            return res;
        }

        private int CalcPaymentNumber(int nNumOfPayments, int nPaymentNumber, bool bIsPurchasedWithPreviewModule)
        {
            int res = nPaymentNumber;
            if (nPaymentNumber == 0 && bIsPurchasedWithPreviewModule)
                res = 0;
            else
            {
                if (nNumOfPayments != 0)
                {
                    res = nPaymentNumber % nNumOfPayments;
                    if (res == 0)
                        res = nNumOfPayments;
                }
            }
            return res;
        }

        private DateTime GetPurchaseEndDate(int nPurchaseId)
        {
            DateTime result = DateTime.MaxValue;
            object objEndDate = ODBCWrapper.Utils.GetTableSingleVal("subscriptions_purchases", "end_date", nPurchaseId, "CA_CONNECTION_STRING");

            if (objEndDate != null)
            {
                result = ODBCWrapper.Utils.GetDateSafeVal(objEndDate);
            }
            return result;
        }

        #endregion
    }
}


