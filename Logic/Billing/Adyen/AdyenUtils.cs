using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using DAL;
using System.Data;
using System.Text.RegularExpressions;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using APILogic.AdyenRecAPI;

namespace Core.Billing
{
    public class AdyenUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Regex AdyenPSPReferenceRegex = new Regex(@"^\d+$");
        public enum BillingType
        {
            CreditCard,
            DirectDebit,
            PayPal
        }


        static public string GetSafeString(string val)
        {
            try
            {
                if (!string.IsNullOrEmpty(val))
                    return val.ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }



        public static void GetAccountCredentials(string merchant, ref string sUN, ref string sPass)
        {
            try
            {
                DataTable dt = AdyenDAL.Get_AccountCredentials(merchant);
                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    sUN = GetStrSafeVal(dt.Rows[0]["ws_user"]);
                    sPass = GetStrSafeVal(dt.Rows[0]["ws_pass"]);
                }
            }
            catch
            {
                //TODO
            }
        }


        public static void GetWSCredentials(int nGroupID, ref string userName, ref string password, ref string merchAccount)
        {

            try
            {
                DataTable dt = AdyenDAL.Get_WSCredentials(nGroupID, null);
                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    merchAccount = GetStrSafeVal(dt.Rows[0]["defaultMerchant"]);
                    userName = GetStrSafeVal(dt.Rows[0]["ws_user"]);
                    password = GetStrSafeVal(dt.Rows[0]["ws_pass"]);
                }
            }
            catch (Exception ex)
            {
                log.Error("Adyen_Logging - " + string.Format("Error on AdyenCreditCard.GetWSCredentials() , for GroupID {0} . ex.Message {1}", nGroupID, ex.ToString()), ex);
            }
        }

        /*Get ws_user, ws_pass, merchant by PurchesType */
        public static void GetWSCredentials(int nGroupID, ref string userName, ref string password, ref string merchDefaultAccount, ref string merchPurchesAccount, int? nPurchaseType)
        {
            try
            {
                log.Debug("Adyen_Logging - " + string.Format("Start AdyenCreditCard.GetWSCredentials() , for GroupID: {0} ", nGroupID));


                DataTable dt = AdyenDAL.Get_WSCredentials(nGroupID, nPurchaseType);
                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    merchDefaultAccount = GetStrSafeVal(dt.Rows[0]["defaultMerchant"]);
                    merchPurchesAccount = GetStrSafeVal(dt.Rows[0]["MerchantByPurchaseType"].ToString());
                    if (string.IsNullOrEmpty(merchPurchesAccount))
                        merchPurchesAccount = merchDefaultAccount;
                    userName = GetStrSafeVal(dt.Rows[0]["ws_user"]);
                    password = GetStrSafeVal(dt.Rows[0]["ws_pass"]);
                }

                log.Debug("Adyen_Logging - " + string.Format("Finished AdyenCreditCard.GetWSCredentials() , for GroupID: {0}, ref userName: {1} , ref password:{2}, ref merchPurchesAccount:{3}  ", nGroupID, userName, password, merchDefaultAccount));
            }
            catch (Exception ex)
            {
                log.Error("Adyen_Logging - " + string.Format("Error on AdyenCreditCard.GetWSCredentials() , for GroupID {0} . ex.Message {1}", nGroupID, ex.ToString()));
            }
        }

        public static void GetTvinciWSCredentials(ref string userName, ref string password, ref string merchAccount)
        {
            userName = TVinciShared.WS_Utils.GetTcmConfigValue("TvinciAdyenWS_User");
            password = TVinciShared.WS_Utils.GetTcmConfigValue("TvinciAdyenWS_Pass");
            merchAccount = TVinciShared.WS_Utils.GetTcmConfigValue("TvinciAdyenWS_MerchAccount");
        }

        public static void SendAdyenPurchaseMail(int nGroupID, string sCustomData, double dChargePrice, string sCurrencyCode, string sPaymentMethod, string sSiteGuid,
            long lBillingTransactionID, string sPSPReference, bool isFail)
        {
            try
            {
                log.Debug("Start - " + sCustomData);
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(sCustomData);
                System.Xml.XmlNode theRequest = doc.FirstChild;
                string sType = TVinciShared.XmlUtils.GetSafeParValue(".", "type", ref theRequest);
                log.Debug("Type - " + sType);
                if (sType.Equals("pp"))
                {
                    int nMediaID = 0;
                    string sMediaID = TVinciShared.XmlUtils.GetSafeValue("m", ref theRequest);
                    log.Debug("Media - " + sMediaID);
                    if (!string.IsNullOrEmpty(sMediaID))
                    {
                        nMediaID = int.Parse(sMediaID);
                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("media", "name", nMediaID, "MAIN_CONNECTION_STRING").ToString();
                        log.Debug("Name - " + sItemName);

                        if (!isFail)
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(),
                                sCurrencyCode, sPSPReference, nGroupID, string.Empty, eMailTemplateType.Purchase);
                        }
                        else
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(),
                                sCurrencyCode, sPSPReference, nGroupID, string.Empty, eMailTemplateType.PaymentFail);
                        }
                    }
                }
                else if (sType.Equals("sp"))
                {
                    int nSubID = 0;
                    string sSubscriptionID = TVinciShared.XmlUtils.GetSafeValue("s", ref theRequest);
                    string sPreivewEnd = TVinciShared.XmlUtils.GetSafeValue("prevlc", ref theRequest);
                    if (!string.IsNullOrEmpty(sSubscriptionID))
                    {
                        nSubID = int.Parse(sSubscriptionID);
                        log.Debug("Subscription ID - " + nSubID.ToString());
                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "name", nSubID, "pricing_connection").ToString();
                        log.Debug("Name - " + sItemName);
                        if (!isFail)
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, nGroupID, sPreivewEnd, eMailTemplateType.Purchase);
                        }
                        else
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, nGroupID, sPreivewEnd, eMailTemplateType.PaymentFail);
                        }
                    }
                }
                else if (sType.Equals("prepaid"))
                {
                    int nPrePaidCode = 0;
                    string sPrePaidCode = TVinciShared.XmlUtils.GetSafeValue("pp", ref theRequest);

                    if (!string.IsNullOrEmpty(sPrePaidCode))
                    {
                        nPrePaidCode = int.Parse(sPrePaidCode);
                        log.Debug("prepaid ID - " + sPrePaidCode);
                        string sItemName = ODBCWrapper.Utils.GetTableSingleVal("pre_paid_modules", "name", nPrePaidCode, "pricing_connection").ToString();
                        log.Debug("Name - " + sItemName);
                        if (!isFail)
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, nGroupID, string.Empty, eMailTemplateType.Purchase);
                        }
                        else
                        {
                            Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sSiteGuid, lBillingTransactionID, dChargePrice.ToString(), sCurrencyCode, sPSPReference, nGroupID, string.Empty, eMailTemplateType.PaymentFail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + ex.Message, ex);
            }

        }


        public static RecurringDetail GetRecurringDetail(RecurringDetail[] details, BillingType bt)
        {
            RecurringDetail retVal = null;
            if (details != null && details.Length == 1)
            {
                return details[0];
            }
            if (details != null)
            {
                var obj = from RecurringDetail t in details
                          orderby t.creationDate descending
                          select t;

                foreach (RecurringDetail det in obj)
                {
                    switch (bt)
                    {
                        case BillingType.CreditCard:
                            {
                                if (det.card != null)
                                {
                                    retVal = det;
                                }
                                break;
                            }
                        case BillingType.DirectDebit:
                            {
                                if (det.bank != null)
                                {
                                    retVal = det;
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    if (retVal != null)
                    {
                        break;
                    }
                }
            }
            return retVal;
        }

        public static RecurringDetail GetRecurringDetailByLastFourDigits(string sSiteGUID, RecurringDetail[] details, BillingType bt, string sLast4DigitsOrEmptyStringIfYouDontKnowIt)
        {
            RecurringDetail retVal = null;
            string lastFourDigits = string.Empty;
            if (sLast4DigitsOrEmptyStringIfYouDontKnowIt.Length == 4)
                lastFourDigits = sLast4DigitsOrEmptyStringIfYouDontKnowIt;
            else
                lastFourDigits = Core.Billing.Utils.GetLastTransactionFourDigits(sSiteGUID);

            if (details != null && details.Length == 1)
            {
                return details[0];
            }
            if (details != null)
            {
                var obj = from RecurringDetail t in details
                          orderby t.creationDate descending
                          select t;


                if (obj != null)
                {
                    foreach (RecurringDetail det in obj)
                    {
                        switch (bt)
                        {
                            case BillingType.CreditCard:
                                {
                                    if (det.card != null && det.card.number == lastFourDigits)
                                    {
                                        retVal = det;
                                    }
                                    break;
                                }
                            case BillingType.DirectDebit:
                                {
                                    if (det.bank != null && det.card != null && det.card.number == lastFourDigits)
                                    {
                                        retVal = det;
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        if (retVal != null)
                        {
                            break;
                        }
                    }

                    if (retVal == null) // If no card found by last 4 digits return the contract with the last creation date.
                    {
                        retVal = obj.ToList()[0];
                    }
                }
            }
            return retVal;
        }

        public static string GetWSRecurringUrl(int nGroupID)
        {
            string urlRes = string.Empty;
            try
            {
                log.Debug("Adyen_Logging - " + string.Format("Start AdyenCreditCard.GetWSRecurringUrl() , for GroupID: {0} ", nGroupID));

                DataTable dt = AdyenDAL.Get_WSRecurringUrl(nGroupID);
                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                    urlRes = GetStrSafeVal(dt.Rows[0]["ws_recurring_Url"]);

                log.Debug("Adyen_Logging - " + string.Format("Finished AdyenCreditCard.GetWSRecurringUrl() , for GroupID: {0} , urlRes: {1} ", nGroupID, urlRes));

            }
            catch (Exception ex)
            {
                log.Error("Adyen_Logging - " + string.Format("Error on AdyenCreditCard.GetWSRecurringUrl() , for GroupID {0} . ex.Message {1}", nGroupID, ex.ToString()), ex);
            }
            return urlRes;
        }


        public static string GetWSPaymentUrl(int nGroupID)
        {
            string urlRes = string.Empty;
            try
            {
                DataTable dt = AdyenDAL.Get_WSPaymentUrl(nGroupID);
                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                    urlRes = GetStrSafeVal(dt.Rows[0]["ws_payment_url"]);
            }
            catch
            {
                return string.Empty;
            }
            return urlRes;
        }

        private static string GetStrSafeVal(object val)
        {
            try
            {
                if (val != null && val != DBNull.Value)
                    return val.ToString();
                return string.Empty;

            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetItemID(string sCustomData)
        {
            string retVal = string.Empty;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sCustomData);
            System.Xml.XmlNode theRequest = doc.FirstChild;
            string sType = TVinciShared.XmlUtils.GetSafeParValue(".", "type", ref theRequest);
            log.Debug("Type - " + sType);
            string searchValue = "";

            switch (sType)
            {
                case "pp":
                    searchValue = "m";
                    break;
                case "sp":
                    searchValue = "s";
                    break;
                case "prepaid":
                    searchValue = "pp";
                    break;
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                retVal = TVinciShared.XmlUtils.GetSafeValue(searchValue, ref theRequest);
            }

            return retVal;
        }

        public static long GetAdyenPriceFormat(double price)
        {
            return (long)((decimal)price * 100);
        }

        public static bool? ConvertAdyenSuccess(string sSuccess)
        {
            bool? res = null;
            bool temp = false;
            if (sSuccess.Length > 0 && Boolean.TryParse(sSuccess, out temp))
                res = temp;
            return res;

        }

        public static bool IsValidPSPReference(string sPSPReference)
        {
            string sOverrideRegexFromConfig = string.Empty;

            if (TVinciShared.WS_Utils.GetTcmConfigValue("AdyenPSPReferenceRegexOverride") != string.Empty)
                sOverrideRegexFromConfig = TVinciShared.WS_Utils.GetTcmConfigValue("AdyenPSPReferenceRegexOverride");
            if (!string.IsNullOrEmpty(sOverrideRegexFromConfig))
            {
                try
                {
                    Regex r = new Regex(sOverrideRegexFromConfig);
                    if (r.IsMatch(sPSPReference))
                        return true;
                    return false;
                }
                catch
                {

                }
            }
            return !string.IsNullOrEmpty(sPSPReference) && AdyenPSPReferenceRegex.IsMatch(sPSPReference);
        }

    }
}
