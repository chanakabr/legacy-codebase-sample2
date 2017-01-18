using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class TikleSMS : BaseSMS
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TikleSMS(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override BillingResponse CheckCode(string sSiteGUID, string sCellPhone, string sCode, string sReferenceCode)
        {
            throw new NotImplementedException();
        }

        protected string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
            }
            catch
            {
                return "";
            }
        }

        protected string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
            }
            catch
            {
                return "";
            }
        }

        public string GetTransactionData(string sSiteGUID, string sMSISDN,
            double dPrice, string sCurrency, string sCustomData, string sExtraParams, bool bIsRecurring)
        {

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sCustomData);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            string sType = GetSafeParValue(".", "type", ref theRequest);
            if (GetSafeParValue("//u", "id", ref theRequest) != sSiteGUID)
                return "";
            string sSubscriptionID = GetSafeValue("s", ref theRequest);
            string scouponcode = GetSafeValue("cc", ref theRequest);
            string sPayNum = GetSafeParValue("//p", "n", ref theRequest);
            string sir = Utils.GetSafeParValue("//p", "ir", ref theRequest);
            string sPayOutOf = GetSafeParValue("//p", "o", ref theRequest);
            string smedia_file = GetSafeValue("mf", ref theRequest);
            string smedia_id = GetSafeValue("m", ref theRequest);
            string ssub = GetSafeValue("s", ref theRequest);
            string sppvmodule = GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = GetSafeValue("rs", ref theRequest);
            string smnou = GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = GetSafeValue("vlcs", ref theRequest);


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
            sTransactionData += "<PaymentToken></PaymentToken>";
            sTransactionData += "<Msisdn>" + sMSISDN + "</Msisdn>";
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
            return sTransactionData;
        }

        protected string GetHash(string sToHash, string sHashParameterName)
        {
            string sSecret = ODBCWrapper.Utils.GetTableSingleVal("tikle_group_parameters", sHashParameterName, 1).ToString();
            sToHash += sSecret;

            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider = new MD5CryptoServiceProvider();
            byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToHash);
            byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        }

        public override BillingResponse SendCode(string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters)
        {
            Int32 nMediaFileID = 0;
            Int32 nMediaID = 0;
            string sSubscriptionCode = "";
            string sPPVCode = "";
            string sPriceCode = "";
            double dPrice = 0.0;
            string sCurrencyCode = "";
            bool bIsRecurring = false;
            string sPPVModuleCode = "";
            Int32 nNumberOfPayments = 0;

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

            Utils.SplitRefference(sReferenceCode, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode,
                ref sPPVCode, ref sRelevantPrePaid, ref sPriceCode, ref dPrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode,
                ref nNumberOfPayments, ref sUserGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                ref sCountryCd, ref sLanguageCode, ref sDeviceName);

            APILogic.tikle.Service s = new APILogic.tikle.Service();
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            s.Url = sTikleWSURL;
            string sTransactionData = GetTransactionData(sSiteGUID, sCellPhone, dPrice,
                sCurrencyCode, sReferenceCode, sExtraParameters, bIsRecurring);

            InsertSMSCode(sSubscriptionCode, nMediaFileID, sSiteGUID, "", sCellPhone,
                        sPPVCode, sPriceCode, dPrice, sCurrencyCode, sReferenceCode, 6);

            Int32 nAS = 0;
            Int32 nID = CheckSMSCode(ref sSubscriptionCode, ref nMediaFileID, sSiteGUID, "", sCellPhone,
                    ref sPPVCode, sPriceCode, dPrice, sCurrencyCode, false, ref nAS, ref sReferenceCode);

            log.Debug("send to tikle (SMS): " + sTransactionData);
            string sMD5Hash = Utils.GetHash(nID.ToString() + sTransactionData, "WS_SECRET");
            try
            {
                APILogic.tikle.PurchaseResponseInfo resp = s.Purchase(nID.ToString(), sTransactionData, sMD5Hash);
                log.Debug("returned from tikle: " + resp.Result.ToString() + " : " + resp.ResultDetail);
                BillingResponse ret = new BillingResponse();
                if (resp.Result == 0)
                    ret.m_oStatus = BillingResponseStatus.Success;
                else
                    ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                if (resp.Result != 0)
                    ret.m_sStatusDescription = resp.ResultDetail;
                return ret;
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
    }
}
