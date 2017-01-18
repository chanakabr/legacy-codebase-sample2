using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class DummyCreditCard : BaseCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DummyCreditCard(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            return string.Empty;
        }

        public override string GetClientMerchantSig(string sParams)
        {
            return string.Empty;
        }

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();
            if (!Utils.IsUserExist(sSiteGUID, m_nGroupID))
            {
                ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = string.Empty;
                ret.m_sStatusDescription = "Unknown or active user";
                return ret;
            }

            Int32 nTransactionLocalID = InsertNewDummyTransaction(sSiteGUID, dChargePrice, sCurrencyCode, sCustomData);
            if (sExtraParameters == "AdyanDummy")
            {
                ret = GetBillingResponse(nTransactionLocalID, sSiteGUID, nPaymentNumber, nNumberOfPayments, 10, sCustomData);
                if (ret.m_oStatus == BillingResponseStatus.Success)
                {
                    int id = 0;
                    if (!string.IsNullOrEmpty(ret.m_sRecieptCode))
                    {
                        id = int.Parse(ret.m_sRecieptCode);
                    }
                    SendAdyenPurchaseMail(sCustomData, dChargePrice, sCurrencyCode, "Gift", sSiteGUID, id, "0");
                }
            }
            else
            {
                ret = GetBillingResponse(nTransactionLocalID, sSiteGUID, nPaymentNumber, nNumberOfPayments, sCustomData);
            }

            return ret;
        }

        public override bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID)
        {
            return true;
        }
        
        private void SendAdyenPurchaseMail(string sCustomData, double dChargePrice, string sCurrencyCode, string sPaymentMethod, string sSiteGuid, long nBillingTransactionID, string sPSPReference)
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(sCustomData);
                System.Xml.XmlNode theRequest = doc.FirstChild;
                string sType = TVinciShared.XmlUtils.GetSafeParValue(".", "type", ref theRequest);
                if (sType.Equals("pp"))
                {
                    int nMediaID = 0;
                    string sMediaID = TVinciShared.XmlUtils.GetSafeValue("m", ref theRequest);
                    if (!string.IsNullOrEmpty(sMediaID))
                    {
                        nMediaID = int.Parse(sMediaID);
                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();
                        Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, nBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, m_nGroupID, string.Empty, eMailTemplateType.Purchase);
                    }
                }
                else if (sType.Equals("sp"))
                {
                    int nSubID = 0;
                    string sSubscriptionID = TVinciShared.XmlUtils.GetSafeValue("s", ref theRequest);

                    if (!string.IsNullOrEmpty(sSubscriptionID))
                    {
                        nSubID = int.Parse(sSubscriptionID);
                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubID, "pricing_connection").ToString();

                        string sPreivewEnd = TVinciShared.XmlUtils.GetSafeValue("prevlc", ref theRequest);
                        log.Debug("Name - " + sItemName);
                        Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, nBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, m_nGroupID, sPreivewEnd, eMailTemplateType.Purchase);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + String.Concat(ex.Message, " || ", ex.StackTrace), ex);
            }
        }

        protected Int32 InsertNewDummyTransaction(string sSiteGUID, double dPrice, string sCurrency, string sCustomData)
        {
            Int32 nRet = 0;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                insertQuery = new ODBCWrapper.InsertQuery("dummy_transactions");
                insertQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DUMMY_CUSTOMDATA", "=", sCustomData);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery.Execute();


                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select id from dummy_transactions where is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DUMMY_CUSTOMDATA", "=", sCustomData);
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
            }
            return nRet;
        }

        protected BillingResponse GetBillingResponse(Int32 nTransactionLocalID, string sSiteGUID,
            Int32 nPaymentNumber, Int32 nNumberOfPayments, string sCustomData)
        {
            return GetBillingResponse(nTransactionLocalID, sSiteGUID, nPaymentNumber, nNumberOfPayments, (int)eBillingProvider.Dummy, sCustomData);
        }

        protected BillingResponse GetBillingResponse(Int32 nTransactionLocalID, string sSiteGUID,
          Int32 nPaymentNumber, Int32 nNumberOfPayments, Int32 nBillingProvider, string sCustomData)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("dummy_transactions");
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTransactionLocalID);
                updateQuery += " and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DUMMY_CUSTOMDATA", "=", sCustomData);
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }


            BillingResponse ret = new BillingResponse();
            ret.m_oStatus = BillingResponseStatus.Success;
            ret.m_sExternalReceiptCode = nTransactionLocalID.ToString();
            ret.m_sStatusDescription = "OK";

            Int32 nMediaFileID = 0;
            Int32 nMediaID = 0;
            string sSubscriptionCode = "";
            string sPrePaidCode = string.Empty;
            string sPPVCode = "";
            string sPriceCode = "";
            string sPPVModuleCode = "";
            bool bIsRecurring = false;
            string sCurrencyCode = "";
            double dChargePrice = 0.0;

            string sRelevantSub = "";
            string sRelevantPrePaid = string.Empty;
            string sUserGUID = "";
            Int32 nMaxNumberOfUses = 0;
            Int32 nMaxUsageModuleLifeCycle = 0;
            Int32 nViewLifeCycleSecs = 0;
            string sPurchaseType = "";

            string sCountryCd = "";
            string sLanguageCode = "";
            string sDeviceName = "";
            string sPreviewModuleID = string.Empty;
            string sCollectionCode = string.Empty;

            Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sRelevantPrePaid, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments,
                    ref sUserGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                    ref sCountryCd, ref sLanguageCode, ref sDeviceName, ref sPreviewModuleID, ref sCollectionCode);

            long lTransID = Utils.InsertBillingTransaction(sSiteGUID, "", dChargePrice, sPriceCode,
                    sCurrencyCode, sCustomData, (int)(ret.m_oStatus), "", bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                    sSubscriptionCode, "", m_nGroupID, nBillingProvider, nTransactionLocalID, 0.0, dChargePrice, nPaymentNumber, nNumberOfPayments, "",
                    sCountryCd, sLanguageCode, sDeviceName, 4, 7, sRelevantPrePaid, sPreviewModuleID, sCollectionCode);
            ret.m_sRecieptCode = lTransID.ToString();
            return ret;
        }
    }
}
