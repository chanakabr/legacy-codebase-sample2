using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Core.Users;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class TikleCreditCard : BaseCreditCard
    {
        public TikleCreditCard(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override string GetClientMerchantSig(string sParams)
        {
            return string.Empty;
        }


        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, 
            string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments , string sExtraParams)
        {
            UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
            if (uObj.m_RespStatus != ResponseStatus.OK)
            {
                BillingResponse ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Unknown or active user";
                return ret;
            }

            Int32 nExpM = 1;
            Int32 nExpY = 10;
            string sIssueNum = "";
            string sStartM = "";
            string sStartY = "";
            string sIssuerBank = "";
            int transID = 0;
            string sToken = GetUserToken(sSiteGUID, ref nExpM, ref nExpY, ref sStartM, ref sStartY, ref sIssueNum, ref sIssuerBank, ref transID);
            if (sToken == "")
            {
                BillingResponse ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "User does not have a token";
                return ret;
            }

            UserBasicData uBasicData = uObj.m_user.m_oBasicData;
            Int32 nTransactionLocalID = 0;

            APILogic.tikle.Service s = new APILogic.tikle.Service();
            //System.Net.WebProxy p = new WebProxy("127.0.0.1");
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            s.Url = sTikleWSURL;

            string sTransactionData = GetTransactionData(sSiteGUID, uBasicData, sToken, dChargePrice,
                sCurrencyCode, sUserIP, sCustomData, sExtraParams , ref nTransactionLocalID);
            
            string sMD5Hash = Utils.GetHash(nTransactionLocalID.ToString() + sTransactionData, "WS_SECRET");
            try
            {
                APILogic.tikle.PurchaseResponseInfo resp = s.Purchase(nTransactionLocalID.ToString(), sTransactionData, sMD5Hash);
                BillingResponse bResp = GetBillingResponse(resp, nTransactionLocalID, sSiteGUID);
                Int32 nMediaFileID = 0;
                Int32 nMediaID = 0;
                string sSubscriptionCode = "";
                string sPPVCode = "";
                string sPriceCode = "";
                string sPPVModuleCode = "";
                bool bIsRecurring = false;

                string sRelevantSub = "";
                string sRelevantPrePaid = string.Empty;
                Int32 nMaxNumberOfUses = 0;
                Int32 nMaxUsageModuleLifeCycle = 0;
                Int32 nViewLifeCycleSecs = 0;
                string sPurchaseType = "";
                string sCountryCd = "";
                string sLanguageCode = "";
                string sDeviceName = "";

                Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID , ref sSubscriptionCode, ref sPPVCode, ref sRelevantPrePaid, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments,
                    ref sSiteGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs , 
                    ref sPurchaseType , ref sCountryCd , ref sLanguageCode , ref sDeviceName);
                bool bSaved = false;
                string sLastDigits = GetUserDigits(sSiteGUID, ref bSaved);
                if (String.IsNullOrEmpty(resp.ResultDetail) == false)
                    resp.ResultDetail = resp.ResultDetail;
                long lTransactionID = Utils.InsertBillingTransaction(sSiteGUID, sLastDigits, dChargePrice, sPriceCode,
                    sCurrencyCode, sCustomData, resp.Result, resp.ResultDetail, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                    sSubscriptionCode, "", m_nGroupID, 5, nTransactionLocalID , 0.0 , dChargePrice , nPaymentNumber , nNumberOfPayments , sExtraParams ,
                    sCountryCd, sLanguageCode, sDeviceName, 2, 1, sRelevantPrePaid);
                bResp.m_sRecieptCode = lTransactionID.ToString();
                return bResp;
            }
            catch (Exception ex)
            {
                BillingResponse ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Proccessor error: " + ex.Message + " || " + ex.StackTrace;
                return ret;
            }
        }

        protected BillingResponse GetBillingResponse(APILogic.tikle.PurchaseResponseInfo resp, Int32 nTransactionLocalID, string sSiteGUID)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tikle_transactions");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TIKLE_STATUS", "=", resp.Result);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("TIKLE_REASON", "=", resp.ResultDetail);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTransactionLocalID);
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;


            BillingResponse ret = new BillingResponse();
            if (resp.Result == 0)
            {
                ret.m_oStatus = BillingResponseStatus.Success;
                ret.m_sRecieptCode = nTransactionLocalID.ToString();
                ret.m_sStatusDescription = "OK";
            }
            else
            {
                DeleteUserCreditCardDigits(sSiteGUID);
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = resp.ResultDetail;
            }
            return ret;
        }

        protected Int32 InsertNewTikleTransaction(string sSiteGUID, string sDigits, double dPrice, string sCurrency, string sCustomData)
        {
            Int32 nRet = 0;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("tikle_transactions");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from tikle_transactions where is_active=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        public string GetTransactionData(string sSiteGUID, UserBasicData userData, string sToken,
            double dPrice, string sCurrency, string sUserIP, string sCustomData, string sExtraParams, ref Int32 nTransactionLocalID)
        {

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sCustomData);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            string sType = Utils.GetSafeParValue(".", "type", ref theRequest);
            if (Utils.GetSafeParValue("//u", "id", ref theRequest) != sSiteGUID)
                return "";
            string sSubscriptionID = Utils.GetSafeValue("s", ref theRequest);
            string scouponcode = Utils.GetSafeValue("cc", ref theRequest);
            string sPayNum = Utils.GetSafeParValue("//p", "n", ref theRequest);
            string sPayOutOf = Utils.GetSafeParValue("//p", "o", ref theRequest);
            string smedia_file = Utils.GetSafeValue("mf", ref theRequest);
            string smedia_id = Utils.GetSafeValue("m", ref theRequest);
            string ssub = Utils.GetSafeValue("s", ref theRequest);
            string sppvmodule = Utils.GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = Utils.GetSafeValue("rs", ref theRequest);
            string smnou = Utils.GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = Utils.GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = Utils.GetSafeValue("vlcs", ref theRequest);
            string sir = Utils.GetSafeParValue("//p", "ir", ref theRequest);
            bool bIsRecurring = false;
            if (sir == "true")
                bIsRecurring = true;
            string sTransactionData = "<TransactionData>";
            sTransactionData += "<CustomerId>" + sSiteGUID + "</CustomerId>";
            sTransactionData += "<ProductId>" + smedia_id + "</ProductId>";
            sTransactionData += "<SubscriptionId>" + ssub + "</SubscriptionId>";

            if (sType == "sp")
            {
                if (bIsRecurring == true)
                {
                    if (smaxusagemodulelifecycle == "111111")
                        sTransactionData += "<SalesType>2</SalesType>";
                    else if (smaxusagemodulelifecycle == "10080")
                        sTransactionData += "<SalesType>1</SalesType>";
                    else
                        sTransactionData += "<SalesType>0</SalesType>";
                }
                else
                    sTransactionData += "<SalesType>0</SalesType>";
            }
            if (sType == "pp")
                sTransactionData += "<SalesType>0</SalesType>";

            sTransactionData += "<PaymentToken>" + sToken + "</PaymentToken>";
            sTransactionData += "<Msisdn></Msisdn>";
            sTransactionData += "<IsCargo>";
            if (String.IsNullOrEmpty(sExtraParams) == false && sExtraParams == "1")
                sTransactionData += "true";
            else
                sTransactionData += "false";
            sTransactionData += "</IsCargo>";
            sTransactionData += "<Price>" + dPrice.ToString() + "</Price>";
            sTransactionData += "<Currency>" + sCurrency + "</Currency>";
            sTransactionData += "<PromotionCoupon>" + scouponcode + "</PromotionCoupon>";
            sTransactionData += "<TransactionDate>" + DateTime.UtcNow.ToString("dd.MM.yyyy hh:mm:ss") + "</TransactionDate>";
            sTransactionData += "<CustomData>" + sCustomData + "</CustomData>";
            sTransactionData += "</TransactionData>";
            bool bSaved = true;
            string sLastDigits = GetUserDigits(sSiteGUID, ref bSaved);
            nTransactionLocalID = InsertNewTikleTransaction(sSiteGUID, sLastDigits, dPrice, sCurrency, sCustomData);
            return sTransactionData;
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            return Utils.GetHash(sRandom , "CLIENT_SECRET");
        }


        public override bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID)
        {
            return true;
        }
    }
}
