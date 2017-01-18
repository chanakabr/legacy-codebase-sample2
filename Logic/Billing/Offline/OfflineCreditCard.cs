using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueueWrapper;
using ApiObjects.MediaIndexingObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    /// <summary>
    /// Credit card class for offline billing - vodafone/ono
    /// </summary>
    public class OfflineCreditCard : BaseCreditCard
    {
        #region Ctor

        /// <summary>
        /// Basic initialization
        /// </summary>
        /// <param name="p_nGroupID"></param>
        public OfflineCreditCard(int p_nGroupID)
            : base(p_nGroupID)
        {

        } 

        #endregion

        #region Implement BaseCreditCard

        /// <summary>
        /// Charges user in an offline way: Not charging him at the moment, 
        /// instead publish a notification to the queue and later on it will be picked and a charge will occur
        /// </summary>
        /// <param name="p_sSiteGUID"></param>
        /// <param name="p_dChargePrice"></param>
        /// <param name="p_sCurrencyCode"></param>
        /// <param name="p_sUserIP"></param>
        /// <param name="p_sCustomData"></param>
        /// <param name="p_nPaymentNumber"></param>
        /// <param name="p_nNumberOfPayments"></param>
        /// <param name="p_sExtraParameters"></param>
        /// <returns></returns>
        public override BillingResponse ChargeUser(string p_sSiteGUID, double p_dChargePrice, 
            string p_sCurrencyCode, string p_sUserIP, string p_sCustomData, int p_nPaymentNumber, int p_nNumberOfPayments, string p_sExtraParameters)
        {
            BillingResponse oResponse = new BillingResponse();
            
            // By default, start as unkown until method clears it up
            oResponse.m_oStatus = BillingResponseStatus.UnKnown;
            oResponse.m_sStatusDescription = "Unkown";

            // Check if user exists and respond accordingly
            if (!Utils.IsUserExist(p_sSiteGUID, this.m_nGroupID))
            {
                oResponse.m_oStatus = BillingResponseStatus.UnKnownUser;
                oResponse.m_sRecieptCode = string.Empty;
                oResponse.m_sStatusDescription = "Unknown or active user";
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
                    catch (Exception)
                    {
                        oResponse.m_oStatus = BillingResponseStatus.Fail;
                        oResponse.m_sRecieptCode = string.Empty;
                        oResponse.m_sStatusDescription = "Insert transaction failed";
                    }

                    // If insert was successful
                    if (lOfflineTransactionID != 0)
                    {
                        long lBillingTransactionID = 
                            InsertBillingTransaction(p_sSiteGUID, p_sCustomData, p_nPaymentNumber, ref p_nNumberOfPayments, lOfflineTransactionID);

                        // If insert was succesful
                        if (lBillingTransactionID != 0)
                        {
                            oResponse.m_oStatus = BillingResponseStatus.Success;
                            // TODO: What should be the reciept code: billing or offline?
                            oResponse.m_sRecieptCode = lBillingTransactionID.ToString();
                            oResponse.m_sStatusDescription = "OK";
                        }
                    }
                }
            }

            return (oResponse);
        }

        /// <summary>
        /// Function not needed in offline billing
        /// </summary>
        /// <param name="sUserIP"></param>
        /// <param name="sRandom"></param>
        /// <returns></returns>
        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            return (string.Empty);
        }

        /// <summary>
        /// Function not needed in offline billing
        /// </summary>
        /// <param name="sParams"></param>
        /// <returns></returns>
        public override string GetClientMerchantSig(string sParams)
        {
            return (string.Empty);
        }

        public override bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID)
        {
            return true;
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
            
            #region Ref varaibles definition

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
