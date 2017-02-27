using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using ApiObjects.Epg;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Core.ConditionalAccess.Response;
using ApiObjects.Billing;
using AdapterControllers;
using Core.Pricing;
using Core.Billing;
using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess.Modules;


namespace Core.ConditionalAccess
{
    public class TvinciConditionalAccess : BaseConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public TvinciConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        internal override string GetLicensedLink(int streamingCompany, Dictionary<string, string> dParams)
        {
            string response = null;

            CDNTokenizers.Tokenizers.ICDNTokenizer tokenizer = CDNTokenizers.CDNTokenizerFactory.GetTokenizerInstance(m_nGroupID, streamingCompany);
            response = tokenizer == null ? string.Empty : tokenizer.GenerateToken(dParams);

            return response;
        }

        protected override bool GetUserCASubStatus(string sSiteGUID, ref UserCAStatus oUserCAStatus)
        {
            bool retVal = false;
            PermittedSubscriptionContainer[] subscriptionsItems = GetUserPermittedSubscriptions(sSiteGUID);
            if (subscriptionsItems != null)
            {
                Int32 nCurrentSubItems = subscriptionsItems.Length;
                if (nCurrentSubItems > 0)
                {
                    oUserCAStatus = UserCAStatus.CurrentSub;
                    retVal = true;
                }
            }
            return retVal;
        }

        protected override string GetErrorLicensedLink(string sBasicLink)
        {
            return string.Empty;
        }

        public override bool ActivateCampaign(int campaignID, CampaignActionInfo cai)
        {
            bool retVal = false;
            Campaign camp = Pricing.Module.GetCampaignData(m_nGroupID, campaignID);
            if (camp != null)
            {
                BaseCampaignActionImpl campImpl = Utils.GetCampaignActionByType(camp.m_CampaignResult);
                if (campImpl != null)
                {
                    retVal = campImpl.ActivateCampaign(camp, cai, m_nGroupID);
                }
            }
            return retVal;
        }

        public override CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai)
        {
            CampaignActionInfo retVal = null;
            Campaign camp = null;
            if (campaignID > 0)
            {
                camp = Pricing.Module.GetCampaignData(m_nGroupID, campaignID);
            }
            else
            {
                if (cai != null && !string.IsNullOrEmpty(cai.m_socialInviteInfo.m_hashCode))
                {
                    camp = Pricing.Module.GetCampaignsByHash(m_nGroupID, cai.m_socialInviteInfo.m_hashCode);
                }
            }
            if (camp != null)
            {
                BaseCampaignActionImpl campImpl = Utils.GetCampaignActionByTriggerType(camp.m_CampaignTrigger);

                if (campImpl != null)
                {
                    retVal = campImpl.ActivateCampaignWithInfo(camp, cai, m_nGroupID);
                }
            }
            return retVal;
        }

        protected override BillingResponse HandleBaseRenewMPPBillingCharge(string sSiteGuid, double dPrice, string sCurrency, string sUserIP,
            string sCustomData, int nPaymentNumber, int nRecPeriods, string sExtraParams, int nBillingMethod, long lPurchaseID, eBillingProvider eBillingProvider)
        {
            BillingResponse oResponse = null;

            try
            {
                if (dPrice != 0)
                {
                    switch (eBillingProvider)
                    {
                        case eBillingProvider.Adyen:
                            {
                                oResponse = Billing.Module.DD_ChargeUser(m_nGroupID, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, lPurchaseID.ToString(), nBillingMethod);
                                break;
                            }
                        case eBillingProvider.M1:
                            {
                                oResponse = Billing.Module.Cellular_ChargeUser(m_nGroupID, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, lPurchaseID.ToString());
                                break;
                            }
                        case eBillingProvider.Offline:
                            {
                                oResponse =
                                    Billing.Module.DD_ChargeUser(m_nGroupID, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber,
                                                                    nRecPeriods, lPurchaseID.ToString(), nBillingMethod);

                                break;
                            }
                    }
                }
                else
                {
                    sExtraParams = "AdyanDummy";
                    oResponse = Billing.Module.CC_DummyChargeUser(m_nGroupID, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                throw;
            }

            return oResponse;
        }

        /// <summary>
        /// Handle success of renewal. Returns true if this method worked fine, false otherwise.
        /// </summary>
        /// <param name="sSiteGUID"></param>
        /// <param name="sSubscriptionCode"></param>
        /// <param name="dtCurrentEndDate"></param>
        /// <param name="bIsPurchasedWithPreviewModule"></param>
        /// <param name="lPurchaseID"></param>
        /// <param name="sCurrency"></param>
        /// <param name="dPrice"></param>
        /// <param name="nPaymentNumber"></param>
        /// <param name="sBillingTransactionID"></param>
        /// <param name="nUsageModuleMaxVLC"></param>
        /// <param name="bIsMPPRecurringInfinitely"></param>
        /// <param name="nNumOfRecPeriods"></param>
        /// <returns></returns>
        protected override bool HandleMPPRenewalBillingSuccess(string sSiteGUID, string sSubscriptionCode, DateTime dtCurrentEndDate,
            bool bIsPurchasedWithPreviewModule, long lPurchaseID, string sCurrency, double dPrice, int nPaymentNumber,
            string sBillingTransactionID, int nUsageModuleMaxVLC, bool bIsMPPRecurringInfinitely, int nNumOfRecPeriods)
        {
            bool bSuccesful = true;
            WriteToUserLog(sSiteGUID, String.Concat("MPP Renewal: ", sSubscriptionCode, " renewed ", dPrice.ToString(), sCurrency));

            DateTime dtNextEndDate = Utils.GetEndDateTime(dtCurrentEndDate, nUsageModuleMaxVLC);

            if (IsLastPeriodOfLastUsageModule(bIsPurchasedWithPreviewModule, bIsMPPRecurringInfinitely, nNumOfRecPeriods, nPaymentNumber))
            {
                ConditionalAccessDAL.Update_MPPRenewalData(lPurchaseID, false, dtNextEndDate, 0, "CA_CONNECTION_STRING");
            }
            else
            {
                ConditionalAccessDAL.Update_MPPRenewalData(lPurchaseID, true, dtNextEndDate, 0, "CA_CONNECTION_STRING");
            }

            long lBillingTransactionID = 0;

            if (!string.IsNullOrEmpty(sBillingTransactionID) && long.TryParse(sBillingTransactionID, out lBillingTransactionID) && lBillingTransactionID > 0)
            {
                ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPurchaseID);
            }
            else
            {
                // invalid format for billing transaction id
                bSuccesful = false;

                #region Logging
                log.Debug("DD_BaseRenewMultiUsageSubscription - " + string.Format("HandleMPPRenewalBillingSuccess. Failed to update purchase id in billing_transactions. Purchase ID: {0} , Billing Transaction ID: {1} , Site Guid: {2} , BaseConditionalAccess is: {3}", lPurchaseID, sBillingTransactionID, sSiteGUID, this.GetType().ToString()));
                WriteToUserLog(sSiteGUID, string.Format("MPP Renewal. Failed to update purchase id in billing transactions. Purchase ID: {0} , Billing transaction id: {1}", lPurchaseID, lBillingTransactionID));
                #endregion
            }

            return bSuccesful;
        }

        protected internal override BillingResponse HandleCCChargeUser(string sSiteGuid,
            double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nNumOfPayments,
            string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, bool bIsDummy, bool bIsEntitledToPreviewModule)
        {
            if (!bIsDummy && !bIsEntitledToPreviewModule)
            {
                return Billing.Module.CC_ChargeUser(m_nGroupID, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, 1,
                    nNumOfPayments, sExtraParams, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                // if the user is entitled to preview module, and this function is called it means
                // we already have the user's cc details so we can dummy charge him
                // (this comment is correct for Adyen only. In Cinepolis there is no dummy charge. During development only MediaCorp and Cinepolis asked for preview module.)
                return Billing.Module.CC_DummyChargeUser(m_nGroupID, sSiteGuid,
                    bIsEntitledToPreviewModule ? 0.0 : dPrice,
                    sCurrency, sUserIP, sCustomData, 1, nNumOfPayments, sExtraParams);
            }
        }

        protected override bool UpdatePurchaseIDInBilling(long purchaseID, long billingRefTransactionID)
        {
            return Billing.Module.UpdatePurchaseIDInBilling(m_nGroupID, purchaseID, billingRefTransactionID);

        }

        protected override bool HandleChargeUserForSubscriptionBillingSuccess(string sSiteGUID, int domianID, Subscription theSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, BillingResponse br, bool bIsEntitledToPreviewModule, string sSubscriptionCode,
            string sCustomData, bool bIsRecurring, ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy)
        {
            bool res = true;
            HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP, sCountryCd, sLanguageCode, sDeviceName, true, 0, 0);

            long lPreviewModuleID = 0;
            if (theSub.m_oPreviewModule != null)
                lPreviewModuleID = theSub.m_oPreviewModule.m_nID;

            lBillingTransactionID = Utils.ParseLongIfNotEmpty(br.m_sRecieptCode);

            bool bUsageModuleExists = (theSub != null && theSub.m_oUsageModule != null);
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtSubEndDate = Utils.CalcSubscriptionEndDate(theSub, bIsEntitledToPreviewModule, dtUtcNow);

            lPurchaseID = ConditionalAccessDAL.Insert_NewMPPPurchase(m_nGroupID, sSubscriptionCode, sSiteGUID, bIsEntitledToPreviewModule ? 0.0 : dPrice, sCurrency, sCustomData, sCountryCd, sLanguageCode,
                sDeviceName, bUsageModuleExists ? theSub.m_oUsageModule.m_nMaxNumberOfViews : 0, bUsageModuleExists ? theSub.m_oUsageModule.m_tsViewLifeCycle : 0, bIsRecurring, lBillingTransactionID,
                lPreviewModuleID, dtUtcNow, dtSubEndDate, dtUtcNow, string.Empty, domianID);

            if (lPurchaseID > 0)
            {
                // writing to subscription_purchases succeeded
                WriteToUserLog(sSiteGUID, string.Format("Subscription purchased. ID in billing transaction {0} , ID in subscriptions_purchases {1}", lBillingTransactionID, lPurchaseID));
                if (lBillingTransactionID > 0)
                {
                    // update in billing_transactions
                    if (!ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPurchaseID))
                    {
                        // purchase id in billing transactions is critical for renewal process. log if fails.
                        #region Logging
                        StringBuilder sb = new StringBuilder("Failed to update purchase id in billing_transactions table");
                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                        sb.Append(String.Concat(" Purchase ID: ", lPurchaseID));
                        sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
                        sb.Append(String.Concat(" Subscription Code: ", sSubscriptionCode));
                        sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().Name));
                        sb.Append(String.Concat(" Custom Data: ", sCustomData));
                        log.Debug("HandleChargeUserForSubscriptionBillingSuccess - " + sb.ToString());
                        #endregion
                    }
                    else
                    {
                        UpdatePurchaseIDInExternalBillingTable(lPurchaseID, lBillingTransactionID);
                    }
                }
                else
                {
                    // no id in billing_transactions
                    res = false;
                    #region Logging
                    log.Debug("HandleChargeUserForSubscriptionBillingSuccess - " + string.Format("No billing_transactions ID. SiteGuid: {0} , Purchase ID: {1} , Sub Code: {2} , Coupon Code: {3}", sSiteGUID, lPurchaseID, sSubscriptionCode, sCouponCode));
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForSubscriptionBillingSuccess. Failed to update billing_transactions. Purchase ID: {0} , Sub Code: {1} , Coupon Code: {2}", lPurchaseID, sSubscriptionCode, sCouponCode));
                    #endregion
                }
            }
            else
            {
                // writing to subscription_purchases failed
                res = false;
                #region Logging
                log.Debug("HandleChargeUserForSubscriptionBillingSuccess - " + string.Format("No ID in subscriptions_purchases. Site Guid: {0} , Sub Code: {1} , Coupon Code: {2} , User IP: {3}", sSiteGUID, sSubscriptionCode, sCouponCode, sUserIP));
                WriteToUserLog(sSiteGUID, string.Format("Failed to write to subscription_purchases. Sub Code: {0} , Coupon Code: {1}", sSubscriptionCode, sCouponCode));
                #endregion
            }

            return res;
        }

        protected override bool HandleChargeUserForCollectionBillingSuccess(string sSiteGUID, int domianID, Collection theCol,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, BillingResponse br, string sCollectionCode,
            string sCustomData, ref long lBillingTransactionID, ref long lPurchaseID)
        {
            bool res = true;
            Int32 nColCode;
            Int32.TryParse(sCollectionCode, out nColCode);
            HandleCouponUses(null, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP,
                sCountryCd, sLanguageCode, sDeviceName, true, 0, nColCode);

            lBillingTransactionID = Utils.ParseLongIfNotEmpty(br.m_sRecieptCode);

            bool bUsageModuleExists = (theCol != null && theCol.m_oUsageModule != null);
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtSubEndDate = CalcCollectionEndDate(theCol, dtUtcNow);

            lPurchaseID = ConditionalAccessDAL.Insert_NewMColPurchase(m_nGroupID, sCollectionCode, sSiteGUID, dPrice, sCurrency, sCustomData, sCountryCd, sLanguageCode, sDeviceName,
                bUsageModuleExists ? theCol.m_oUsageModule.m_nMaxNumberOfViews : 0, bUsageModuleExists ? theCol.m_oUsageModule.m_tsViewLifeCycle : 0, lBillingTransactionID,
                dtUtcNow, dtSubEndDate, dtUtcNow, string.Empty, domianID);

            if (lPurchaseID > 0)
            {
                // writing to collection_purchases succeeded
                WriteToUserLog(sSiteGUID, string.Format("Collection purchased. ID in billing transaction {0} , ID in collection_purchases {1}", lBillingTransactionID, lPurchaseID));
                if (lBillingTransactionID > 0)
                {
                    // update in billing_transactions
                    if (!ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPurchaseID))
                    {
                        // purchase id in billing transactions is critical for renewal process. log if fails.
                        #region Logging
                        StringBuilder sb = new StringBuilder("Failed to update purchase id in billing_transactions table");
                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                        sb.Append(String.Concat(" Purchase ID: ", lPurchaseID));
                        sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
                        sb.Append(String.Concat(" Collection Code: ", sCollectionCode));
                        sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().Name));
                        sb.Append(String.Concat(" Custom Data: ", sCustomData));
                        log.Debug("HandleChargeUserForCollectionBillingSuccess - " + sb.ToString());
                        #endregion
                    }
                    else
                    {
                        UpdatePurchaseIDInExternalBillingTable(lBillingTransactionID, lPurchaseID);
                    }
                }
                else
                {
                    // no id in billing_transactions
                    res = false;
                    #region Logging
                    log.Debug("HandleChargeUserForCollectionBillingSuccess - " + string.Format("No billing_transactions ID. SiteGuid: {0} , Purchase ID: {1} , Col Code: {2} , Coupon Code: {3}", sSiteGUID, lPurchaseID, sCollectionCode, sCouponCode));
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForCollectionBillingSuccess. Failed to update billing_transactions. Purchase ID: {0} , Col Code: {1} , Coupon Code: {2}", lPurchaseID, sCollectionCode, sCouponCode));
                    #endregion
                }
            }
            else
            {
                // writing to collection_purchases failed
                res = false;
                #region Logging
                log.Debug("HandleChargeUserForCollectionBillingSuccess - " + string.Format("No ID in Collection_purchases. Site Guid: {0} , Col Code: {1} , Coupon Code: {2} , User IP: {3}", sSiteGUID, sCollectionCode, sCouponCode, sUserIP));
                WriteToUserLog(sSiteGUID, string.Format("Failed to write to collection_purchases. Col Code: {0} , Coupon Code: {1}", sCollectionCode, sCouponCode));
                #endregion
            }

            return res;
        }

        protected internal override bool HandleChargeUserForMediaFileBillingSuccess(string sSiteGUID, int domianID,
            Subscription relevantSub, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
            string sCountryCd, string sLanguageCode, string sDeviceName, BillingResponse br, string sCustomData,
            PPVModule thePPVModule, long lMediaFileID, ref long lBillingTransactionID, ref long lPurchaseID, bool isDummy, 
            string billingGuid = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            bool res = true;

            HandleCouponUses(relevantSub, string.Empty, sSiteGUID, dPrice, sCurrency, (int)lMediaFileID, sCouponCode, sUserIP,
                sCountryCd, sLanguageCode, sDeviceName, true, 0, 0);

            lBillingTransactionID = Utils.ParseLongIfNotEmpty(br.m_sRecieptCode);
            bool bIsPPVUsageModuleExists = (thePPVModule != null && thePPVModule.m_oUsageModule != null);

            if (!startDate.HasValue)
            {
                startDate = DateTime.UtcNow;
            }

            if (!endDate.HasValue)
            {
                if (bIsPPVUsageModuleExists)
                    endDate = Utils.GetEndDateTime(startDate.Value, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
            }

            lPurchaseID = ConditionalAccessDAL.Insert_NewPPVPurchase(m_nGroupID, lMediaFileID, sSiteGUID, dPrice, sCurrency,
                bIsPPVUsageModuleExists ? thePPVModule.m_oUsageModule.m_nMaxNumberOfViews : 0, sCustomData,
                relevantSub != null ? relevantSub.m_sObjectCode : null, lBillingTransactionID, startDate.Value, endDate.Value,
                DateTime.UtcNow, sCountryCd, sLanguageCode, sDeviceName, domianID, billingGuid);

            if (lPurchaseID > 0)
            {

                WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForMediaFileBillingSuccess. PPV purchase inserted into ppv_purchases. Purchase ID: {0}", lPurchaseID));
                if (lBillingTransactionID > 0)
                {
                    if (!ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPurchaseID))
                    {
                        // failed to update purchase id in billing_transactions. log
                        #region Logging
                        StringBuilder sb = new StringBuilder("Failed to update purchase id in billing_transactions table");
                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                        sb.Append(String.Concat(" Purchase ID: ", lPurchaseID));
                        sb.Append(String.Concat(" Billing transaction ID: ", lBillingTransactionID));
                        sb.Append(String.Concat(" Media File ID: ", lMediaFileID));
                        sb.Append(String.Concat(" BaseConditionalAccess is: ", this.GetType().Name));
                        sb.Append(String.Concat(" Custom Data: ", sCustomData));
                        log.Debug("HandleChargeUserForMediaFileBillingSuccess - " + sb.ToString());
                        #endregion
                    }
                    else
                    {
                        UpdatePurchaseIDInExternalBillingTable(lBillingTransactionID, lPurchaseID);
                    }
                }
                else
                {
                    res = false;
                    #region Logging
                    log.Debug("HandleChargeUserForMediaFileBillingSuccess - " + string.Format("No billing transaction id. Purchase ID: {0} , Site Guid: {1} , Media File ID: {2} , Coupon Code: {3}", lPurchaseID, sSiteGUID, lMediaFileID, sCouponCode));
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForMediaFileBillingSuccess. No billing_transactions id. Purchase ID: {0}", lPurchaseID));
                    #endregion
                }
            }
            else
            {
                res = false;
                #region Logging
                log.Debug("HandleChargeUserForMediaFileBillingSuccess - " + string.Format("No PPV Purchase ID. Billing transaction ID: {0} , Site Guid: {1} , Coupon Code: {2} , Media File ID: {3}", lBillingTransactionID, sSiteGUID, sCouponCode, lMediaFileID));
                WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForMediaFileBillingSuccess. No purchase id. Media File ID:  {0} , Coupon Code: {1}", lMediaFileID, sCouponCode));
                #endregion
            }

            return res;
        }

        /*
         * Vodafone patch. 2.12.14
         * If this method is called from the module.asmx, sProgramId will be an int.
         * If this method is called from LicensedLinkNPVRCommand, sProgramId is not necessarily an int.
         * Question: Why we decided to do that and not just create a GetNPVRLicensedLink method inside VodafoneConditionalAccess ? 
         * Answer: In order to later on unify the NPVR Licensed Link calculation with the EPG Licensed Link
         */
        public override LicensedLinkResponse GetEPGLink(string sProgramId, DateTime dStartTime, int format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP,
             string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            LicensedLinkResponse response = new LicensedLinkResponse();
            // validate user state (suspended or not)
            int domainId = 0;
            DomainSuspentionStatus domainStatus = DomainSuspentionStatus.OK;
            Utils.IsUserValid(sSiteGUID, m_nGroupID, ref domainId, ref domainStatus);

            // check if domain is suspended
            if (domainStatus == DomainSuspentionStatus.Suspended)
            {
                StringBuilder sb = new StringBuilder("GetEPGLink: domain is suspended.");
                sb.Append(String.Concat(" sSiteGUID: ", sSiteGUID));
                sb.Append(String.Concat(" group ID: ", m_nGroupID));
                sb.Append(String.Concat(" domain ID: ", domainId));
                log.Error("Error - " + sb.ToString());
                response.status = eResponseStatus.DomainSuspended.ToString();
                response.Status.Code = (int)eResponseStatus.DomainSuspended;
                return response;
            }

            string url = string.Empty;

            try
            {
                // validate EPG type format
                if (!Enum.IsDefined(typeof(eEPGFormatType), format))
                    throw new ArgumentException(String.Concat("Unknown format. Format: ", format));

                eEPGFormatType eformat = (eEPGFormatType)format;

                // check if the service allowed for domain  
                eService eservice = GetServiceByEPGFormat(eformat);
                if (eservice != eService.Unknown && !IsServiceAllowed(domainId, eservice))
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("GetEPGLink: service not allowed.");
                    sb.Append(String.Concat(" service: ", eservice.ToString()));
                    sb.Append(String.Concat(" group ID: ", m_nGroupID));
                    sb.Append(String.Concat(" domain ID: ", domainId));
                    log.Error("Error - " + sb.ToString());
                    #endregion
                    response.status = eLicensedLinkStatus.ServiceNotAllowed.ToString();
                    response.Status.Code = (int)eResponseStatus.ServiceNotAllowed;
                    response.Status.Message = string.Format("{0} service is not allowed", eservice.ToString());
                    return response;
                }

                //TODO - comment and replace
                if (eformat == eEPGFormatType.NPVR)
                {
                    /*
                     * 2.12.14
                     * Vodafone patch. In Vodafone we retrieve the NPVR Licensed Link directly from the NPVR Provider (ALU)
                     * CalcNPVRLicensedLink returns string.Empty unless it is Vodafone. Meaning, that if the account is not Vodafone,
                     * It continues as usual. If it is Vodafone, it returns the licensed link that we fetched from ALU.
                     */
                    string npvrLicensedLink = CalcNPVRLicensedLink(sProgramId, dStartTime, format, sSiteGUID, nMediaFileID, sBasicLink, sUserIP,
                        sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sCouponCode);
                    if (npvrLicensedLink.Length > 0)
                    {
                        response.Status.Code = (int)eResponseStatus.OK;
                        response.status = eLicensedLinkStatus.OK.ToString();
                        response.mainUrl = npvrLicensedLink;
                        return response;
                    }
                }
                int nProgramId = Int32.Parse(sProgramId);
                int fileMainStreamingCoID = 0; // CDN Streaming id
                int mediaId = 0;
                string fileType = string.Empty;
                LicensedLinkResponse oLicensedLinkResponse = GetLicensedLinks(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sCouponCode, 
                    eObjectType.EPG, ref fileMainStreamingCoID, ref mediaId, ref fileType);
                
                //GetLicensedLink return empty link no need to continue
                if (oLicensedLinkResponse == null || oLicensedLinkResponse.Status == null || oLicensedLinkResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    throw new Exception("GetLicensedLinks returned empty response.");
                }

                Dictionary<string, object> dURLParams = new Dictionary<string, object>();

                Scheduling scheduling = Api.Module.GetProgramSchedule(m_nGroupID, nProgramId);
                if (scheduling != null)
                {
                    dURLParams.Add(EpgLinkConstants.PROGRAM_END, scheduling.EndTime);

                    dURLParams.Add(EpgLinkConstants.EPG_FORMAT_TYPE, eformat);
                    switch (eformat)
                    {
                        case eEPGFormatType.Catchup:
                        case eEPGFormatType.StartOver:
                            {
                                dURLParams.Add(EpgLinkConstants.PROGRAM_START, scheduling.StartDate);
                            }
                            break;
                        case eEPGFormatType.LivePause:
                            dURLParams.Add(EpgLinkConstants.PROGRAM_START, dStartTime);
                            break;
                        case eEPGFormatType.NPVR:
                        default:
                            {
                                #region Logging
                                StringBuilder sb = new StringBuilder(String.Concat("Error. Flow not implemented for format: ", eformat.ToString()));
                                sb.Append(String.Concat(" P ID: ", nProgramId));
                                sb.Append(String.Concat(" ST: ", dStartTime.ToString()));
                                sb.Append(String.Concat(" SG :", sSiteGUID));
                                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                                sb.Append(String.Concat(" BL: ", sBasicLink));
                                sb.Append(String.Concat(" U IP: ", sUserIP));
                                sb.Append(String.Concat(" Ref: ", sRefferer));
                                sb.Append(String.Concat(" Country Cd: ", sCOUNTRY_CODE));
                                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                                log.Error("Error - " + sb.ToString());
                                #endregion
                                break;
                            }
                    }
                }
                else
                {
                    // to do write to log
                    log.Debug("get epg url link - " + string.Format("api.GetProgramSchedule return null response can't create link with no dates "));
                    response.status = eLicensedLinkStatus.Error.ToString();
                    response.Status.Code = (int)eResponseStatus.Error;
                    return response;
                }

                // get adapter
                bool isDefaultAdapter = false;
                var adapterResponse = Utils.GetRelevantCDN(m_nGroupID, fileMainStreamingCoID, eAssetTypes.EPG, ref isDefaultAdapter);

                // if adapter response is not null and is adapter (has an adapter url) - call the adapter
                if (adapterResponse.Adapter != null && !string.IsNullOrEmpty(adapterResponse.Adapter.AdapterUrl))
                {
                    int actionType = Utils.MapActionTypeForAdapter(eformat);

                    // if the adapter is default - append the adapter's base url to the file urls
                    if (isDefaultAdapter)
                    {
                        sBasicLink = string.Format("{0}{1}", adapterResponse.Adapter.BaseUrl, sBasicLink);
                    }

                    // main url
                    var link = CDNAdapterController.GetInstance().GetEpgLink(m_nGroupID, adapterResponse.Adapter.ID, sSiteGUID, sBasicLink, fileType, nProgramId, mediaId, nMediaFileID,
                        TVinciShared.DateUtils.DateTimeToUnixTimestamp(scheduling.StartDate), actionType, sUserIP);

                    if (link != null)
                    {
                        response.mainUrl = link.Url;
                        response.status = eLicensedLinkStatus.OK.ToString();
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                else
                {
                    //call the right provider to get the epg link 
                    string CdnStrID = string.Empty;

                    bool bIsDynamic = Utils.GetStreamingUrlType(fileMainStreamingCoID, ref CdnStrID);
                    dURLParams.Add(EpgLinkConstants.IS_DYNAMIC, bIsDynamic);
                    dURLParams.Add(EpgLinkConstants.BASIC_LINK, sBasicLink);

                    StreamingProvider.ILSProvider provider = StreamingProvider.LSProviderFactory.GetLSProvidernstance(CdnStrID);
                    if (provider != null)
                    {
                        string liveUrl = provider.GenerateEPGLink(dURLParams);
                        if (!string.IsNullOrEmpty(liveUrl))
                        {
                            url = liveUrl;
                        }
                    }
                    response.Status.Code = (int)eResponseStatus.OK;
                    response.status = eLicensedLinkStatus.OK.ToString();
                    response.mainUrl = url;
                }

                return response;

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at GetEPGLink. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" P ID: ", sProgramId));
                sb.Append(String.Concat(" ST: ", dStartTime.ToString()));
                sb.Append(String.Concat(" SG :", sSiteGUID));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" BL: ", sBasicLink));
                sb.Append(String.Concat(" U IP: ", sUserIP));
                sb.Append(String.Concat(" Ref: ", sRefferer));
                sb.Append(String.Concat(" Country Cd: ", sCOUNTRY_CODE));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Cpn Cd: ", sCouponCode));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                response.status = eLicensedLinkStatus.Error.ToString();
                response.Status.Code = (int)eResponseStatus.Error;
                return response;
            }
        }

        protected internal override bool HandlePPVBillingSuccess(ref TransactionResponse response, string siteguid, long houseHoldId, Subscription relevantSub, double price, string currency,
                                                        string coupon, string userIp, string country, string deviceName, long billingTransactionId, string customData,
                                                        PPVModule thePPVModule, int productId, int contentId, string billingGuid, DateTime entitlementDate, ref long purchaseId)
        {
            purchaseId = 0;
            try
            {
                // update coupon uses
                HandleCouponUses(relevantSub, productId.ToString(), siteguid, price, currency, contentId, coupon, userIp, country, string.Empty, deviceName, true, 0, 0);

                bool isPPVUsageModuleExists = (thePPVModule != null && thePPVModule.m_oUsageModule != null);

                // get PPV end date
                DateTime startDate = entitlementDate;
                DateTime endDate = entitlementDate;

                if (response != null && response.StartDateSeconds > 0)
                {
                    // received start date form transaction - calculate end date accordingly
                    startDate = TVinciShared.DateUtils.UnixTimeStampToDateTime(response.StartDateSeconds);
                }

                if (isPPVUsageModuleExists)
                    endDate = Utils.GetEndDateTime(startDate, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);

                if (response != null)
                {
                    if (response.EndDateSeconds == 0)
                        response.EndDateSeconds = (long)endDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                    if (response.StartDateSeconds == 0)
                        response.StartDateSeconds = (long)entitlementDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }

                int maxNumOfViews = isPPVUsageModuleExists ? thePPVModule.m_oUsageModule.m_nMaxNumberOfViews : 0;
                string subscriptionCode = relevantSub != null ? relevantSub.m_sObjectCode : null;

                // grant entitlement
                PpvPurchase ppvPurchase = new PpvPurchase(m_nGroupID)
                {
                    contentId = contentId,
                    siteGuid = siteguid,
                    price = price,
                    currency = currency,
                    maxNumOfViews = maxNumOfViews,
                    customData = customData,
                    subscriptionCode = subscriptionCode,
                    billingTransactionId = billingTransactionId,
                    startDate = startDate,
                    endDate = endDate,
                    entitlementDate = entitlementDate,
                    country = country,
                    deviceName = deviceName,
                    houseHoldId = houseHoldId,
                    billingGuid = billingGuid
                };
                ppvPurchase.Insert();
                purchaseId = ppvPurchase.purchaseId;
                               
                if (purchaseId < 1)
                {
                    // entitlement failed
                    log.ErrorFormat("Failed to insert PPV purchase. Billing transaction ID: {0} , Siteguid: {1} , Content ID: {2}, Product ID: {3}",
                                    billingTransactionId, // {0}
                                    siteguid,             // {1}
                                    contentId,            // {2}
                                    productId);           // {3}
                }

                if (billingTransactionId > 0)
                {
                    ApiDAL.Update_PurchaseIDInBillingTransactions(billingTransactionId, purchaseId);
                }
            }
            catch (Exception ex)
            {
                log.Error("fail HandlePPVBillingSuccess ", ex);
            }

            return purchaseId > 0;
        }

        protected internal override bool HandleSubscriptionBillingSuccess(
            ref TransactionResponse response, string siteguid, long houseHoldId, Subscription subscription, double price, 
            string currency, string coupon, string userIP,
                                                                 string country, string deviceName, long billingTransactionId, string customData, int productId, string billingGuid,
            bool isEntitledToPreviewModule, bool isRecurring, DateTime? entitlementDate, ref long purchaseId, ref DateTime? subscriptionEndDate,
            SubscriptionPurchaseStatus purchaseStatus = SubscriptionPurchaseStatus.OK)
        {
            purchaseId = 0;
            try
            {
                // update coupon uses
                HandleCouponUses(subscription, string.Empty, siteguid, price, currency, 0, coupon, userIP, country, string.Empty, deviceName, true, 0, 0);

                long previewModuleID = 0;
                bool usageModuleExists = (subscription != null && subscription.m_oUsageModule != null);
                if (subscription.m_oPreviewModule != null)
                {
                    previewModuleID = subscription.m_oPreviewModule.m_nID;
                }

                // get subscription end date
                if (!entitlementDate.HasValue)
                {
                    entitlementDate = DateTime.UtcNow;
                }

                if (!subscriptionEndDate.HasValue)
                {
                    subscriptionEndDate = Utils.CalcSubscriptionEndDate(subscription, isEntitledToPreviewModule, entitlementDate.Value);
                }

                DateTime transactionStartDate = entitlementDate.Value;
                // update response object
                if (response != null)
                {
                    if (response.EndDateSeconds == 0)
                    {
                        // update end date by subscription end date
                        response.EndDateSeconds = (long)subscriptionEndDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    }

                    if (response.StartDateSeconds == 0)
                    {
                        // update start date by subscription start date
                        response.StartDateSeconds = (long)entitlementDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    }
                    else
                    {
                        // update start date by transaction start date
                        transactionStartDate = TVinciShared.DateUtils.UnixTimeStampToDateTime(response.StartDateSeconds);
                    }

                    response.AutoRenewing = isRecurring;

                }

                // grant entitlement
                SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(m_nGroupID)
                    {
                        productId = productId.ToString(),
                        price = price,
                        siteGuid = siteguid,
                        isEntitledToPreviewModule = isEntitledToPreviewModule,
                        currency = currency,
                        customData = customData,
                        country = country,
                        deviceName = deviceName,
                        usageModuleExists = usageModuleExists,
                        viewLifeCycle = subscription.m_oUsageModule.m_tsViewLifeCycle,
                        maxNumberOfViews = subscription.m_oUsageModule.m_nMaxNumberOfViews,
                        isRecurring = isRecurring,
                        billingTransactionId = billingTransactionId,
                        previewModuleId = previewModuleID,
                        startDate = transactionStartDate,
                        endDate = subscriptionEndDate,
                        entitlementDate = entitlementDate,
                        houseHoldId = houseHoldId,
                        billingGuid = billingGuid
                    };
                subscriptionPurchase.Insert();
                purchaseId = subscriptionPurchase.purchaseId;


                if (purchaseId == 0)
                {
                    // entitlement failed
                    log.ErrorFormat("Failed to insert subscription purchase. Billing transaction ID: {0} , Siteguid: {1} , Product ID: {2}",
                                    billingTransactionId, // {0}
                                    siteguid,             // {1}
                                    productId);           // {2}
                }

                if (billingTransactionId > 0)
                {
                    ApiDAL.Update_PurchaseIDInBillingTransactions(billingTransactionId, purchaseId);
                }
            }
            catch (Exception ex)
            {
                log.Error("fail HandleSubscriptionBillingSuccess ", ex);
            }
            return purchaseId > 0;
        }

        protected internal override bool HandleCollectionBillingSuccess(ref TransactionResponse response, string siteGUID, long houseHoldID, 
            Collection collection, double price, string currency, string coupon, string userIP, string country, string deviceName, 
            long billingTransactionId, string customData, int productID, string billingGuid, bool isEntitledToPreviewModule, DateTime entitlementDate, ref long purchaseID)
        {
            purchaseID = 0;
            try
            {
                // update coupon uses
                HandleCouponUses(null, string.Empty, siteGUID, price, currency, 0, coupon, userIP, country, string.Empty, deviceName, true, 0, productID);

                bool usageModuleExists = (collection != null && collection.m_oUsageModule != null);

                // get collection end date
                DateTime collectionEndDate = CalcCollectionEndDate(collection, entitlementDate);

                // update response object
                if (response != null)
                {
                    response.EndDateSeconds = (long)collectionEndDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    response.StartDateSeconds = (long)entitlementDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                }

                // grant entitlement
                purchaseID = ConditionalAccessDAL.Insert_NewMColPurchase(m_nGroupID, productID.ToString(), siteGUID, price, currency, customData, country,
                                                                         deviceName, usageModuleExists ? collection.m_oUsageModule.m_nMaxNumberOfViews : 0,
                                                                         usageModuleExists ? collection.m_oUsageModule.m_tsViewLifeCycle : 0, billingTransactionId,
                                                                         entitlementDate, collectionEndDate, entitlementDate, houseHoldID, billingGuid);

                if (purchaseID < 1)
                {
                    // entitlement failed
                    log.ErrorFormat("Failed to insert collection purchase. Billing transaction ID: {0} , Siteguid: {1} , Product ID: {2}",
                                    billingTransactionId, // {0}
                                    siteGUID,             // {1}
                                    productID);           // {2}
                }

                ApiDAL.Update_PurchaseIDInBillingTransactions(billingTransactionId, purchaseID);
            }
            catch (Exception ex)
            {
                log.Error("fail HandleCollectionBillingSuccess ", ex);
            }
            return purchaseID > 0;
        }
    }
}
