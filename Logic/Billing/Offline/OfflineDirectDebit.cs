using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects.Billing;

namespace Core.Billing
{
    /// <summary>
    /// Direct debit for offline billing - vodafone/ono
    /// </summary>
    public class OfflineDirectDebit : BaseDirectDebit
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Ctor

        /// <summary>
        /// Basic initialization
        /// </summary>
        /// <param name="p_nGroupID"></param>
        public OfflineDirectDebit(int p_nGroupID)
            : base(p_nGroupID)
        {

        }

        #endregion

        #region Implement BaseDirectDebit

        /// <summary>
        /// Charges a user in an offline way: Inserts records to offline_transaction and billing_transactions
        /// </summary>
        /// <param name="p_sSiteGUID"></param>
        /// <param name="p_dChargePrice"></param>
        /// <param name="p_sCurrencyCode"></param>
        /// <param name="p_sUserIP"></param>
        /// <param name="p_sCustomData"></param>
        /// <param name="p_nPaymentNumber"></param>
        /// <param name="p_nNumberOfPayments"></param>
        /// <param name="p_sExtraParameters"></param>
        /// <param name="p_nBillingMethod"></param>
        /// <returns></returns>
        public override BillingResponse ChargeUser(string p_sSiteGUID, double p_dChargePrice, string p_sCurrencyCode, string p_sUserIP,
            string p_sCustomData, int p_nPaymentNumber, int p_nNumberOfPayments, string p_sExtraParameters, int p_nBillingMethod)
        {
            BillingResponse oBillingResponse = new BillingResponse();

            // By default, start as unkown until method clears it up
            oBillingResponse.m_oStatus = BillingResponseStatus.UnKnown;
            oBillingResponse.m_sStatusDescription = "Unkown";

            try
            {
                // Check if user exists and respond accordingly
                if (!Utils.IsUserExist(p_sSiteGUID, this.m_nGroupID))
                {
                    oBillingResponse.m_oStatus = BillingResponseStatus.UnKnownUser;
                    oBillingResponse.m_sRecieptCode = string.Empty;
                    oBillingResponse.m_sStatusDescription = "Unknown or active user";
                }
                else
                {
                    long lSiteGuid;

                    if (long.TryParse(p_sSiteGUID, out lSiteGuid))
                    {
                        long lOfflineTransactionID = 0;

                        try
                        {
                            // Insert new offline transaction record
                            lOfflineTransactionID =
                                DAL.BillingDAL.Insert_NewOfflineTransaction(lSiteGuid, p_dChargePrice, p_sCurrencyCode, this.m_nGroupID, p_sCustomData, null);
                        }
                        catch (Exception ex)
                        {
                            oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                            oBillingResponse.m_sRecieptCode = string.Empty;
                            oBillingResponse.m_sStatusDescription = "Insert transaction failed";
                            log.Error("Excpetion - Exception on renewal for user " + p_sSiteGUID + " ex: " + ex.Message, ex);
                        }

                        // If insert was successful
                        if (lOfflineTransactionID != 0)
                        {
                            long lBillingTransactionID =
                                InsertBillingTransaction(p_sSiteGUID, p_sCustomData, p_nPaymentNumber, ref p_nNumberOfPayments, lOfflineTransactionID);

                            // If insert was succesful
                            if (lBillingTransactionID != 0)
                            {
                                oBillingResponse.m_oStatus = BillingResponseStatus.Success;
                                oBillingResponse.m_sRecieptCode = lBillingTransactionID.ToString();
                                oBillingResponse.m_sStatusDescription = "OK";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oBillingResponse.m_oStatus = BillingResponseStatus.Fail;
                oBillingResponse.m_sStatusDescription = "Failed charging renewal of user subscription";
                log.Error("Exception - Exception on renewal for user " + p_sSiteGUID + " ex: " + ex.Message, ex);
            }

            return oBillingResponse;
        }

        /// <summary>
        /// Not implemented for now
        /// </summary>
        /// <param name="sPSPReference"></param>
        /// <param name="sSiteGuid"></param>
        /// <param name="nGroupID"></param>
        /// <param name="dChargePrice"></param>
        /// <param name="sCurrencyCode"></param>
        /// <param name="lPurchaseID"></param>
        /// <param name="nType"></param>
        /// <param name="nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne"></param>
        /// <returns></returns>
        public override bool RefundPayment(string sPSPReference, string sSiteGuid, int nGroupID, double dChargePrice, string sCurrencyCode,
            long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            return (false);
        }

        /// <summary>
        /// Not implemented for now
        /// </summary>
        /// <param name="sPSPReference"></param>
        /// <param name="sMerchantAccount"></param>
        /// <param name="sSiteGuid"></param>
        /// <param name="nGroupID"></param>
        /// <param name="lPurchaseID"></param>
        /// <param name="nType"></param>
        /// <param name="nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne"></param>
        /// <param name="dChargePrice"></param>
        /// <param name="sCurrencyCode"></param>
        /// <returns></returns>
        public override bool CancelPayment(string sPSPReference, string sMerchantAccount, string sSiteGuid, int nGroupID, long lPurchaseID,
            int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne, double? dChargePrice, string sCurrencyCode)
        {
            return (false);
        }

        /// <summary>
        /// Not implemented for now
        /// </summary>
        /// <param name="sPSPReference"></param>
        /// <param name="sSiteGuid"></param>
        /// <param name="dPrice"></param>
        /// <param name="sCurrencyCode"></param>
        /// <param name="lPurchaseID"></param>
        /// <param name="nType"></param>
        /// <param name="bIsCancelOrRefundResultOfPreviewModule"></param>
        /// <param name="nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne"></param>
        /// <returns></returns>
        public override bool CancelOrRefundPayment(string sPSPReference, string sSiteGuid, double? dPrice, string sCurrencyCode, long lPurchaseID,
            int nType, bool bIsCancelOrRefundResultOfPreviewModule, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            return (false);
        }

        #endregion

        #region Protected and private methods

        /// <summary>
        /// Insert billing transaction
        /// </summary>
        /// <param name="p_sSiteGUID"></param>
        /// <param name="p_sCustomData"></param>
        /// <param name="p_nPaymentNumber"></param>
        /// <param name="p_nNumberOfPayments"></param>
        /// <param name="lOfflineTransactionLocalID"></param>
        /// <returns></returns>
        protected long InsertBillingTransaction(string p_sSiteGUID, string p_sCustomData, int p_nPaymentNumber,
            ref int p_nNumberOfPayments, long lOfflineTransactionLocalID)
        {
            int nBillingProvider = (int)eBillingProvider.Offline;
            int nBillingProcessor = 4;
            // TODO s: What is the billing method?
            int nBillingMethod = 50;

            #region Ref variables definition

            // initialize variables for split reference and insert billing transaction
            int nMediaFileID = 0;
            int nMediaID = 0;
            string sSubscriptionCode = string.Empty;
            string sPrePaidCode = string.Empty;
            string sPriceCode = string.Empty;
            string sPPVModuleCode = string.Empty;
            bool bIsRecurring = false;
            string sCurrencyCode = string.Empty;
            double dChargePrice = 0.0;
            string sRelevantPrePaid = string.Empty;
            string sCountryCd = string.Empty;
            string sLanguageCode = string.Empty;
            string sDeviceName = string.Empty;
            string sPreviewModuleID = string.Empty;
            string sCollectionCode = string.Empty;

            // initialize variables only for split reference
            string sUserGUID = string.Empty;
            string sRelevantSub = string.Empty;
            string sPPVCode = string.Empty;
            int nMaxNumberOfUses = 0;
            int nMaxUsageModuleLifeCycle = 0;
            int nViewLifeCycleSecs = 0;
            string sPurchaseType = string.Empty;

            #endregion

            Utils.SplitRefference(p_sCustomData, ref nMediaFileID, ref nMediaID,
                ref sSubscriptionCode, ref sPPVCode, ref sRelevantPrePaid, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref p_nNumberOfPayments,
                    ref sUserGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle,
                    ref nViewLifeCycleSecs, ref sPurchaseType, ref sCountryCd, ref sLanguageCode, ref sDeviceName,
                    ref sPreviewModuleID, ref sCollectionCode);

            long lBillingTransactionID = Utils.InsertBillingTransaction(p_sSiteGUID,
                // no last four digits
                string.Empty,
                dChargePrice, sPriceCode, sCurrencyCode, p_sCustomData,
                // status is success, for now (?)
                (int)BillingResponseStatus.Success,
                string.Empty, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode, sSubscriptionCode,
                // no cellphone
                string.Empty, this.m_nGroupID, nBillingProvider, (int)lOfflineTransactionLocalID,
                // no payment method addition
                0.0, dChargePrice, p_nPaymentNumber, p_nNumberOfPayments,
                // no extra params
                string.Empty,
                sCountryCd, sLanguageCode, sDeviceName, nBillingProcessor, nBillingMethod, sRelevantPrePaid, sPreviewModuleID, sCollectionCode);

            return (lBillingTransactionID);
        }

        #endregion
    }
}
