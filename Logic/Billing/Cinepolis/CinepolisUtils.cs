using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using DAL;
using System.Threading;
using System.Web;
using KLogMonitor;
using System.Reflection;
using System.ServiceModel;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class CinepolisUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string CINEPOLIS_UTILS_LOG_FILENAME = "CinepolisUtils";
        internal const string CINEPOLIS_DUMMY = "true_cinepolis_dummy";

        public static bool TrySendOperationConfirm(long lPurchaseID, long lBillingTransactionID, ItemType bit,
            ref bool bIsSuccess, ref string sMessage, ref int nInternalCode)
        {
            bool res = false;

            string sAddress = Utils.GetValFromConfig("CinepolisOperationConfirmAddress");
            string sContentType = Utils.GetValFromConfig("CinepolisPostRequestContentType");
            if (sAddress.Length == 0 || sContentType.Length == 0)
            {
                // either address or content type retrieved from config is empty
                #region Logging
                log.Debug("TrySendOperationConfirm - "+ GetTrySendOperationConfirmStdErrMsg(string.Format("address or content type is empty. Address: {0} , Content type: {1}", sAddress, sContentType), lPurchaseID, lBillingTransactionID, bit, null));
                #endregion
                return false;
            }

            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>(3);
            lst.Add(new KeyValuePair<string, string>("tvinci_transaction_id", lBillingTransactionID + ""));
            lst.Add(new KeyValuePair<string, string>("tvinci_confirmation_id", String.Concat((int)bit, "_", lPurchaseID)));
            lst.Add(new KeyValuePair<string, string>("sh", CalcSecurityHash()));

            string sRequestData = TVinciShared.WS_Utils.BuildDelimiterSeperatedString(lst, "&", false, false);
            string sResponseJSON = string.Empty;
            string sErrorMsg = string.Empty;

            if (TVinciShared.WS_Utils.TrySendHttpPostRequest(sAddress, sRequestData, sContentType, Encoding.UTF8, ref sResponseJSON,
                ref sErrorMsg))
            {
                bool bIsParsingSuccessful = false;
                Dictionary<string, string> dict = TVinciShared.WS_Utils.TryParseJSONToDictionary(sResponseJSON, (new string[3] { "status", "internal_code", "message" }).ToList(), ref bIsParsingSuccessful, ref sErrorMsg);
                if (dict.ContainsKey("status"))
                {
                    if (dict["status"] != null && dict["status"].Trim().ToLower() == "ok")
                    {
                        bIsSuccess = true;
                    }
                    else
                    {
                        bIsSuccess = false;
                    }
                    if (dict.ContainsKey("internal_code") && dict["internal_code"] != null)
                        Int32.TryParse(dict["internal_code"], out nInternalCode);
                    if (dict.ContainsKey("message") && dict["message"] != null)
                        sMessage = dict["message"];
                    res = true;
                }
                else
                {
                    // no status in json. 
                    res = false;
                    #region Logging
                    log.Debug("TrySendOperationConfirm - "+ GetTrySendOperationConfirmStdErrMsg(string.Format("No status key in JSON. JSON: {0}", sResponseJSON), lPurchaseID, lBillingTransactionID, bit, dict));
                    #endregion
                }
            }
            else
            {
                // Failed to send request to cinepolis
                res = false;
                #region Logging
                log.Debug("TrySendOperationConfirm - "+ GetTrySendOperationConfirmStdErrMsg(string.Format("Post request to Cinepolis failed. Address: {0} , Content type: {1} , Request data: {2}", sAddress, sContentType, sRequestData), lPurchaseID, lBillingTransactionID, bit, null));
                #endregion
            }

            return res;
        }

        private static string GetTrySendOperationConfirmStdErrMsg(string sDescription, long lPurchaseID, long lBillingTransactionID,
            ItemType bit, Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sDescription, " , "));
            sb.Append(String.Concat("Purchase ID: ", lPurchaseID));
            sb.Append(String.Concat(" Billing Transaction ID: ", lBillingTransactionID));
            sb.Append(String.Concat(" Billing Item Type: ", bit.ToString()));

            if (dict != null && dict.Count > 0)
            {
                sb.Append(" JSON Dictionary: ");
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    sb.Append(String.Concat(kvp.Key, ":", kvp.Value, " "));
                }
            }

            return sb.ToString();
        }

        internal static string CalcSecurityHash()
        {
            string secret = string.Empty;
            if (!BillingDAL.Get_CinepolisSecret(string.Empty, ref secret))
                return string.Empty; // MD5 output is of length 16bytes (32 hexa chars), hence we can recognize failure by empty string
            return TVinciShared.HashUtils.GetMD5HashUTF8EncodingInHexaString(String.Concat(DateTime.UtcNow.ToString("yyyy-MM-dd"), secret));
        }

        public static void SendMail(ItemType ePurchaseType, Dictionary<string, string> oCustomDataDict, double dPrice,
            long lGroupID, CinepolisMailType cmt, long lBillingTransactionID)
        {
            // get here item name
            string sItemName = Utils.GetItemNameForPurchaseMail(ePurchaseType, oCustomDataDict);
            string sUsername = GetNameToAppearOnMail(oCustomDataDict[Constants.SITE_GUID], cmt);
            string sPurchaseDate = CinepolisUtils.GetPurchaseDateForPurchaseMail(lGroupID, cmt, lBillingTransactionID);
            string sPrice = dPrice + "";
            CinepolisMailObj oThreadDelegateArgumentsObj = new CinepolisMailObj(sUsername, sItemName, sPurchaseDate, sPrice, lGroupID, oCustomDataDict[Constants.SITE_GUID], cmt);
            Thread t = new Thread(new ParameterizedThreadStart(SendMailThreadDelegate));
            t.Start(oThreadDelegateArgumentsObj);

        }

        private static string GetNameToAppearOnMail(string sSiteGuid, CinepolisMailType cmt)
        {
            string res = string.Empty;
            long lSiteGuid = Int64.Parse(sSiteGuid);
            switch (cmt)
            {
                case CinepolisMailType.RenewalFail:
                    res = UsersDal.Get_FirstnameBySiteGuid(lSiteGuid, "USERS_CONNECTION_STRING");
                    break;
                default:
                    // all other mails are purchase success. there we need username.
                    res = UsersDal.Get_UsernameBySiteGuid(lSiteGuid, "USERS_CONNECTION_STRING");
                    break;
            }

            return res;
        }

        private static void SendMailThreadDelegate(object oMailObj)
        {
            try
            {
                if (oMailObj == null || !(oMailObj is CinepolisMailObj))
                {
                    throw new Exception("argument is null or is not of type CinepolisMailObj");
                }

                CinepolisMailObj cmo = (CinepolisMailObj)oMailObj;
                string sEmail = string.Empty;
                string sPurchaseMailTemplate = string.Empty;
                string sPurchaseMailSubject = string.Empty;
                string sMailFromName = string.Empty;
                string sMailFromAdd = string.Empty;
                string sMailServer = string.Empty;
                string sMailServerUN = string.Empty;
                string sMailServerPass = string.Empty;
                string sBccAddress = string.Empty;

                InitializeMailParameters(cmo.lGroupID, cmo.sSiteGuid, cmo.eCMT, ref sPurchaseMailTemplate, ref sPurchaseMailSubject,
                    ref sMailFromName, ref sMailFromAdd, ref sMailServer, ref sMailServerUN, ref sMailServerPass, ref sBccAddress);

                if (!UsersDal.Get_UserEmailBySiteGuid(Int64.Parse(cmo.sSiteGuid), "USERS_CONNECTION_STRING", ref sEmail) || !TVinciShared.Mailer.IsEmailAddressValid(sEmail))
                    throw new Exception("No user email extracted from DB");

                switch (cmo.eCMT)
                {
                    case CinepolisMailType.RenewalFail:
                        {
                            CinepolisRenewalFailMailRequest renewalFailRequest = new CinepolisRenewalFailMailRequest();
                            renewalFailRequest.m_sFirstName = cmo.sUsername;
                            renewalFailRequest.m_sItemName = cmo.sItemName;
                            renewalFailRequest.m_sPurchaseDate = cmo.sPurchaseDate;
                            renewalFailRequest.m_sSenderFrom = sMailFromAdd;
                            renewalFailRequest.m_sSenderName = sMailFromName;
                            renewalFailRequest.m_sSenderTo = sEmail;
                            renewalFailRequest.m_sTemplateName = sPurchaseMailTemplate;
                            renewalFailRequest.m_sSubject = sPurchaseMailSubject;
                            renewalFailRequest.m_eMailType = eMailTemplateType.PaymentFail;
                            ApiSendMailTemplateWrapper(renewalFailRequest, cmo.lGroupID);
                            break;
                        }
                    default:
                        {
                            // send purchase success mail
                            CinepolisPurchaseMailRequest purchaseRequest = new CinepolisPurchaseMailRequest();
                            purchaseRequest.m_sUsername = cmo.sUsername;
                            purchaseRequest.m_sItemName = cmo.sItemName;
                            purchaseRequest.m_sPurchaseDate = cmo.sPurchaseDate;
                            purchaseRequest.m_sPrice = cmo.sPrice;
                            purchaseRequest.m_sSenderFrom = sMailFromAdd;
                            purchaseRequest.m_sSenderName = sMailFromName;
                            purchaseRequest.m_sSenderTo = sEmail;
                            purchaseRequest.m_sTemplateName = sPurchaseMailTemplate;
                            purchaseRequest.m_sSubject = sPurchaseMailSubject;
                            purchaseRequest.m_eMailType = eMailTemplateType.Purchase;
                            ApiSendMailTemplateWrapper(purchaseRequest, cmo.lGroupID);
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception. Exception msg: ", ex.Message));
                if (oMailObj is CinepolisMailObj)
                {
                    sb.Append(oMailObj.ToString());
                }
                else
                {
                    sb.Append(String.Concat("oMailObj is: ", oMailObj != null ? oMailObj.GetType().Name : "null"));
                }
                sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));

                log.Debug("SendMailThreadDelegate - "+ sb.ToString());
                #endregion
            }
        }

        private static void ApiSendMailTemplateWrapper(MailRequestObj mro, long lGroupID)
        {
            bool resp = Api.Module.SendMailTemplate((int)lGroupID, mro);
            if (!resp)
            {
                // sending purchase mail probably failed.
                #region Logging
                StringBuilder sb = new StringBuilder("Probably failed to send purchase/renewal fail mail.");
                sb.Append(String.Concat(" User email: ", mro.m_sSenderTo));
                sb.Append(String.Concat(" Mail template: ", mro.GetType().Name));
                log.Debug("SendMailThreadDelegate - "+ sb.ToString());
                #endregion
            }
        }

        private static void InitializeMailParameters(long lGroupID, string sUserGUID, CinepolisMailType cmt, ref string sPurchaseMailTemplate, ref string sPurchaseMailSubject,
            ref string sMailFromName, ref string sMailFromAdd, ref string sMailServer, ref string sMailServerUN,
            ref string sMailServerPass, ref string sBccAddress)
        {
            string sMailName = string.Empty;
            string sMailSubject = string.Empty;
            GetMailAndMailSubject(cmt, ref sMailName, ref sMailSubject);
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from groups_parameters where status=1 and is_active=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr((int)lGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " order by id desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        object oPurchaseMail = selectQuery.Table("query").DefaultView[0].Row[sMailName];
                        object oMailFromName = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_NAME"];
                        object oMailFromAdd = selectQuery.Table("query").DefaultView[0].Row["MAIL_FROM_ADD"];
                        object oPurchaseMailSubject = selectQuery.Table("query").DefaultView[0].Row[sMailSubject];
                        object oMailServer = selectQuery.Table("query").DefaultView[0].Row["MAIL_SERVER"];
                        object oMailServerUN = selectQuery.Table("query").DefaultView[0].Row["MAIL_USER_NAME"];
                        object oMailServerPass = selectQuery.Table("query").DefaultView[0].Row["MAIL_PASSWORD"];
                        object oTaxVal = selectQuery.Table("query").DefaultView[0].Row["tax_value"];
                        object oLastInvoiceNum = selectQuery.Table("query").DefaultView[0].Row["last_invoice_num"];
                        object oBccAddress = selectQuery.Table("query").DefaultView[0].Row["bcc_address"];
                        if (oPurchaseMail != null && oPurchaseMail != DBNull.Value)
                            sPurchaseMailTemplate = oPurchaseMail.ToString();
                        if (oPurchaseMailSubject != null && oPurchaseMailSubject != DBNull.Value)
                            sPurchaseMailSubject = oPurchaseMailSubject.ToString();
                        if (oMailFromName != null && oMailFromName != DBNull.Value)
                            sMailFromName = oMailFromName.ToString();
                        if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                            sMailFromAdd = oMailFromAdd.ToString();
                        if (oMailServer != null && oMailServer != DBNull.Value)
                            sMailServer = oMailServer.ToString();
                        if (oMailServerUN != null && oMailServerUN != DBNull.Value)
                            sMailServerUN = oMailServerUN.ToString();
                        if (oMailServerPass != null && oMailServerPass != DBNull.Value)
                            sMailServerPass = oMailServerPass.ToString();
                        if (oBccAddress != null && oBccAddress != DBNull.Value)
                            sBccAddress = oBccAddress.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Disposing
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                    selectQuery = null;
                }
                #endregion
            }

        }

        private static void GetMailAndMailSubject(CinepolisMailType cmt, ref string sMailName, ref string sMailSubject)
        {
            switch (cmt)
            {
                case CinepolisMailType.RenewalFail:
                    sMailName = "PURCHASE_FAIL_MAIL";
                    sMailSubject = "PURCHASE_FAIL_MAIL_SUBJECT";
                    break;
                default:
                    sMailName = "PURCHASE_MAIL";
                    sMailSubject = "PURCHASE_MAIL_SUBJECT";
                    break;
            }
        }

        private static string GetPurchaseDateForPurchaseMail(long lGroupID, CinepolisMailType cmt, long lBillingTransactionID)
        {
            int hoursOffset = 0;
            DateTime res = DateTime.UtcNow;
            if (cmt == CinepolisMailType.RenewalFail)
            {
                res = ApiDAL.Get_PurchaseDateByBillingTransactionID(lBillingTransactionID);
                DateTime corruptedDate = new DateTime(2000, 1, 1);
                if (res.Equals(corruptedDate))
                    res = DateTime.UtcNow;
            }
            string sGMTOffset = Utils.GetWSURL(string.Format("GMTOffset_{0}", lGroupID.ToString()));
            if (sGMTOffset.Length > 0 && Int32.TryParse(sGMTOffset, out hoursOffset))
                return res.AddHours(hoursOffset).ToString("dd/MM/yyyy");
            return res.ToString("dd/MM/yyyy");
        }

        public static string GetIncomingRequestMsg(ref HttpContext context)
        {
            StringBuilder sb = new StringBuilder("Incoming request. ");
            if (context.Request != null)
            {
                sb.Append(String.Concat(" Request type: ", context.Request.RequestType));
                sb.Append(String.Concat(" ,Request content type: ", context.Request.ContentType));
                sb.Append(String.Concat(" ,Request form: ", context.Request.Form != null ? context.Request.Form.ToString() : "null"));
                sb.Append(String.Concat(" ,User host address: ", context.Request.UserHostAddress));
                sb.Append(String.Concat(" ,User host name: ", context.Request.UserHostName));
                sb.Append(String.Concat(" ,Server machine name: ", context.Server.MachineName));
                if (context.Request.Url != null)
                {
                    sb.Append(String.Concat(" ,Querystring: ", context.Request.Url.OriginalString));
                }
                else
                {
                    sb.Append(" ,No querystring");
                }
            }
            else
            {
                sb.Append("Request is null");
            }
            return sb.ToString();
        }

    }
}
