using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.llnw.mediavault;
using DAL;


namespace ConditionalAccess
{
    public class TvinciConditionalAccess : BaseConditionalAccess
    {
        public TvinciConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public TvinciConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        protected override string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer)
        {
            //object oSecretCode = ODBCWrapper.Utils.GetTableSingleVal("groups_ll_parameters", "SECRET_CODE", "group_id", "=", m_nGroupID);
            //string sSecretCode = "";
            //if (oSecretCode != null && oSecretCode != DBNull.Value)
            //    sSecretCode = oSecretCode.ToString();
            //if (sSecretCode != "")
            //    return MediaVault.GetHashedURL(sSecretCode, sBasicLink, sUserIP, sRefferer);
            //return sBasicLink;

            string secretCode = ConditionalAccessDAL.Get_LicensedLinkSecretCode(m_nGroupID);
            if (secretCode.Length > 0)
                return MediaVault.GetHashedURL(secretCode, sBasicLink, sUserIP, sRefferer);
            return sBasicLink;
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
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            using (TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.Campaign camp = m.GetCampaignData(sWSUserName, sWSPass, campaignID);
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
        }

        public override CampaignActionInfo ActivateCampaignWithInfo(int campaignID, CampaignActionInfo cai)
        {
            CampaignActionInfo retVal = null;
            string sIP = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            using (TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule())
            {
                string sWSURL = Utils.GetWSURL("pricing_ws");
                if (sWSURL.Length > 0)
                    m.Url = sWSURL;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.Campaign camp = null;
                if (campaignID > 0)
                {
                    camp = m.GetCampaignData(sWSUserName, sWSPass, campaignID);
                }
                else
                {
                    if (cai != null && !string.IsNullOrEmpty(cai.m_socialInviteInfo.m_hashCode))
                    {
                        camp = m.GetCampaignsByHash(sWSUserName, sWSPass, cai.m_socialInviteInfo.m_hashCode);
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
        }

        protected override TvinciBilling.BillingResponse HandleBaseRenewMPPBillingCharge(string sSiteGuid, double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nRecPeriods, string sExtraParams, int nBillingMethod, long lPurchaseID, ConditionalAccess.eBillingProvider bp)
        {
            TvinciBilling.module bm = null;
            TvinciBilling.BillingResponse res = null;

            try
            {
                string sWSUsername = string.Empty;
                string sWSPass = string.Empty;

                InitializeBillingModule(ref bm, ref sWSUsername, ref sWSPass);

                if (dPrice != 0)
                {
                    switch (bp)
                    {
                        case eBillingProvider.Adyen:
                            {
                                res = bm.DD_ChargeUser(sWSUsername, sWSPass, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, lPurchaseID.ToString(), nBillingMethod);
                                break;
                            }
                        case eBillingProvider.M1:
                            {
                                res = bm.Cellular_ChargeUser(sWSUsername, sWSPass, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, lPurchaseID.ToString());
                                break;
                            }
                    }



                }
                else
                {
                    sExtraParams = "AdyanDummy";
                    res = bm.CC_DummyChargeUser(sWSUsername, sWSPass, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, nPaymentNumber, nRecPeriods, sExtraParams);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                #region Disposing
                if (bm != null)
                {
                    bm.Dispose();
                    bm = null;
                }
                #endregion
            }

            return res;
        }

        protected override bool HandleMPPRenewalBillingSuccess(string sSiteGUID, string sSubscriptionCode, DateTime dtCurrentEndDate,
            bool bIsPurchasedWithPreviewModule, long lPurchaseID, string sCurrency, double dPrice, int nPaymentNumber,
            string sBillingTransactionID, int nUsageModuleMaxVLC, bool bIsMPPRecurringInfinitely, int nNumOfRecPeriods)
        {
            bool res = true;
            WriteToUserLog(sSiteGUID, String.Concat("MPP Renewal: ", sSubscriptionCode, " renewed ", dPrice.ToString(), sCurrency));

            DateTime dNext = Utils.GetEndDateTime(dtCurrentEndDate, nUsageModuleMaxVLC);

            if (IsLastPeriodOfLastUsageModule(bIsPurchasedWithPreviewModule, bIsMPPRecurringInfinitely, nNumOfRecPeriods, nPaymentNumber))
            {
                ConditionalAccessDAL.Update_MPPRenewalData(lPurchaseID, false, dNext, 0, "CA_CONNECTION_STRING");
            }
            else
            {
                ConditionalAccessDAL.Update_MPPRenewalData(lPurchaseID, true, dNext, 0, "CA_CONNECTION_STRING");
            }

            long lBillingTransactionID = 0;

            if (!string.IsNullOrEmpty(sBillingTransactionID) && Int64.TryParse(sBillingTransactionID, out lBillingTransactionID) && lBillingTransactionID > 0)
            {
                ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPurchaseID);
            }
            else
            {
                // invalid format for billing transaction id
                res = false;

                #region Logging
                Logger.Logger.Log("DD_BaseRenewMultiUsageSubscription", string.Format("HandleMPPRenewalBillingSuccess. Failed to update purchase id in billing_transactions. Purchase ID: {0} , Billing Transaction ID: {1} , Site Guid: {2} , BaseConditionalAccess is: {3}", lPurchaseID, sBillingTransactionID, sSiteGUID, this.GetType().ToString()), "TvinciRenewer");
                WriteToUserLog(sSiteGUID, string.Format("MPP Renewal. Failed to update purchase id in billing transactions. Purchase ID: {0} , Billing transaction id: {1}", lPurchaseID, lBillingTransactionID));
                #endregion
            }

            return res;
        }

        protected override TvinciBilling.BillingResponse HandleCCChargeUser(string sWSUsername, string sWSPassword, string sSiteGuid, double dPrice, string sCurrency, string sUserIP, string sCustomData, int nPaymentNumber, int nNumOfPayments, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV, bool bIsDummy, bool bIsEntitledToPreviewModule, ref TvinciBilling.module bm)
        {
            if (!bIsDummy && !bIsEntitledToPreviewModule)
            {
                return bm.CC_ChargeUser(sWSUsername, sWSPassword, sSiteGuid, dPrice, sCurrency, sUserIP, sCustomData, 1, nNumOfPayments, sExtraParams, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                // if the user is entitled to preview module, and this function is called it means
                // we already have the user's cc details so we can dummy charge him
                // (this comment is correct for Adyen only. In Cinepolis there is no dummy charge. During development only MediaCorp and Cinepolis asked for preview module.)
                return bm.CC_DummyChargeUser(sWSUsername, sWSPassword, sSiteGuid, bIsEntitledToPreviewModule ? 0.0 : dPrice, sCurrency, sUserIP, sCustomData, 1, nNumOfPayments, sExtraParams);
            }
        }

        protected override bool HandleChargeUserForSubscriptionBillingSuccess(string sSiteGUID, TvinciPricing.Subscription theSub,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, bool bIsEntitledToPreviewModule, string sSubscriptionCode,
            string sCustomData, bool bIsRecurring, ref long lBillingTransactionID, ref long lPurchaseID)
        {
            bool res = true;
            HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP, sCountryCd, sLanguageCode, sDeviceName, true, 0, 0);

            long lPreviewModuleID = 0;
            if (theSub.m_oPreviewModule != null)
                lPreviewModuleID = theSub.m_oPreviewModule.m_nID;

            lBillingTransactionID = Utils.ParseLongIfNotEmpty(br.m_sRecieptCode);

            bool bUsageModuleExists = (theSub != null && theSub.m_oUsageModule != null);
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtSubEndDate = CalcSubscriptionEndDate(theSub, bIsEntitledToPreviewModule, dtUtcNow);

            lPurchaseID = ConditionalAccessDAL.Insert_NewMPPPurchase(m_nGroupID,
                sSubscriptionCode, sSiteGUID, bIsEntitledToPreviewModule ? 0.0 : dPrice, sCurrency, sCustomData,
                sCountryCd, sLanguageCode, sDeviceName, bUsageModuleExists ? theSub.m_oUsageModule.m_nMaxNumberOfViews : 0,
                bUsageModuleExists ? theSub.m_oUsageModule.m_tsViewLifeCycle : 0, bIsRecurring, lBillingTransactionID,
                lPreviewModuleID, dtUtcNow, dtSubEndDate, dtUtcNow, string.Empty);

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
                        Logger.Logger.Log("HandleChargeUserForSubscriptionBillingSuccess", sb.ToString(), GetLogFilename());
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
                    Logger.Logger.Log("HandleChargeUserForSubscriptionBillingSuccess", string.Format("No billing_transactions ID. SiteGuid: {0} , Purchase ID: {1} , Sub Code: {2} , Coupon Code: {3}", sSiteGUID, lPurchaseID, sSubscriptionCode, sCouponCode), GetLogFilename());
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForSubscriptionBillingSuccess. Failed to update billing_transactions. Purchase ID: {0} , Sub Code: {1} , Coupon Code: {2}", lPurchaseID, sSubscriptionCode, sCouponCode));
                    #endregion
                }
            }
            else
            {
                // writing to subscription_purchases failed
                res = false;
                #region Logging
                Logger.Logger.Log("HandleChargeUserForSubscriptionBillingSuccess", string.Format("No ID in subscriptions_purchases. Site Guid: {0} , Sub Code: {1} , Coupon Code: {2} , User IP: {3}", sSiteGUID, sSubscriptionCode, sCouponCode, sUserIP), GetLogFilename());
                WriteToUserLog(sSiteGUID, string.Format("Failed to write to subscription_purchases. Sub Code: {0} , Coupon Code: {1}", sSubscriptionCode, sCouponCode));
                #endregion
            }

            return res;
        }

        protected override bool HandleChargeUserForCollectionBillingSuccess(string sSiteGUID, TvinciPricing.Collection theCol,
            double dPrice, string sCurrency, string sCouponCode, string sUserIP, string sCountryCd, string sLanguageCode,
            string sDeviceName, TvinciBilling.BillingResponse br, string sCollectionCode,
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

            lPurchaseID = ConditionalAccessDAL.Insert_NewMColPurchase(m_nGroupID,
                sCollectionCode, sSiteGUID, dPrice, sCurrency, sCustomData,
                sCountryCd, sLanguageCode, sDeviceName, bUsageModuleExists ? theCol.m_oUsageModule.m_nMaxNumberOfViews : 0,
                bUsageModuleExists ? theCol.m_oUsageModule.m_tsViewLifeCycle : 0, lBillingTransactionID,
                dtUtcNow, dtSubEndDate, dtUtcNow, string.Empty);

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
                        Logger.Logger.Log("HandleChargeUserForCollectionBillingSuccess", sb.ToString(), GetLogFilename());
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
                    Logger.Logger.Log("HandleChargeUserForCollectionBillingSuccess", string.Format("No billing_transactions ID. SiteGuid: {0} , Purchase ID: {1} , Col Code: {2} , Coupon Code: {3}", sSiteGUID, lPurchaseID, sCollectionCode, sCouponCode), GetLogFilename());
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForCollectionBillingSuccess. Failed to update billing_transactions. Purchase ID: {0} , Col Code: {1} , Coupon Code: {2}", lPurchaseID, sCollectionCode, sCouponCode));
                    #endregion
                }
            }
            else
            {
                // writing to collection_purchases failed
                res = false;
                #region Logging
                Logger.Logger.Log("HandleChargeUserForCollectionBillingSuccess", string.Format("No ID in Collection_purchases. Site Guid: {0} , Col Code: {1} , Coupon Code: {2} , User IP: {3}", sSiteGUID, sCollectionCode, sCouponCode, sUserIP), GetLogFilename());
                WriteToUserLog(sSiteGUID, string.Format("Failed to write to collection_purchases. Col Code: {0} , Coupon Code: {1}", sCollectionCode, sCouponCode));
                #endregion
            }

            return res;
        }

        protected override bool HandleChargeUserForMediaFileBillingSuccess(string sSiteGUID,
            TvinciPricing.Subscription relevantSub, double dPrice, string sCurrency, string sCouponCode, string sUserIP,
            string sCountryCd, string sLanguageCode, string sDeviceName, TvinciBilling.BillingResponse br, string sCustomData,
            TvinciPricing.PPVModule thePPVModule, long lMediaFileID, ref long lBillingTransactionID, ref long lPurchaseID)
        {
            bool res = true;

            HandleCouponUses(relevantSub, string.Empty, sSiteGUID, dPrice, sCurrency, (int)lMediaFileID, sCouponCode, sUserIP,
                sCountryCd, sLanguageCode, sDeviceName, true, 0, 0);

            lBillingTransactionID = Utils.ParseLongIfNotEmpty(br.m_sRecieptCode);
            bool bIsPPVUsageModuleExists = (thePPVModule != null && thePPVModule.m_oUsageModule != null);
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtEndDate = dtUtcNow;
            if (bIsPPVUsageModuleExists)
                dtEndDate = Utils.GetEndDateTime(dtUtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);

            lPurchaseID = ConditionalAccessDAL.Insert_NewPPVPurchase(m_nGroupID, lMediaFileID, sSiteGUID, dPrice, sCurrency,
                bIsPPVUsageModuleExists ? thePPVModule.m_oUsageModule.m_nMaxNumberOfViews : 0, sCustomData,
                relevantSub != null ? relevantSub.m_sObjectCode : null, lBillingTransactionID, dtUtcNow, dtEndDate,
                dtUtcNow, sCountryCd, sLanguageCode, sDeviceName, string.Empty);

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
                        Logger.Logger.Log("HandleChargeUserForMediaFileBillingSuccess", sb.ToString(), GetLogFilename());
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
                    Logger.Logger.Log("HandleChargeUserForMediaFileBillingSuccess", string.Format("No billing transaction id. Purchase ID: {0} , Site Guid: {1} , Media File ID: {2} , Coupon Code: {3}", lPurchaseID, sSiteGUID, lMediaFileID, sCouponCode), GetLogFilename());
                    WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForMediaFileBillingSuccess. No billing_transactions id. Purchase ID: {0}", lPurchaseID));
                    #endregion
                }
            }
            else
            {
                res = false;
                #region Logging
                Logger.Logger.Log("HandleChargeUserForMediaFileBillingSuccess", string.Format("No PPV Purchase ID. Billing transaction ID: {0} , Site Guid: {1} , Coupon Code: {2} , Media File ID: {3}", lBillingTransactionID, sSiteGUID, sCouponCode, lMediaFileID), GetLogFilename());
                WriteToUserLog(sSiteGUID, string.Format("HandleChargeUserForMediaFileBillingSuccess. No purchase id. Media File ID:  {0} , Coupon Code: {1}", lMediaFileID, sCouponCode));
                #endregion
            }

            return res;
        }

        protected override bool RecalculateDummyIndicatorForChargeMediaFile(bool bDummy, PriceReason reason, bool bIsCouponUsedAndValid)
        {
            return bDummy;
        }

    }
}
