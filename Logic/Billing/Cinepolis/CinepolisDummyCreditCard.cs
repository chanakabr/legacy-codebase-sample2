using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects.Billing;

namespace Core.Billing
{
    /*
    * This module is called only by Cinepolis's MPP renewer flow.
    * In all other flows (web purchase, one click) this class is never used.
    * 
    * 
    */
    public class CinepolisDummyCreditCard : BaseCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public CinepolisDummyCreditCard(int nGroupID) : base(nGroupID) { }

        private const string CINEPOLIS_DUMMY_CC_LOG_FILE_NAME = "CinepolisDummyCC";

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse res = new BillingResponse();
            try
            {
                log.Debug("ChargeUser - " + string.Format("Entering CinepolisDummyCreditCard ChargeUser try block. Site Guid: {0} , User IP: {1} , Custom data: {2} , ExtraParams: {3}", sSiteGUID, sUserIP, sCustomData, sExtraParameters));
                if (!Utils.IsUserExist(sSiteGUID, m_nGroupID))
                {
                    // user does not exist
                    res.m_oStatus = BillingResponseStatus.UnKnownUser;
                    res.m_sRecieptCode = string.Empty;
                    res.m_sStatusDescription = "Unknown or inactive user";
                }
                else
                {
                    // user exists
                    bool isTrueDummy = sExtraParameters != null && sExtraParameters.Equals(CinepolisUtils.CINEPOLIS_DUMMY);
                    if (!isTrueDummy)
                    {
                        // renewal
                        long lCustomDataID = Utils.AddCustomData(sCustomData);
                        long lSiteGuid = Int64.Parse(sSiteGUID); // in Utils.IsUserExist we checked that SiteGuid is parsable
                        Dictionary<string, string> oCustomDataDict = Utils.GetCustomDataDictionary(sCustomData);
                        int nBillingProvider = (int)eBillingProvider.Cinepolis;
                        ItemType it = Utils.CinepolisConvertToItemType(oCustomDataDict[Constants.BUSINESS_MODULE_TYPE]);
                        int nType = (int)it;
                        long lCinepolisTransactionID = 0;
                        long lBillingTransactionID = Utils.InsertNewCinepolisTransaction(m_nGroupID, lSiteGuid, dChargePrice,
                            sCurrencyCode, lCustomDataID, sCustomData, oCustomDataDict, string.Empty, (byte)CinepolisTransactionStatus.Accepted,
                            nPaymentNumber, nNumberOfPayments, nBillingProvider, 1, nBillingProvider, nType, (byte)CinepolisConfirmationStatus.NotSentYet,
                            string.Empty, ref lCinepolisTransactionID, false);
                        switch (lBillingTransactionID)
                        {
                            case -2:
                                res.m_sStatusDescription = "Failed to write to cinepolis transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            case -1:
                                res.m_sStatusDescription = "Double cinepolis transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            case 0:
                                res.m_sStatusDescription = "Failed to write to billing transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            default:
                                {
                                    res.m_oStatus = BillingResponseStatus.Success;
                                    res.m_sRecieptCode = lBillingTransactionID + "";
                                    break;
                                }
                        }
                        if (res.m_oStatus != BillingResponseStatus.Success)
                        {
                            // log here fail.
                            #region Logging
                            Utils.TryWriteToUserLog(m_nGroupID, sSiteGUID, string.Format("Error desc: {0} , Custom data ID: {1} , IsTrueDummy: {2}", res.m_sStatusDescription, lCustomDataID, isTrueDummy));
                            log.Debug("ChargeUser - " + string.Format("Billing response not success. Error msg: {0} , Site Guid: {1} , Custom data: {2} , IsTrueDummy: {3}", res.m_sStatusDescription, sSiteGUID, sCustomData, isTrueDummy));
                            #endregion
                        }
                        else
                        {
                            // success. send renewal mail.
                            CinepolisUtils.SendMail(it, oCustomDataDict, dChargePrice, m_nGroupID, CinepolisMailType.Purchase, lBillingTransactionID);
                        }
                    }
                    else
                    {
                        // true dummy
                        long lCustomDataID = Utils.AddCustomData(sCustomData);
                        long lSiteGuid = Int64.Parse(sSiteGUID); // in Utils.IsUserExist we checked that SiteGuid is parsable
                        Dictionary<string, string> oCustomDataDict = Utils.GetCustomDataDictionary(sCustomData);
                        int nBillingProvider = (int)eBillingProvider.Cinepolis;
                        ItemType it = Utils.CinepolisConvertToItemType(oCustomDataDict[Constants.BUSINESS_MODULE_TYPE]);
                        int nType = (int)it;
                        long lCinepolisTransactionID = 0;
                        long lBillingTransactionID = Utils.InsertNewCinepolisTransaction(m_nGroupID, lSiteGuid, dChargePrice,
                            sCurrencyCode, lCustomDataID, sCustomData, oCustomDataDict, string.Empty, (byte)CinepolisTransactionStatus.Dummy,
                            nPaymentNumber, nNumberOfPayments, nBillingProvider, 1, nBillingProvider, nType, (byte)CinepolisConfirmationStatus.NotSentYet,
                            string.Empty, ref lCinepolisTransactionID, false);
                        switch (lBillingTransactionID)
                        {
                            case -2:
                                res.m_sStatusDescription = "Failed to write to cinepolis transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            case -1:
                                res.m_sStatusDescription = "Double cinepolis transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            case 0:
                                res.m_sStatusDescription = "Failed to write to billing transactions";
                                res.m_oStatus = BillingResponseStatus.Fail;
                                res.m_sRecieptCode = string.Empty;
                                break;
                            default:
                                {
                                    res.m_oStatus = BillingResponseStatus.Success;
                                    res.m_sRecieptCode = lBillingTransactionID + "";
                                    break;
                                }
                        }
                        if (res.m_oStatus != BillingResponseStatus.Success)
                        {
                            // log here fail.
                            #region Logging
                            Utils.TryWriteToUserLog(m_nGroupID, sSiteGUID, string.Format("Error desc: {0} , Custom data ID: {1} , IsTrueDummy: {2}", res.m_sStatusDescription, lCustomDataID, isTrueDummy));
                            log.Debug("ChargeUser - " + string.Format("Billing response not success. Error msg: {0} , Site Guid: {1} , Custom data: {2} , IsTrueDummy: {3}", res.m_sStatusDescription, sSiteGUID, sCustomData, isTrueDummy));
                            #endregion
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                res.m_oStatus = BillingResponseStatus.Fail;
                res.m_sStatusDescription = "Exception occurred.";
                res.m_sRecieptCode = string.Empty;
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Charge Price: ", dChargePrice));
                sb.Append(String.Concat(" Currency Code: ", sCurrencyCode));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Custom Data: ", sCustomData));
                sb.Append(String.Concat(" Payment Number: ", nPaymentNumber));
                sb.Append(String.Concat(" Number of Payments: ", nNumberOfPayments));
                sb.Append(String.Concat(" Extra Params: ", sExtraParameters));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("ChargeUser - " + sb.ToString(), ex);
                #endregion
            }

            return res;
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            return string.Empty;
        }

        public override string GetClientMerchantSig(string sParams)
        {
            return string.Empty;
        }

        public override bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID)
        {
            return true;
        }
    }
}
