using ApiObjects.Response;
using DAL;
using KLogMonitor;
using M1BL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ApiObjects.Billing;
using System.Net;
using ApiObjects;
using Core.Pricing;
using Core.Users;

namespace Core.Billing
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        internal const int DEFAULT_PREVIEW_MODULE_NUM_OF_CANCEL_OR_REFUND_ATTEMPTS = 4;
        private const int BLOCK_SIZE = 16;

        private const string NO_CONFIGURATION_TO_UPDATE = "No configuration to update";
        private const string NO_CONFIGURATION_VALUE_UPDATE = "No configuration value to update";
        private const string OSS_ADAPTER_NOT_EXIST = "OSS adapter not exist";
        private const string PAYMENT_GATEWAY_NOT_EXIST = "Payment gateway not exist";

        //possible values for ENABLE_PAYMENT_GATEWAY_SELECTION ( true, yes, 1 : 1. false, no,0:0
        private static Dictionary<string, string> enablePaymentGatewayInputValues = new Dictionary<string, string>()
        { { "true", "1" }, { "yes", "1" }, { "1", "1" }, { "false", "0" }, { "no", "0" }, { "0", "0" } };

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseBilling t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);

            if (nGroupID != 0)
                Utils.GetBaseBillingImpl(ref t, nGroupID);
            else
                log.Debug("WS ignord - Function: " + sFunctionName);

            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCreditCard t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseCreditCardImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, string sPaymentMethodID, string sEncryptedCVV, ref BaseCreditCard t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseCreditCardImpl(ref t, nGroupID, sPaymentMethodID, sEncryptedCVV);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        static public Int32 GetDummyGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCreditCard t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                GetDummyCreditCardImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }



        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePopup t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBasePopupImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseSMS t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseSMSImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName + " UN: " + sWSUserName + " Pass: " + sWSPassword);
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseInAppPurchase t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseInAppPurchaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseDirectDebit t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseDirectDebitImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCellularCreditCard t)
        {
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
            if (nGroupID != 0)
                Utils.GetBaseCellularCreditCardImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - Function: " + sFunctionName);
            return nGroupID;
        }

        //static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePaymentGateway t)
        //{
        //    Int32 groupID = GetGroupID(sWSUserName, sWSPassword, sFunctionName);
        //    if (groupID != 0)
        //    {
        //        t = new Billing.BasePaymentGateway(groupID);
        //    }
        //    else
        //    {
        //        log.Debug("WS ignored - Function: " + sFunctionName);
        //    }
        //    return groupID;
        //}


        static public Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName)
        {
            ApiObjects.Credentials wsc = new ApiObjects.Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(ApiObjects.eWSModules.BILLING, wsc);

            if (nGroupID == 0)
            {
                log.Debug("WS ignored - Function: " + sFunctionName);
            }
            return nGroupID;
        }

        public static void SendMail(string sPaymentMethod, string sItemName, string sSiteGUID, long lBillingTransID, string stotalAmount, string scurrency, string sExternalNum, Int32 nGroupID, string sPreviewEnd, eMailTemplateType templateType)
        {
            SendMail(sPaymentMethod, sItemName, sSiteGUID, lBillingTransID, stotalAmount, scurrency, sExternalNum, nGroupID, string.Empty, sPreviewEnd, templateType);
        }

        public static void SendMail(string sPaymentMethod, string sItemName, string sSiteGUID, long lBillingTransID, string stotalAmount, string scurrency, string sExternalNum, 
            Int32 nGroupID, string sLast4Digits, string sPreviewEnd, eMailTemplateType templateType, eTransactionType? transactionType = null)
        {
            try
            {                
                if (!string.IsNullOrEmpty(sPreviewEnd) && templateType == eMailTemplateType.Purchase)
                    templateType = eMailTemplateType.PurchaseWithPreviewModule;
                //get HH from siteGuid
                User houseHoldUser = GetHHFromSiteGuid(sSiteGUID, nGroupID);
                MailRequestObj purchaseRequest = 
                    BillingMailTemplateFactory.GetMailTemplate(nGroupID, houseHoldUser.m_sSiteGUID, sExternalNum, double.Parse(stotalAmount),
                    scurrency, sItemName, sPaymentMethod, sLast4Digits, sPreviewEnd, templateType, lBillingTransID, houseHoldUser);
                log.DebugFormat("params for purchase mail ws_billing purchaseRequest.m_sSubject={0}, houseHoldUser.m_sSiteGUID={1}, purchaseRequest.m_sTemplateName={2}", purchaseRequest.m_sSubject, houseHoldUser.m_sSiteGUID, purchaseRequest.m_sTemplateName);
                
                if (purchaseRequest != null && !string.IsNullOrEmpty(purchaseRequest.m_sTemplateName))
                {
                    Api.Module.SendMailTemplate(nGroupID, purchaseRequest);
                }
            }
            catch (Exception ex)
            {
                log.Error("Send purchase mail - " + String.Concat("Exception. ", sSiteGUID, " | ", ex.Message, " | ", ex.StackTrace), ex);
            }
        }

        public static User GetHHFromSiteGuid(string siteGuid, int groupID)
        {
            User hhUser = null;

            try
            {
                UserResponseObject uObj = Core.Users.Module.GetUserData(groupID, siteGuid, string.Empty);
                if (uObj.m_RespStatus == ResponseStatus.OK && uObj.m_user != null)
                {
                    // if user is master 
                    if (uObj.m_user.m_isDomainMaster)
                    {
                        hhUser = uObj.m_user;
                    }
                    else
                    {
                        // get domainId details 
                        DomainResponse domain = Core.Domains.Module.GetDomainInfo(groupID, uObj.m_user.m_domianID);
                        // get first master user
                        if (domain.Status.Code == (int)eResponseStatus.OK && domain.Domain != null)
                        {
                            if (domain.Domain.m_masterGUIDs != null && domain.Domain.m_masterGUIDs.Count > 0)
                            {
                                int userID = domain.Domain.m_masterGUIDs[0];
                                // get master user details                                
                                uObj = Core.Users.Module.GetUserData(groupID, userID.ToString(), string.Empty);
                                if (uObj.m_RespStatus == ResponseStatus.OK && uObj.m_user != null)
                                {
                                    hhUser = uObj.m_user;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Get HH From SiteGuid - purchase mail - Exception={0}, SiteGUID={1}", ex.Message, siteGuid);
                hhUser = null;
            }
            return hhUser;
        }

        public static void GetCredentials(Int32 nGroupID, ref string sWSUserName, ref string sWSPass, ApiObjects.eWSModules eModule)
        {
            ApiObjects.Credentials oCredentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.BILLING, nGroupID, eModule);
            if (oCredentials != null)
            {
                sWSUserName = oCredentials.m_sUsername;
                sWSPass = oCredentials.m_sPassword;
            }
        }

        public static void SendMailToFixedAddress(string sPaymentMethod, string sItemName, string sSiteGUID,
            long lBillingTransID, string stotalAmount, string scurrency, string sExternalNum, Int32 nGroupID, string sFixedEmailAddress, eMailTemplateType templateType)
        {
            try
            {
                PurchaseMailRequest purchaseRequest = BillingMailTemplateFactory.GetMailTemplate(nGroupID, sSiteGUID, sExternalNum, 
                    double.Parse(stotalAmount), scurrency, sItemName, sPaymentMethod, string.Empty, string.Empty, templateType, lBillingTransID);


                if (purchaseRequest != null && !string.IsNullOrEmpty(purchaseRequest.m_sTemplateName))
                {
                    purchaseRequest.m_sSenderTo = sFixedEmailAddress;
                    Api.Module.SendMailTemplate(nGroupID, purchaseRequest);
                }
            }
            catch (Exception ex)
            {
                log.Error("Send purchase mail - " + ex.Message + " | " + ex.StackTrace, ex);
            }
        }

        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);

        }

        static public string GetSafeValue(string sQueryKey, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) == null ||
                    theRoot.SelectSingleNode(sQueryKey).FirstChild == null)
                {
                    return "";
                }

                return theRoot.SelectSingleNode(sQueryKey).FirstChild.Value;
            }
            catch
            {
                return "";
            }
        }

        static public string GetSafeParValue(string sQueryKey, string sParName, ref System.Xml.XmlNode theRoot)
        {
            try
            {
                if (theRoot.SelectSingleNode(sQueryKey) == null ||
                    theRoot.SelectSingleNode(sQueryKey).Attributes[sParName] == null)
                {
                    return "";
                }

                return theRoot.SelectSingleNode(sQueryKey).Attributes[sParName].Value;
            }
            catch
            {
                return "";
            }
        }

        static public Int32 GetCustomData(string sCustomData)
        {
            return (int)BillingDAL.Get_LatestCustomDataID(sCustomData);
        }

        static public string GetCustomData(long nCustomDataID)
        {
            string res = string.Empty;
            BillingDAL.Get_CustomDataByID(nCustomDataID, ref res);
            return res;
        }

        static public Int32 AddCustomData(string sCustomData)
        {
            Int32 nRet = GetCustomData(sCustomData);
            if (nRet == 0)
            {
                return (int)BillingDAL.Insert_NewCustomData(sCustomData);
            }
            return nRet;
        }

        static public void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref Int32 nMediaID, ref string sSubscriptionCode, ref string sPPVCode, ref string sPrePaidCode,
            ref string sPriceCode, ref double dPrice, ref string sCurrencyCd, ref bool bIsRecurring, ref string sPPVModuleCode,
            ref Int32 nNumberOfPayments, ref string sSiteGUID, ref string sRelevantSub, ref Int32 nMaxNumberOfUses,
            ref Int32 nMaxUsageModuleLifeCycle, ref Int32 nViewLifeCycleSecs, ref string sPurchaseType,
            ref string sCountryCd, ref string sLanguageCd, ref string sDeviceName)
        {
            string sPreviewModuleID = string.Empty;
            string sCollectionCode = string.Empty;
            SplitRefference(sRefference, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode, ref dPrice, ref sCurrencyCd, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sSiteGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType, ref sCurrencyCd, ref sLanguageCd, ref sDeviceName, ref sPreviewModuleID, ref sCollectionCode);
        }

        static public void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref Int32 nMediaID, ref string sSubscriptionCode, ref string sPPVCode, ref string sPrePaidCode,
            ref string sPriceCode, ref double dPrice, ref string sCurrencyCd, ref bool bIsRecurring, ref string sPPVModuleCode,
            ref Int32 nNumberOfPayments, ref string sSiteGUID, ref string sRelevantSub, ref Int32 nMaxNumberOfUses,
            ref Int32 nMaxUsageModuleLifeCycle, ref Int32 nViewLifeCycleSecs, ref string sPurchaseType,
            ref string sCountryCd, ref string sLanguageCd, ref string sDeviceName, ref string sPreviewModuleID, ref string sCollectionCode)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sRefference);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            bIsRecurring = false;
            string sType = Utils.GetSafeParValue(".", "type", ref theRequest);
            string sSubscriptionID = Utils.GetSafeValue("s", ref theRequest);
            string scouponcode = Utils.GetSafeValue("cc", ref theRequest);
            string sPayNum = Utils.GetSafeParValue("//p", "n", ref theRequest);
            string sPayOutOf = Utils.GetSafeParValue("//p", "o", ref theRequest);
            string suid = Utils.GetSafeParValue("//u", "id", ref theRequest);
            string smedia_file = GetSafeValue("mf", ref theRequest);
            string smedia_id = GetSafeValue("m", ref theRequest);
            string ssub = Utils.GetSafeValue("s", ref theRequest);
            string sPP = Utils.GetSafeValue("pp", ref theRequest);
            string sppvmodule = Utils.GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = Utils.GetSafeValue("rs", ref theRequest);
            string smnou = Utils.GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = Utils.GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = Utils.GetSafeValue("vlcs", ref theRequest);
            string sppvcode = Utils.GetSafeValue("ppvm", ref theRequest);
            string spc = Utils.GetSafeValue("pc", ref theRequest);
            string spri = Utils.GetSafeValue("pri", ref theRequest);
            string scur = Utils.GetSafeValue("cu", ref theRequest);
            string sir = Utils.GetSafeParValue("//p", "ir", ref theRequest);
            string srs = Utils.GetSafeValue("rs", ref theRequest);
            string pm = Utils.GetSafeValue("pm", ref theRequest);
            string collectioncode = Utils.GetSafeValue("cID", ref theRequest);
            string slcc = Utils.GetSafeValue("lcc", ref theRequest);
            if (!string.IsNullOrEmpty(slcc))
            {
                sCountryCd = slcc;
            }
            string sllc = Utils.GetSafeValue("llc", ref theRequest);
            sLanguageCd = sllc;
            string sldn = Utils.GetSafeValue("ldn", ref theRequest);
            sDeviceName = sldn;

            if (smedia_file != "")
                nMediaFileID = int.Parse(smedia_file);
            if (smedia_id != "")
                nMediaID = int.Parse(smedia_id);
            sSubscriptionCode = sSubscriptionID;
            sPPVCode = sppvcode;
            sPrePaidCode = sPP;
            sPriceCode = spc;
            if (spri != "")
                dPrice = double.Parse(spri);
            sCurrencyCd = scur;
            //if (sir == "true")
            //    bIsRecurring = true;
            sPPVModuleCode = sppvmodule;
            if (sPayOutOf != "")
                nNumberOfPayments = int.Parse(sPayOutOf);
            sSiteGUID = suid;
            sRelevantSub = srs;
            if (smnou != "")
                nMaxNumberOfUses = int.Parse(smnou);
            if (smaxusagemodulelifecycle != "")
                nMaxUsageModuleLifeCycle = int.Parse(smaxusagemodulelifecycle);
            if (sviewlifecyclesecs != "")
                nViewLifeCycleSecs = int.Parse(sviewlifecyclesecs);
            sPurchaseType = sType;
            sPreviewModuleID = pm;
            if (sir == "true")
                bIsRecurring = true;
            else
                bIsRecurring = !string.IsNullOrEmpty(sPreviewModuleID);
            sCollectionCode = collectioncode;
        }

        static public string GetHash(string sToHash, string sHashParameterName)
        {
            string sSecret = ODBCWrapper.Utils.GetTableSingleVal("tikle_group_parameters", sHashParameterName, 1).ToString();
            sToHash += sSecret;

            using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
            {
                byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToHash);
                byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
                return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
            }
        }



        static public void GetBaseCreditCardImpl(ref BaseCreditCard t, Int32 nGroupID)
        {
            GetBaseCreditCardImpl(ref t, nGroupID, string.Empty, string.Empty);
        }

        public static void GetBaseCreditCardImpl(ref BaseCreditCard p_oCreditCard, Int32 nGroupID, string sPaymentMethodID, string sEncryptedCVV)
        {
            Int32 nImplID = 0;

            string key = string.Format("{0}_GetBaseCreditCardImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 1);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select implementation_id from groups_modules_implementations where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }

            switch (nImplID)
            {
                case 1:
                    {
                        p_oCreditCard = new TvinciCreditCard(nGroupID);
                        break;
                    }
                case 5:
                    {
                        p_oCreditCard = new TikleCreditCard(nGroupID);
                        break;
                    }
                case 600:
                    {
                        p_oCreditCard = new AdyenCreditCard(nGroupID);
                        break;
                    }
                case 9:
                    {
                        p_oCreditCard = new CinepolisCreditCard(nGroupID, sPaymentMethodID, sEncryptedCVV);
                        break;
                    }
                case 50:
                    {
                        p_oCreditCard = new OfflineCreditCard(nGroupID);
                        break;
                    }
                case 601:
                    {
                        p_oCreditCard = new SmartSunCreditCard(nGroupID, sPaymentMethodID, sEncryptedCVV);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public static void GetDummyCreditCardImpl(ref BaseCreditCard t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetDummyCreditCardImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 1);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select implementation_id from groups_modules_implementations where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 1);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }

            if (nImplID == 9)
            {
                t = new Core.Billing.CinepolisDummyCreditCard(nGroupID);
            }
            else
            {
                if (nGroupID != 0)
                    t = new DummyCreditCard(nGroupID);
            }
        }

        static public void GetBaseDirectDebitImpl(ref BaseDirectDebit t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBaseDirectDebitImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 7);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 7);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }

            switch (nImplID)
            {
                case (1):
                    {
                        t = new Core.Billing.AdyenDirectDebit(nGroupID);
                        break;
                    }
                case (50):
                    {
                        t = new Core.Billing.OfflineDirectDebit(nGroupID);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        static public void GetBaseInAppPurchaseImpl(ref BaseInAppPurchase t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBaseInAppPurchaseImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 9);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 9);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }
            if (nImplID == 200)
            {
                t = new ElisaInAppPurchase(nGroupID);
            }
        }

        static public void GetBaseBillingImpl(ref BaseBilling t, Int32 nGgroupID)
        {
            int nImplID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "Select implementation_id from groups_modules_implementations where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGgroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 8);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
        }


        static public void GetBaseCellularCreditCardImpl(ref BaseCellularCreditCard t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBaseCellularCreditCardImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 10);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations(nolock) where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 10);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }
            if (nImplID == 1)
            {
                t = new M1CellularCreditCard(nGroupID);
            }
        }

        static public void GetBaseCellularDirectDebitImpl(ref BaseCellularDirectDebit t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBaseCellularDirectDebitImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 10);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations(nolock) where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 10);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                            if (nImplID > 0)
                            {
                                BillingCache.AddItem(key, nImplID);
                            }
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }
            if (nImplID == 1)
            {
                t = new Core.Billing.M1CellularDirectDebit(nGroupID);
            }
        }


        public static long InsertNewAdyenTransaction(Int32 nGroupID, string sSiteGUID, string sDigits, double dPrice,
                                                     string sCurrency, string sCustomDataID, string sCustomData, string pspReference, string sStatus,
                                                     string sBankName, string sAccountNumber, string sReason, string sErrCode,
                                                     Int32 nPaymentNum, Int32 nNumberOfPayments, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD,
                                                     Int32 nBillingProvider, int nType, ref int adyenTransactionID, bool checkDoubles, bool bIsCreatedByAdyenCallback)
        {
            Int32 nRet = 0;
            long lBillingTransactionID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.InsertQuery insertQuery = null;
            try
            {
                if (checkDoubles && IsDoubleAdyenTransaction(nGroupID, sSiteGUID, pspReference, sStatus))
                {
                    #region Write To Log
                    StringBuilder sb = new StringBuilder("Utils.InsertNewAdyenTransaction Double Transaction Detected: ");
                    sb.Append(String.Concat("Group ID: ", nGroupID, " "));
                    sb.Append(String.Concat("Site GUID: ", sSiteGUID, " "));
                    sb.Append(String.Concat("Digits: ", sDigits, " "));
                    sb.Append(String.Concat("Price: ", dPrice, " "));
                    sb.Append(String.Concat("Currency: ", sCurrency, " "));
                    sb.Append(String.Concat("CustomDataID: ", sCustomDataID, " "));
                    sb.Append(String.Concat("CustomData: ", sCustomData, " "));
                    sb.Append(String.Concat("PSP Reference: ", pspReference, " "));
                    sb.Append(String.Concat("Status: ", sStatus, " "));
                    sb.Append(String.Concat("Bank Name: ", sBankName, " "));
                    sb.Append(String.Concat("Account Number: ", sAccountNumber, " "));
                    sb.Append(String.Concat("Reason: ", sReason, " "));
                    sb.Append(String.Concat("Error Code: ", sErrCode, " "));
                    sb.Append(String.Concat("Payment Number: ", nPaymentNum, " "));
                    sb.Append(String.Concat("Number Of Payments: ", nNumberOfPayments, " "));
                    sb.Append(String.Concat("Billing Processor: ", nBILLING_PROCESSOR, " "));
                    sb.Append(String.Concat("Billing Method: ", nBILLING_METHOD, " "));
                    sb.Append(String.Concat("Billing Provider: ", nBillingProvider, " "));
                    sb.Append(String.Concat("Type: ", nType, " "));
                    log.Debug("Utils.InsertNewAdyenTransaction - " + sb.ToString());
                    #endregion
                    return -1;
                }

                #region Insert New Adyan Transaction
                DateTime dtToWriteToDB = DateTime.UtcNow;
                insertQuery = new ODBCWrapper.InsertQuery("adyen_transactions");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_CUSTOMDATA", "=", sCustomDataID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("psp_reference", "=", pspReference);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_STATUS", "=", sStatus);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("bank_name", "=", sBankName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("account_number", "=", sAccountNumber);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_REASON", "=", sReason);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", dtToWriteToDB);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_created_by_callback", "=", bIsCreatedByAdyenCallback ? 1 : 0);
                if (sErrCode != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_ERRCODE", "=", int.Parse(sErrCode));

                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("type", "=", nType);

                insertQuery.Execute();
                #endregion

                #region Get adyan transaction ID
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select id from adyen_transactions where is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("psp_reference", "=", pspReference);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_STATUS", "=", sStatus);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("bank_name", "=", sBankName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("account_number", "=", sAccountNumber);

                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_REASON", "=", sReason);

                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_created_by_callback", "=", bIsCreatedByAdyenCallback ? 1 : 0);
                if (sErrCode != "")
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ADYEN_ERRCODE", "=", int.Parse(sErrCode));
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

                }
                #endregion

                adyenTransactionID = nRet;
                Int32 nMediaFileID = 0;
                Int32 nMediaID = 0;
                string sSubscriptionCode = "";
                string sPPVCode = "";
                string sPriceCode = "";
                string sPPVModuleCode = "";
                bool bIsRecurring = false;
                string sCurrencyCode = "";
                double dChargePrice = 0.0;
                Int32 nStatus = 1;
                string sRelevantSub = "";
                string sUserGUID = "";
                Int32 nMaxNumberOfUses = 0;
                Int32 nMaxUsageModuleLifeCycle = 0;
                Int32 nViewLifeCycleSecs = 0;
                string sPurchaseType = "";
                string sPreviewModuleID = string.Empty;

                string sCountryCd = "";
                string sLanguageCode = "";
                string sDeviceName = "";
                string sPrePaidCode = string.Empty;
                string sCollectionCode = string.Empty;
                string sLoweredTrimmedStatus = sStatus.Trim().ToLower();
                if (sLoweredTrimmedStatus.StartsWith("pending") || sLoweredTrimmedStatus.StartsWith("authorised") || sLoweredTrimmedStatus.StartsWith("authorized") || sLoweredTrimmedStatus.StartsWith("success") || sLoweredTrimmedStatus.StartsWith("renewal") || sLoweredTrimmedStatus.StartsWith("charge"))
                    nStatus = 0;
                else
                    nStatus = 1;

                Core.Billing.Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                        ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                    ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                    ref sCountryCd, ref sLanguageCode, ref sDeviceName, ref sPreviewModuleID, ref sCollectionCode);

                lBillingTransactionID = Core.Billing.Utils.InsertBillingTransaction(sSiteGUID, sDigits, dChargePrice, sPriceCode,
                        sCurrencyCode, sCustomData, nStatus, sErrCode + "|" + sReason, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                        sSubscriptionCode, "", nGroupID, nBillingProvider, nRet, 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                        sCountryCd, sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPrePaidCode, sPreviewModuleID, sCollectionCode);
            }
            finally
            {
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return lBillingTransactionID;
        }


        public static long InsertNewM1Transaction(int nGroupID, string sSiteGUID, int nItemType, string sChargedMobileNumber, string sCustomerServiceID, double dPrice, int nCustomDataID, int nM1TransactionStatus, string sDigits,
                                                 string sCurrency, string sCustomData, int nBillingStatus, string sBankName, string sAccountNumber, string sReason, string sErrCode,
                                                 int nPaymentNum, int nNumberOfPayments, int nBillingProcessor, int nBillingMethod, int nBillingProvider, ref int nM1TransactionID)
        {

            nM1TransactionID = DAL.BillingDAL.Insert_M1Transaction(nGroupID, sSiteGUID, nItemType, sChargedMobileNumber, sCustomerServiceID, dPrice, nCustomDataID, nM1TransactionStatus);

            Int32 nMediaFileID = 0;
            Int32 nMediaID = 0;
            string sSubscriptionCode = "";
            string sPPVCode = "";
            string sPriceCode = "";
            string sPPVModuleCode = "";
            bool bIsRecurring = false;
            string sCurrencyCode = "";
            double dChargePrice = 0.0;
            string sRelevantSub = "";
            string sUserGUID = "";
            Int32 nMaxNumberOfUses = 0;
            Int32 nMaxUsageModuleLifeCycle = 0;
            Int32 nViewLifeCycleSecs = 0;
            string sPurchaseType = "";
            string sPreviewModuleID = string.Empty;
            string sCountryCd = "";
            string sLanguageCode = "";
            string sDeviceName = "";
            string sPrePaidCode = string.Empty;
            string sCollectionCode = string.Empty;

            Core.Billing.Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                                          ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                          ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                          ref sCountryCd, ref sLanguageCode, ref sDeviceName, ref sPreviewModuleID, ref sCollectionCode);

            long lBillingTransactionID = Core.Billing.Utils.InsertBillingTransaction(sSiteGUID, sDigits, dChargePrice, sPriceCode,
                                        sCurrencyCode, sCustomData, nBillingStatus, sErrCode + "|" + sReason, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                                        sSubscriptionCode, sChargedMobileNumber, nGroupID, nBillingProvider, nM1TransactionID, 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, sCustomerServiceID,
                                        sCountryCd, sLanguageCode, sDeviceName, nBillingProcessor, nBillingMethod, sPrePaidCode, sPreviewModuleID, sCollectionCode);

            return lBillingTransactionID;
        }


        private static bool IsDoubleAdyenTransaction(int nGroupID, string sSiteGuid, string sPspReference, string sStatus)
        {

            return BillingDAL.IsDoubleAdyenTransaction(nGroupID, sSiteGuid, sPspReference, sStatus);
        }

        internal static Int32 CheckExistInAppTransaction(string sReceiptData, Int32 nGroupID, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select id from InApp_transactions where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RECEIPT_DATA", "=", sReceiptData);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", sSiteGUID);
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
            }
            return nRet;
        }

        internal static Int32 CheckExistBillingTransaction(string sInAppTransactionID, Int32 nGroupID, string sSiteGUID)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select id from billing_transactions where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("BILLING_PROVIDER_REFFERENCE", "=", sInAppTransactionID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", sSiteGUID);

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
            }
            return nRet;
        }

        internal static long InsertInAppTransaction(Int32 nGroupID, string sSiteGUID, double dPrice, string sCurrency, string sReceiptData, Int32 nPaymentNum, Int32 nNumberOfPayments, string sCustomData)
        {

            Int32 nRet = 0;
            long lBillingTransactionID = 0;
            //check exesit receipt
            Int32 ExistInAppTransaction = 0;
            Int32 ExistBillingTransaction = 0;
            ODBCWrapper.InsertQuery insertQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                ExistInAppTransaction = CheckExistInAppTransaction(sReceiptData, nGroupID, sSiteGUID);

                if (ExistInAppTransaction == 0)
                {
                    #region Insert New InApp Transaction
                    insertQuery = new ODBCWrapper.InsertQuery("InApp_Transactions");
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RECEIPT_DATA", "=", sReceiptData);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);

                    insertQuery.Execute();

                    #endregion

                    #region Get last InApp transaction
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                    selectQuery += "select id from InApp_transactions where";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("RECEIPT_DATA", "=", sReceiptData);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMDATA", "=", sCustomData);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS", "=", 1);


                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount > 0)
                            nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

                    }

                    if (nRet == 0)
                    {
                        return -1;
                    }
                    #endregion

                    #region add billing transaction
                    Int32 nMediaFileID = 0;
                    Int32 nMediaID = 0;
                    string sSubscriptionCode = "";
                    string sPPVCode = "";
                    string sPriceCode = "";
                    string sPPVModuleCode = "";
                    bool bIsRecurring = false;
                    string sCurrencyCode = "";
                    double dChargePrice = 0.0;
                    Int32 nStatus = 0;
                    string sRelevantSub = "";
                    string sUserGUID = "";
                    Int32 nMaxNumberOfUses = 0;
                    Int32 nMaxUsageModuleLifeCycle = 0;
                    Int32 nViewLifeCycleSecs = 0;
                    string sPurchaseType = "";

                    string sCountryCd = "";
                    string sLanguageCode = "";
                    string sDeviceName = "";
                    string sPrePaidCode = string.Empty;

                    int nBILLING_PROCESSOR = 200;
                    int nBILLING_METHOD = 200;


                    Core.Billing.Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                            ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                        ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                        ref sCountryCd, ref sLanguageCode, ref sDeviceName);

                    lBillingTransactionID = Core.Billing.Utils.InsertBillingTransaction(sSiteGUID, string.Empty, dChargePrice, sPriceCode,
                            sCurrencyCode, sCustomData, nStatus, string.Empty, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                            sSubscriptionCode, "", nGroupID, (int)eBillingProvider.InApp, nRet, 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                            sCountryCd, sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPrePaidCode);
                    #endregion
                }
                else
                {
                    ExistBillingTransaction = CheckExistBillingTransaction(ExistInAppTransaction.ToString(), nGroupID, sSiteGUID);
                    if (ExistBillingTransaction == 0)
                    {
                        #region update billing tranasction with the exist in app transaction
                        Int32 nMediaFileID = 0;
                        Int32 nMediaID = 0;
                        string sSubscriptionCode = "";
                        string sPPVCode = "";
                        string sPriceCode = "";
                        string sPPVModuleCode = "";
                        bool bIsRecurring = false;
                        string sCurrencyCode = "";
                        double dChargePrice = 0.0;
                        Int32 nStatus = 0;
                        string sRelevantSub = "";
                        string sUserGUID = "";
                        Int32 nMaxNumberOfUses = 0;
                        Int32 nMaxUsageModuleLifeCycle = 0;
                        Int32 nViewLifeCycleSecs = 0;
                        string sPurchaseType = "";

                        string sCountryCd = "";
                        string sLanguageCode = "";
                        string sDeviceName = "";
                        string sPrePaidCode = string.Empty;

                        int nBILLING_PROCESSOR = 200;
                        int nBILLING_METHOD = 200;

                        Core.Billing.Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref sPrePaidCode, ref sPriceCode,
                                ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments, ref sUserGUID,
                                            ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                                            ref sCountryCd, ref sLanguageCode, ref sDeviceName);

                        lBillingTransactionID = Core.Billing.Utils.InsertBillingTransaction(sSiteGUID, string.Empty, dChargePrice, sPriceCode,
                                sCurrencyCode, sCustomData, nStatus, string.Empty, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                                sSubscriptionCode, "", nGroupID, (int)eBillingProvider.InApp, nRet, 0.0, dChargePrice, nPaymentNum, nNumberOfPayments, "",
                                sCountryCd, sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPrePaidCode);
                        #endregion
                    }
                    else
                    {
                        lBillingTransactionID = ExistBillingTransaction;
                    }
                }
            }
            finally
            {
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                }
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }

            return lBillingTransactionID;



        }

        public static Int32 InsertNewPlimusTransaction(Int32 nGroupID, string sSiteGUID, string sDigits, double dPrice, string sCurrency, string sCustomDataID, string sCustomData, string productId, string productName, string contractId, string contractName, string referenceNumber, string transactionType, string transactionDate, string paymentMethod, string paymentType, string creditCardType, string remoteAddress, string contractOwner, string creditCardLastFourDigits, string creditCardExpDate, string accountId, string firstName, string lastName, string CustomData, ref int plimusTransactionID, bool checkDoubles,
            string sReason, string sErrCode, Int32 nPaymentNum, Int32 nNumberOfPayments, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD, Int32 nBillingProvider, int nType)
        {
            Int32 nRet = 0;
            bool isOK = true;
            if (checkDoubles)
            {
                // Check Double transaction data.
                #region Check Doubls
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select id from plimus_transactions where is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_ID", "=", contractId);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_NAME", "=", productName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_NAME", "=", contractName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REFERENCE_NUMBER", "=", referenceNumber);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_TYPE", "=", transactionType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_DATAE", "=", transactionDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_METHOD", "=", paymentMethod);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_TYPE", "=", paymentType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_TYPE", "=", creditCardType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REMOTE_ADDRESS", "=", remoteAddress);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_OWNER", "=", contractOwner);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREADIT_CARD_LAST_FOUR_DIGITS", "=", creditCardLastFourDigits);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_EXPDATA", "=", creditCardExpDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_NAME", "=", firstName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_NAME", "=", lastName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMEDATA", "=", CustomData);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLIMUS_REASON", "=", sReason);
                if (sErrCode != "")
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLIMUS_ERRCODE", "=", int.Parse(sErrCode));
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        isOK = false;

                }



                selectQuery.Finish();
                selectQuery = null;
                #endregion
            }
            if (isOK)
            {
                #region Insert New plimus Transaction
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("plimus_transactions");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_ID", "=", productId);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_NAME", "=", productName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_ID", "=", contractId);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_NAME", "=", contractName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REFERENCE_NUMBER", "=", referenceNumber);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_TYPE", "=", transactionType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_DATAE", "=", transactionDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_METHOD", "=", paymentMethod);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_TYPE", "=", paymentType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_TYPE", "=", creditCardType);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("REMOTE_ADDRESS", "=", remoteAddress);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_OWNER", "=", contractOwner);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREADIT_CARD_LAST_FOUR_DIGITS", "=", creditCardLastFourDigits);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_EXPDATA", "=", creditCardExpDate);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_NAME", "=", firstName);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_NAME", "=", lastName);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMEDATA", "=", CustomData);
                if (sErrCode != "")
                    insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PLIMUS_ERRCODE", "=", int.Parse(sErrCode));

                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
                #endregion

                #region Get last plimus transaction
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select id from plimus_transactions where";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_ID", "=", productId);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRODUCT_NAME", "=", productName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_ID", "=", contractId);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_NAME", "=", contractName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REFERENCE_NUMBER", "=", referenceNumber);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_TYPE", "=", transactionType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("TRANSACTION_DATAE", "=", transactionDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_METHOD", "=", paymentMethod);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PAYMENT_TYPE", "=", paymentType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_TYPE", "=", creditCardType);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("REMOTE_ADDRESS", "=", remoteAddress);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CONTRACT_OWNER", "=", contractOwner);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREADIT_CARD_LAST_FOUR_DIGITS", "=", creditCardLastFourDigits);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CREDIT_CARD_EXPDATA", "=", creditCardExpDate);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("FIRST_NAME", "=", firstName);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_NAME", "=", lastName);
                //selectQuery += "and";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CUSTOMEDATA", "=", CustomData);

                if (sErrCode != "")
                {
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PLIMUS_ERRCODE", "=", int.Parse(sErrCode));
                }
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());

                }
                #endregion

                return nRet;
            }
            else
            {
                return -1;
            }

        }

        static public DateTime GetEndDateTime(DateTime dBase, Int32 nVal)
        {
            DateTime dRet = dBase;
            if (nVal == 1111111)
                dRet = dRet.AddMonths(1);
            else if (nVal == 2222222)
                dRet = dRet.AddMonths(2);
            else if (nVal == 3333333)
                dRet = dRet.AddMonths(3);
            else if (nVal == 4444444)
                dRet = dRet.AddMonths(4);
            else if (nVal == 5555555)
                dRet = dRet.AddMonths(5);
            else if (nVal == 6666666)
                dRet = dRet.AddMonths(6);
            else if (nVal == 9999999)
                dRet = dRet.AddMonths(9);
            else if (nVal == 11111111)
                dRet = dRet.AddYears(1);
            else if (nVal == 22222222)
                dRet = dRet.AddYears(2);
            else if (nVal == 33333333)
                dRet = dRet.AddYears(3);
            else if (nVal == 44444444)
                dRet = dRet.AddYears(4);
            else if (nVal == 55555555)
                dRet = dRet.AddYears(5);
            else if (nVal == 100000000)
                dRet = dRet.AddYears(10);
            else
                dRet = dRet.AddMinutes(nVal);
            return dRet;
        }

        static public void GetBasePopupImpl(ref BasePopup t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBasePopupImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 3);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from groups_modules_implementations where is_active=1 and status=1 and ";
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", 3);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                        if (nImplID > 0)
                        {
                            BillingCache.AddItem(key, nImplID);
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            if (nImplID == 4)
            {
                t = new Core.Billing.TvinciPopup(nGroupID);
            }
        }

        static public long InsertBillingTransaction(string sSITE_GUID, string sLAST_FOUR_DIGITS, double dPRICE,
    string sPRICE_CODE, string sCURRENCY_CODE, string sCUSTOMDATA, Int32 nBILLING_STATUS, string sBILLING_REASON,
    bool bIS_RECURRING, Int32 nMEDIA_FILE_ID, Int32 nMEDIA_ID, string sPPVMODULE_CODE,
    string sSUBSCRIPTION_CODE, string sCELL_PHONE, Int32 ngroup_id, Int32 nBILLING_PROVIDER,
    Int32 nBILLING_PROVIDER_REFFERENCE, double dPAYMENT_METHOD_ADDITION, double dTOTAL_PRICE,
    Int32 nPAYMENT_NUMBER, Int32 nNUMBER_OF_PAYMENTS, string sEXTRA_PARAMS,
    string sCountryCd, string sLanguageCode, string sDeviceName, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD, string sPrePaidCode)
        {
            return InsertBillingTransaction(sSITE_GUID, sLAST_FOUR_DIGITS, dPRICE,
            sPRICE_CODE, sCURRENCY_CODE, sCUSTOMDATA, nBILLING_STATUS, sBILLING_REASON,
            bIS_RECURRING, nMEDIA_FILE_ID, nMEDIA_ID, sPPVMODULE_CODE,
            sSUBSCRIPTION_CODE, sCELL_PHONE, ngroup_id, nBILLING_PROVIDER,
            nBILLING_PROVIDER_REFFERENCE, dPAYMENT_METHOD_ADDITION, dTOTAL_PRICE,
            nPAYMENT_NUMBER, nNUMBER_OF_PAYMENTS, sEXTRA_PARAMS,
            sCountryCd, sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPrePaidCode, string.Empty, string.Empty);
        }

        static public long InsertBillingTransaction(string sSITE_GUID, string sLAST_FOUR_DIGITS, double dPRICE,
            string sPRICE_CODE, string sCURRENCY_CODE, string sCUSTOMDATA, Int32 nBILLING_STATUS, string sBILLING_REASON,
            bool bIS_RECURRING, Int32 nMEDIA_FILE_ID, Int32 nMEDIA_ID, string sPPVMODULE_CODE,
            string sSUBSCRIPTION_CODE, string sCELL_PHONE, Int32 ngroup_id, Int32 nBILLING_PROVIDER,
            long lBILLING_PROVIDER_REFFERENCE, double dPAYMENT_METHOD_ADDITION, double dTOTAL_PRICE,
            Int32 nPAYMENT_NUMBER, Int32 nNUMBER_OF_PAYMENTS, string sEXTRA_PARAMS,
            string sCountryCd, string sLanguageCode, string sDeviceName, Int32 nBILLING_PROCESSOR, Int32 nBILLING_METHOD, string sPrePaidCode, string sPreviewModuleID, string sCollectionCode, string billingGuid = null)
        {
            long lPreviewModuleID = 0;
            if (!string.IsNullOrEmpty(sPreviewModuleID))
                Int64.TryParse(sPreviewModuleID, out lPreviewModuleID);
            double dPriceToWriteToDatabase = lPreviewModuleID > 0 ? 0.0 : dPRICE;
            double dTotalPriceToWriteToDatabase = lPreviewModuleID > 0 ? 0.0 : dTOTAL_PRICE;
            int nPaymentNumberToInsertToDB = CalcPaymentNumberForBillingTransactionsDBTable(nPAYMENT_NUMBER, lPreviewModuleID);



            long lRet = ApiDAL.Insert_NewBillingTransaction(sSITE_GUID, sLAST_FOUR_DIGITS, dPriceToWriteToDatabase, sPRICE_CODE, sCURRENCY_CODE,
            sCUSTOMDATA, nBILLING_STATUS, sBILLING_REASON, bIS_RECURRING, nMEDIA_FILE_ID, nMEDIA_ID, sPPVMODULE_CODE,
            sSUBSCRIPTION_CODE, sCELL_PHONE, ngroup_id, nBILLING_PROVIDER, lBILLING_PROVIDER_REFFERENCE,
            dPAYMENT_METHOD_ADDITION, dTotalPriceToWriteToDatabase, nPaymentNumberToInsertToDB, nNUMBER_OF_PAYMENTS, sEXTRA_PARAMS, sCountryCd,
            sLanguageCode, sDeviceName, nBILLING_PROCESSOR, nBILLING_METHOD, sPrePaidCode, lPreviewModuleID, sCollectionCode, billingGuid);

            return lRet;
        }

        static public void GetBaseSMSImpl(ref BaseSMS t, Int32 nGroupID)
        {
            Int32 nImplID = 0;
            string key = string.Format("{0}_GetBaseSMSImpl_{1}_{2}", ApiObjects.eWSModules.BILLING, nGroupID, 2);
            bool bRes = BillingCache.GetItem<Int32>(key, out nImplID);
            if (!bRes)
            {
                nImplID = DAL.BillingDAL.GetModuleImplementationID(nGroupID, 2);
                if (nImplID > 0)
                {
                    BillingCache.AddItem(key, nImplID);
                }
            }

            if (nImplID == 2)
            {
                t = new WinPLCSMS(nGroupID);
            }
            if (nImplID == 3)
            {
                t = new _999SMS(nGroupID);
            }
            if (nImplID == 6)
            {
                t = new Core.Billing.TikleSMS(nGroupID);
            }
        }

        static public void GetAppleValidationReceitsURL(ref string URL, ref string InAppToken, ref string InAppSharedSecret, Int32 nGroupID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                selectQuery += "select * from [groups_parameters] where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        /*
                        Only for test -- 
                        URL = "https://   sandbox.itunes.apple.com/verifyReceipt";*/
                        URL = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "InAppURL", 0);
                        InAppToken = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "InAppToken", 0);
                        InAppSharedSecret = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "InAppSharedSecret", 0);
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
        }

        static public string GetLastTransactionFourDigits(string sSiteGUID)
        {
            string retLastFourDigits = string.Empty;
            object objLastFourDigits = ApiDAL.Get_LastTransactionFourDigits(sSiteGUID);
            if (objLastFourDigits != null)
            {
                retLastFourDigits = objLastFourDigits.ToString();
            }
            return retLastFourDigits;
        }

        public static string GetStrSafeVal(DataRow dr, string sFiled)
        {
            try
            {
                if (dr != null && dr[sFiled] != DBNull.Value)
                    return dr[sFiled].ToString();
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int GetIntSafeVal(DataRow dr, string sFiled)
        {
            try
            {
                if (dr != null && dr[sFiled] != DBNull.Value)
                    return int.Parse(dr[sFiled].ToString());
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        public static PreviewModule GetPreviewModuleByID(int nGroupID, long lPreviewModuleID)
        {
            PreviewModule res = null;
            try
            {
                res = Pricing.Module.GetPreviewModuleByID(nGroupID, lPreviewModuleID);
            }
            catch (Exception ex)
            {
                log.Error("Utils.WS GetPreviewModuleByID() - " + string.Format("Utils.WS GetPreviewModuleByID() failed, Exception: {0} , nGroupID : {1} , lPreviewModuleID : {2}", ex.ToString(), nGroupID, lPreviewModuleID), ex);
                res = null;
            }
            return res;
        }

        /*
        * 1. This function is used when inserting record into billing transactions table.
        * 2. If the user bought an MPP with preview module zero will be inserted into the DB.
        * 3. The payment number is critical for the MPP renewing process.
        * 
        */
        private static int CalcPaymentNumberForBillingTransactionsDBTable(int nPaymentNumber, long lPreviewModuleID)
        {
            if (nPaymentNumber == 1 && lPreviewModuleID > 0)
                return 0;
            return nPaymentNumber;
        }

        public static int GetPreviewModuleNumOfCancelOrRefundAttempts()
        {
            int res = DEFAULT_PREVIEW_MODULE_NUM_OF_CANCEL_OR_REFUND_ATTEMPTS;
            if (TVinciShared.WS_Utils.GetTcmConfigValue("PreviewModuleNumOfCancelOrRefundAttempts") != string.Empty)
                Int32.TryParse(TVinciShared.WS_Utils.GetTcmConfigValue("PreviewModuleNumOfCancelOrRefundAttempts"), out res);
            return res;
        }

        public static string GetPaymentMethod(int nBillingMethod)
        {
            ePaymentMethod epm = (ePaymentMethod)nBillingMethod;
            return epm.ToString();
        }

        internal static string GetPreviewModuleItemName(int nGroupID)
        {
            string res = string.Empty;
            string sKey = String.Concat("ItemNameForPreviewModuleMail_", nGroupID);
            if (TVinciShared.WS_Utils.GetTcmConfigValue(sKey) != string.Empty)
                res = TVinciShared.WS_Utils.GetTcmConfigValue(sKey);

            return res;
        }

        public static Dictionary<string, string> GetCustomDataDictionary(string sCustomData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sCustomData);
            XmlNode theRequest = doc.FirstChild;
            Dictionary<string, string> res = new Dictionary<string, string>();

            res.Add(Constants.BUSINESS_MODULE_TYPE, GetSafeParValue(".", Constants.BUSINESS_MODULE_TYPE, ref theRequest));
            res.Add(Constants.SITE_GUID, GetSafeParValue("//u", Constants.SITE_GUID, ref theRequest));
            res.Add(Constants.SUBSCRIPTION_ID, GetSafeValue(Constants.SUBSCRIPTION_ID, ref theRequest));
            res.Add(Constants.PRE_PAID_ID, GetSafeValue(Constants.PRE_PAID_ID, ref theRequest));
            res.Add(Constants.PRE_PAID_CREDIT_VALUE, GetSafeValue(Constants.PRE_PAID_CREDIT_VALUE, ref theRequest));
            res.Add(Constants.COUPON_CODE, GetSafeValue(Constants.COUPON_CODE, ref theRequest));
            res.Add(Constants.PAYMENT_NUMBER, GetSafeParValue("//p", Constants.PAYMENT_NUMBER, ref theRequest));
            res.Add(Constants.NUMBER_OF_PAYMENTS, GetSafeParValue("//p", Constants.NUMBER_OF_PAYMENTS, ref theRequest));
            res.Add(Constants.IS_RECURRING, GetSafeParValue("//p", Constants.IS_RECURRING, ref theRequest));
            res.Add(Constants.MEDIA_FILE, GetSafeValue(Constants.MEDIA_FILE, ref theRequest));
            res.Add(Constants.PPV_MODULE, GetSafeValue(Constants.PPV_MODULE, ref theRequest));
            res.Add(Constants.RELEVANT_SUBSCRIPTION, GetSafeValue(Constants.RELEVANT_SUBSCRIPTION, ref theRequest));
            res.Add(Constants.MAX_NUM_OF_USES, GetSafeValue(Constants.MAX_NUM_OF_USES, ref theRequest));
            res.Add(Constants.COUNTRY_CODE, GetSafeValue(Constants.COUNTRY_CODE, ref theRequest));
            res.Add(Constants.LANGUAGE_CODE, GetSafeValue(Constants.LANGUAGE_CODE, ref theRequest));
            res.Add(Constants.DEVICE_NAME, GetSafeValue(Constants.DEVICE_NAME, ref theRequest));
            res.Add(Constants.MAX_USAGE_MODULE_LIFE_CYCLE, GetSafeValue(Constants.MAX_USAGE_MODULE_LIFE_CYCLE, ref theRequest));
            res.Add(Constants.VIEW_LIFE_CYCLE_SECS, GetSafeValue(Constants.VIEW_LIFE_CYCLE_SECS, ref theRequest));
            res.Add(Constants.CC_DIGITS, GetSafeValue(Constants.CC_DIGITS, ref theRequest));
            string sPrice = GetSafeValue(Constants.PRICE, ref theRequest);
            if (string.IsNullOrEmpty(sPrice))
                res.Add(Constants.PRICE, "0.0");
            else
                res.Add(Constants.PRICE, sPrice);
            res.Add(Constants.CURRENCY, GetSafeValue(Constants.CURRENCY, ref theRequest));
            res.Add(Constants.USER_IP, GetSafeValue(Constants.USER_IP, ref theRequest));
            res.Add(Constants.CAMPAIGN_CODE, GetSafeValue(Constants.CAMPAIGN_CODE, ref theRequest));
            res.Add(Constants.CAMPAIGN_MAX_NUM_OF_USES, GetSafeValue(Constants.CAMPAIGN_MAX_NUM_OF_USES, ref theRequest));
            res.Add(Constants.CAMPAIGN_MAX_LIFE_CYCLE, GetSafeValue(Constants.CAMPAIGN_MAX_LIFE_CYCLE, ref theRequest));
            res.Add(Constants.OVERRIDE_END_DATE, GetSafeValue(Constants.OVERRIDE_END_DATE, ref theRequest));
            res.Add(Constants.PREVIEW_MODULE, GetSafeValue(Constants.PREVIEW_MODULE, ref theRequest));
            res.Add(Constants.PRICE_CODE, GetSafeValue(Constants.PRICE_CODE, ref theRequest));
            res.Add(Constants.MEDIA_ID, GetSafeValue(Constants.MEDIA_ID, ref theRequest));

            return res;

        }

        /*
         * 1. returns the id in TVinci.billing_transactions
         * 2. places in lCinepolisTransactionID the id in Billing.cinepolis_transactions
         */
        public static long InsertNewCinepolisTransaction(long lGroupID, long lSiteGuid, double dPrice, string sCurrency,
            long lCustomDataID, string sCustomData, Dictionary<string, string> oCustomDataDict, string sBankAuthorisationID,
            byte bytTransactionStatus, int nPaymentNum, int nNumberOfPayments, int nBillingProcessor,
            int nBillingMethod, int nBillingProvider, int nType, byte bytConfirmationSuccess, string sErrorMsg,
            ref long lCinepolisTransactionID, bool bIsCheckDoubles)
        {
            if (bIsCheckDoubles && IsDoubleCinepolisTransaction(lSiteGuid, lGroupID, dPrice, lCustomDataID, sBankAuthorisationID,
                bytTransactionStatus, nType))
            {
                // double cinepolis transaction. no need to update db.
                return -1;
            }
            else
            {
                // insert to Billing.cinepolis_transactions table
                lCinepolisTransactionID = BillingDAL.Insert_NewCinepolisTransaction(lSiteGuid, dPrice, sCurrency,
                    sBankAuthorisationID, bytTransactionStatus, lCustomDataID, lGroupID, true, 1, null,
                    0, nType, bytConfirmationSuccess, 0, string.Empty, string.Empty);

                if (lCinepolisTransactionID == 0)
                {
                    // failed to insert into cinepolis transactions
                    return -2;
                }
                //insert to TVinci.billing_transactions table

                bool bIsRecurring = CalcIsRecurringBool(oCustomDataDict);
                int nMediaFileID = ParseIntIfNotEmpty(oCustomDataDict[Constants.MEDIA_FILE]);
                int nMediaID = ParseIntIfNotEmpty(oCustomDataDict[Constants.MEDIA_ID]);
                int nBillingStatus = 0;
                if (Enum.IsDefined(typeof(CinepolisTransactionStatus), bytTransactionStatus))
                {
                    CinepolisTransactionStatus cts = (CinepolisTransactionStatus)bytTransactionStatus;
                    switch (cts)
                    {
                        case CinepolisTransactionStatus.Refused:
                            nBillingStatus = 1;
                            break;
                        default:
                            nBillingStatus = 0;
                            break;
                    }
                }

                return InsertBillingTransaction(lSiteGuid + "", string.Empty, dPrice, oCustomDataDict[Constants.PRICE_CODE],
                    oCustomDataDict[Constants.CURRENCY], sCustomData, nBillingStatus, string.Empty, bIsRecurring, nMediaFileID,
                    nMediaID, oCustomDataDict[Constants.PPV_MODULE], oCustomDataDict[Constants.SUBSCRIPTION_ID], string.Empty,
                    (int)lGroupID, nBillingProvider, (int)lCinepolisTransactionID, 0.0, dPrice, nPaymentNum, nNumberOfPayments,
                    string.Empty, oCustomDataDict[Constants.COUNTRY_CODE], oCustomDataDict[Constants.LANGUAGE_CODE],
                    oCustomDataDict[Constants.DEVICE_NAME], nBillingProcessor, nBillingMethod, string.Empty, oCustomDataDict[Constants.PREVIEW_MODULE], string.Empty);
            }

        }

        public static bool CalcIsRecurringBool(Dictionary<string, string> oCustomDataDict)
        {
            string temp = oCustomDataDict[Constants.IS_RECURRING];
            if (!string.IsNullOrEmpty(temp) && temp.Trim().ToLower() == "true")
                return true;
            return !string.IsNullOrEmpty(oCustomDataDict[Constants.PREVIEW_MODULE]);
        }

        public static int ParseIntIfNotEmpty(string sStrToParse)
        {
            if (sStrToParse.Length > 0)
                return Int32.Parse(sStrToParse);
            return 0;
        }

        public static double ParseDoubleIfNotEmpty(string sStrToParse)
        {
            if (sStrToParse.Length > 0)
                return Double.Parse(sStrToParse);
            return 0.0;
        }

        public static long ParseLongIfNotEmpty(string sStrToParse)
        {
            if (sStrToParse.Length > 0)
                return Int64.Parse(sStrToParse);
            return 0;
        }

        private static bool IsDoubleCinepolisTransaction(long lSiteGuid, long lGroupID, double dPrice,
            long lCustomDataID, string sBankAuthorisationID, byte bytTransactionStatus, int nType)
        {
            return BillingDAL.Get_CinepolisTransactionID(lSiteGuid, dPrice, sBankAuthorisationID, bytTransactionStatus,
                lCustomDataID, lGroupID, true, 1, nType) > 0;
        }

        public static void WriteUserLogAsync(int groupID, string siteGuid, string msg)
        {
            Task.Factory.StartNew(() => TryWriteToUserLog(groupID, siteGuid, msg));
        }

        public static bool TryWriteToUserLog(int nGroupID, string sSiteGUID, string sMessage)
        {
            bool res = true;
            try
            {
                Core.Users.Module.WriteLog(nGroupID, sSiteGUID, sMessage, "Billing Module");
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Failed to write to user log. ");
                sb.Append(String.Concat(" U: ", sSiteGUID));
                sb.Append(String.Concat(" G ID: ", nGroupID));
                sb.Append(String.Concat(" Log Msg: ", sMessage));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                res = false;
            }

            return res;
        }

        public static long HandleSubscriptionPurchase(Dictionary<string, string> oCustomDataDict, long lBillingTransactionID,
            long lBillingProviderTransactionID, long lGroupID, double dPrice, string sCustomData)
        {

            long lPreviewModuleID = 0;
            if (!string.IsNullOrEmpty(oCustomDataDict[Constants.PREVIEW_MODULE]))
                Int64.TryParse(oCustomDataDict[Constants.PREVIEW_MODULE], out lPreviewModuleID);
            double dPriceToWriteToDB = lPreviewModuleID > 0 ? 0.0 : dPrice;
            long lMaxNumOfUses = ParseLongIfNotEmpty(oCustomDataDict[Constants.MAX_NUM_OF_USES]);
            long lViewLifeCycleSecs = ParseLongIfNotEmpty(oCustomDataDict[Constants.VIEW_LIFE_CYCLE_SECS]);
            long lMaxUsageModuleLifeCycle = ParseLongIfNotEmpty(oCustomDataDict[Constants.MAX_USAGE_MODULE_LIFE_CYCLE]);
            bool bIsRecurringStatus = !string.IsNullOrEmpty(oCustomDataDict[Constants.IS_RECURRING]) && oCustomDataDict[Constants.IS_RECURRING].Trim().ToLower() == "true";
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtEndDate = CalcSubscriptionEndDate(lGroupID, oCustomDataDict[Constants.OVERRIDE_END_DATE], lMaxUsageModuleLifeCycle, lPreviewModuleID, dtUtcNow);

            long lSubscriptionPurchaseID = ConditionalAccessDAL.Insert_NewMPPPurchase(lGroupID,
                oCustomDataDict[Constants.SUBSCRIPTION_ID], oCustomDataDict[Constants.SITE_GUID], dPrice,
                oCustomDataDict[Constants.CURRENCY], sCustomData, oCustomDataDict[Constants.COUNTRY_CODE],
                oCustomDataDict[Constants.LANGUAGE_CODE], oCustomDataDict[Constants.DEVICE_NAME], lMaxNumOfUses,
                lViewLifeCycleSecs, bIsRecurringStatus, lBillingTransactionID, lPreviewModuleID, dtUtcNow, dtEndDate,
                dtUtcNow, "CA_CONNECTION_STRING", int.Parse(oCustomDataDict[Constants.DOMAIN]));

            if (lSubscriptionPurchaseID > 0)
            {
                if (lBillingProviderTransactionID > 0)
                {
                    // update purchase id in Billing.cinepolis_transactions table
                    BillingDAL.Update_PurchaseIDInCinepolisTransactions(lBillingProviderTransactionID, lSubscriptionPurchaseID);
                }
                if (lBillingTransactionID > 0)
                {
                    // update purchase id in TVinci.billing_transactions
                    if (!ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lSubscriptionPurchaseID))
                    {
                        // failed to write purchase id to billing_transactions. it is critical for the mpp
                        // renewer process.
                        #region Logging
                        log.Debug("HandleSubscriptionPurchase - " + string.Format("Failed to write purchase id to billing transactions. Site Guid: {0} , Billing transaction id: {1} , Purchase ID: {2} , Billing provider id: {3}", oCustomDataDict[Constants.SITE_GUID], lBillingTransactionID, lSubscriptionPurchaseID, lBillingProviderTransactionID));
                        #endregion
                    }
                }
            }

            // handle coupon use.
            long lSubscriptionID = ParseLongIfNotEmpty(oCustomDataDict[Constants.SUBSCRIPTION_ID]);
            HandleCouponUse(oCustomDataDict[Constants.COUPON_CODE], oCustomDataDict[Constants.SITE_GUID], 0, lSubscriptionID,
                0, lGroupID);

            return lSubscriptionPurchaseID;
        }

        private static DateTime CalcSubscriptionEndDate(long lGroupID, string sOverrideEndDate, long lMaxUsageModuleLifeCycle, long lPreviewModuleID,
            DateTime dtBaseDate)
        {
            DateTime res = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(sOverrideEndDate))
            {
                try
                {
                    res = DateTime.ParseExact(sOverrideEndDate, "dd/MM/yyyy", null);
                    return res;
                }
                catch
                {
                }

            }

            if (lPreviewModuleID > 0)
            {
                PreviewModule pm = GetPreviewModuleByID((int)lGroupID, lPreviewModuleID);
                if (pm != null && pm.m_tsFullLifeCycle > 0)
                {
                    res = GetEndDateTime(dtBaseDate, pm.m_tsFullLifeCycle);
                    return res;
                }

            }
            if (lMaxUsageModuleLifeCycle > 0)
                res = GetEndDateTime(dtBaseDate, (int)lMaxUsageModuleLifeCycle);

            return res;

        }

        private static void HandleCouponUse(string sCouponCode, string sSiteGUID, long lMediaFileID, long lSubscriptionID, long lPrePaidID, long lGroupID)
        {
            if (!string.IsNullOrEmpty(sCouponCode))
            {
                log.Debug("HandleCouponUse - " + string.Format("User: {0} from group id: {1} used coupon: {2}", sSiteGUID, lGroupID, sCouponCode));
                PricingDAL.Handle_CouponUse(sCouponCode, sSiteGUID, lGroupID, lMediaFileID, lSubscriptionID, lPrePaidID,
                    1);
            }
        }

        public static long HandlePPVPurchase(Dictionary<string, string> oCustomDataDict, long lBillingTransactionID,
            long lBillingProviderTransactionID, long lGroupID, double dPrice, string sCustomData)
        {
            long lMediaFileID = ParseLongIfNotEmpty(oCustomDataDict[Constants.MEDIA_FILE]);
            long lMaxNumOfUses = ParseLongIfNotEmpty(oCustomDataDict[Constants.MAX_NUM_OF_USES]);
            long lMaxUsageModuleLifeCycle = ParseLongIfNotEmpty(oCustomDataDict[Constants.MAX_USAGE_MODULE_LIFE_CYCLE]);
            DateTime dtUtcNow = DateTime.UtcNow;
            DateTime dtEndDate = CalcPPVEndDate(dtUtcNow, lMaxUsageModuleLifeCycle, oCustomDataDict[Constants.OVERRIDE_END_DATE]);
            long lPPVPurchaseID = ConditionalAccessDAL.Insert_NewPPVPurchase(lGroupID, lMediaFileID,
                oCustomDataDict[Constants.SITE_GUID], dPrice, oCustomDataDict[Constants.CURRENCY], lMaxNumOfUses, sCustomData,
                oCustomDataDict[Constants.SUBSCRIPTION_ID], lBillingTransactionID, dtUtcNow, dtEndDate, dtUtcNow,
                oCustomDataDict[Constants.COUNTRY_CODE], oCustomDataDict[Constants.LANGUAGE_CODE],
                oCustomDataDict[Constants.DEVICE_NAME], int.Parse(oCustomDataDict[Constants.DOMAIN]));

            if (lPPVPurchaseID > 0)
            {
                if (lBillingProviderTransactionID > 0)
                {
                    // update purchase_id in cinepolis_transactions table
                    BillingDAL.Update_PurchaseIDInCinepolisTransactions(lBillingProviderTransactionID, lPPVPurchaseID);
                }

                if (lBillingTransactionID > 0)
                {
                    // update purchase_id in billing_transactions
                    if (!ApiDAL.Update_PurchaseIDInBillingTransactions(lBillingTransactionID, lPPVPurchaseID))
                    {
                        // failed to update purchase id in billing_transactions.
                        #region Logging
                        log.Debug("HandlePPVPurchase - " + string.Format("Failed to update purchase id in billing_transactions. Site Guid: {0} , Billing transactions ID: {1} , PPV Purchase ID: {2} , Billing provider transaction id: {3}", oCustomDataDict[Constants.SITE_GUID], lBillingTransactionID, lPPVPurchaseID, lBillingTransactionID));
                        #endregion
                    }
                }
            }

            // handle coupon use
            long lSubscriptionID = ParseLongIfNotEmpty(oCustomDataDict[Constants.SUBSCRIPTION_ID]);
            HandleCouponUse(oCustomDataDict[Constants.COUPON_CODE], oCustomDataDict[Constants.SITE_GUID], lMediaFileID,
                lSubscriptionID, 0, lGroupID);

            return lPPVPurchaseID;
        }

        private static DateTime CalcPPVEndDate(DateTime dtBase, long lMaxUsageModuleLifeCycle, string sOverrideEndDate)
        {
            DateTime res = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(sOverrideEndDate))
            {
                try
                {
                    res = DateTime.ParseExact(sOverrideEndDate, "dd/MM/yyyy", null);
                }
                catch
                {
                }

            }
            else
            {
                if (lMaxUsageModuleLifeCycle > 0)
                {
                    res = Core.Billing.Utils.GetEndDateTime(dtBase, (int)lMaxUsageModuleLifeCycle);
                }
            }

            return res;
        }

        public static string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static ItemType CinepolisConvertToItemType(string sBusinessModuleInCustomData)
        {
            string sLoweredTrimmed = sBusinessModuleInCustomData.Trim().ToLower();
            if (sLoweredTrimmed == "sp")
                return ItemType.Subscription;
            if (sLoweredTrimmed == "pp")
                return ItemType.PPV;
            return ItemType.Unknown; // Cinepolis does not support pre paid business module
        }

        public static bool IsUserExist(string sSiteGuid, int nGroupID)
        {
            string sEmail = string.Empty;
            return IsUserExist(sSiteGuid, nGroupID, ref sEmail);
        }

        public static bool IsUserExist(string sSiteGuid, int nGroupID, ref string sUserEmail)
        {
            int nDomainID = 0;
            return IsUserExist(sSiteGuid, nGroupID, ref sUserEmail, ref nDomainID);
        }

        public static bool IsUserExist(string sSiteGuid, int nGroupID, ref string sUserEmail, ref int nDomainID)
        {
            bool res = false;
            long lSiteGuid = 0;

            if (string.IsNullOrEmpty(sSiteGuid) || !Int64.TryParse(sSiteGuid, out lSiteGuid) || lSiteGuid == 0)
                res = false;
            else
            {
                UserResponseObject uObj = Core.Users.Module.GetUserData(nGroupID, sSiteGuid, string.Empty);

                if (uObj == null || uObj.m_RespStatus != ResponseStatus.OK)
                {
                    res = false;
                }
                else
                {
                    res = true;
                    sUserEmail = uObj.m_user.m_oBasicData.m_sEmail;
                    nDomainID = uObj.m_user.m_domianID;
                }
            }

            return res;
        }

        private static string GetLogFileName(long lGroupID)
        {
            return String.Concat("Billing.Utils.", lGroupID);
        }

        public static string GetItemNameForPurchaseMail(ItemType it, Dictionary<string, string> oCustomDataDict)
        {
            string res = string.Empty;
            switch (it)
            {
                case ItemType.PPV:
                    res = ApiDAL.Get_PPVNameByMediaID(Int64.Parse(oCustomDataDict[Constants.MEDIA_ID]));
                    break;
                case ItemType.Subscription:
                    res = PricingDAL.Get_ItemName("subscriptions", Int64.Parse(oCustomDataDict[Constants.SUBSCRIPTION_ID]));
                    break;
                default:
                    break;

            }

            return res;
        }

        public static string GetRequestFormSafeVal(HttpRequest req, string sKey)
        {
            if (req != null && req.Form != null && req.Form[sKey] != null)
                return req.Form[sKey];
            return string.Empty;
        }

        public static AdyenBillingDetail GetLastBillingTypeUserInfo(int nGroupID, string sSiteGUID)
        {
            AdyenBillingDetail res = new AdyenBillingDetail();
            try
            {
                DataTable dtUserLastBillingTransactions = ApiDAL.Get_LastBillingTransactionToUser(nGroupID, sSiteGUID, null);

                if (dtUserLastBillingTransactions != null && dtUserLastBillingTransactions.Rows.Count > 0)
                {
                    int nPaymentMethod = ODBCWrapper.Utils.GetIntSafeVal(dtUserLastBillingTransactions.Rows[0]["billing_method"]);
                    string sCellPhone = ODBCWrapper.Utils.GetSafeStr(dtUserLastBillingTransactions.Rows[0]["cell_phone"]);
                    ePaymentMethod paymentMethod = (ePaymentMethod)nPaymentMethod;
                    if (paymentMethod == ePaymentMethod.MasterCard || paymentMethod == ePaymentMethod.Visa)
                    {
                        AdyenDirectDebit adyenDirectDebit = new AdyenDirectDebit(nGroupID);
                        res = adyenDirectDebit.GetLastBillingUserInfo(sSiteGUID, (int)paymentMethod);
                    }
                    else if (paymentMethod == ePaymentMethod.M1)
                    {
                        M1Response m1Response = M1Logic.CanAccessVas(nGroupID, sCellPhone);
                        if (m1Response.is_succeeded)
                        {
                            res.billingInfo = new BillingInfo();
                            res.billingInfo.variant = "m1";
                            res.billingInfo.lastFourDigits = sCellPhone;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Utils.WS GetLastBillingTypeUserInfo() - " + string.Format("Utils.WS GetLastBillingTypeUserInfo() failed, Exception: {0} , nGroupID : {1} , sSiteGuid : {2}", ex.ToString(), nGroupID, sSiteGUID), ex);
            }

            return res;
        }

        public static bool GetLastM1BillingUserInfo(int nGroupID, string sSiteGUID, out string sCellPhone, out string sExtraParams)
        {
            bool result = false;
            sCellPhone = string.Empty;
            sExtraParams = string.Empty;
            try
            {
                int nM1BillingProvider = (int)eBillingProvider.M1;
                DataTable dtUserLastBillingTransactions = ApiDAL.Get_LastBillingTransactionToUser(nGroupID, sSiteGUID, nM1BillingProvider);

                if (dtUserLastBillingTransactions != null && dtUserLastBillingTransactions.Rows.Count > 0)
                {
                    sCellPhone = ODBCWrapper.Utils.GetSafeStr(dtUserLastBillingTransactions.Rows[0]["cell_phone"]);
                    sExtraParams = ODBCWrapper.Utils.GetSafeStr(dtUserLastBillingTransactions.Rows[0]["extra_params"]);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Utils.WS GetLastM1BillingUserInfo() - " + string.Format("Utils.WS GetLastM1BillingUserInfo() failed, Exception: {0} , nGroupID : {1} , sSiteGuid : {2}", ex.ToString(), nGroupID, sSiteGUID));
            }

            return result;
        }

        public static string GetRequestParamsSafeVal(HttpRequest request, string key)
        {
            string res = string.Empty;
            if (request != null && request.Params != null && request.Params.AllKeys != null && request.Params.AllKeys.Length > 0)
            {
                res = request.Params[key];
            }

            return res ?? string.Empty;
        }

        public static string GetDateEmailFormat(int groupId)
        {
            string dateEmailFormat = string.Empty;
            try
            {
                // get from cache 
                string key = string.Format("DateEmailFormat_{0}", groupId);
                bool bRes = BillingCache.GetItem<string>(key, out dateEmailFormat);
                if (!bRes)
                {
                    dateEmailFormat = DAL.BillingDAL.getEmailDateFormat(groupId);
                    if (string.IsNullOrEmpty(dateEmailFormat))
                    {
                        dateEmailFormat = "dd/MM/yyyy";
                    }
                    else
                    {
                        double minuteOffset = TVinciShared.WS_Utils.GetTcmDoubleValue("BillingCacheTTL");
                        if (minuteOffset == 0)
                        {
                            minuteOffset = 60;// default value
                        }

                        bRes = BillingCache.AddItem(key, dateEmailFormat, minuteOffset);
                    }
                }
            }
            catch
            {
                dateEmailFormat = "dd/MM/yyyy";
            }

            return dateEmailFormat;
        }


        internal static ApiObjects.Response.Status ValidateUserAndDomain(int groupId, string siteGuid, ref int householdId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            status.Code = -1;

            // If no user - go immediately to domain validation
            if (string.IsNullOrEmpty(siteGuid))
            {
                status.Code = (int)eResponseStatus.OK;
            }
            else
            {
                // Get response from users WS
                ResponseStatus userStatus = ValidateUser(groupId, siteGuid, ref householdId);
                if (householdId == 0)
                {
                    status.Code = (int)eResponseStatus.UserWithNoDomain;
                    status.Message = eResponseStatus.UserWithNoDomain.ToString();
                }
                else
                {
                    // Most of the cases are not interesting - focus only on those that matter
                    switch (userStatus)
                    {
                        case ResponseStatus.OK:
                            {
                                status.Code = (int)eResponseStatus.OK;
                                break;
                            }
                        case ResponseStatus.UserDoesNotExist:
                            {
                                status.Code = (int)eResponseStatus.UserDoesNotExist;
                                status.Message = eResponseStatus.UserDoesNotExist.ToString();
                                break;
                            }
                        case ResponseStatus.UserNotIndDomain:
                            {
                                status.Code = (int)eResponseStatus.UserNotInDomain;
                                status.Message = "User Not In Domain";
                                break;
                            }
                        case ResponseStatus.UserWithNoDomain:
                            {
                                status.Code = (int)eResponseStatus.UserWithNoDomain;
                                status.Message = eResponseStatus.UserWithNoDomain.ToString();
                                break;
                            }
                        case ResponseStatus.UserSuspended:
                            {
                                status.Code = (int)eResponseStatus.UserSuspended;
                                status.Message = eResponseStatus.UserSuspended.ToString();
                                break;
                            }
                        // Most cases will return general error
                        default:
                            {
                                status.Code = (int)eResponseStatus.Error;
                                status.Message = "Error validating user";
                                break;
                            }
                    }
                }
            }

            // If user is valid (or we don't have one)
            if (status.Code == (int)eResponseStatus.OK && householdId != 0)
            {

                //Get resposne from domains WS                
                status = ValidateDomain(groupId, householdId);


            }
            return status;
        }


        /// <summary>
        /// Validates that a user exists and belongs to a given domain
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="siteGuid"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        private static ResponseStatus ValidateUser(int groupId, string siteGuid, ref int householdId)
        {
            ResponseStatus status = ResponseStatus.InternalError;

            try
            {
                UserResponseObject response = Core.Users.Module.GetUserData(groupId, siteGuid, string.Empty);

                // Make sure response is OK
                if (response != null)
                {
                    status = response.m_RespStatus;

                    if (status == ResponseStatus.OK)
                    {
                        //check Domain and suspend
                        if (response.m_user != null)
                        {
                            if (householdId != 0 && householdId != response.m_user.m_domianID)
                            {
                                status = ResponseStatus.UserNotIndDomain;
                            }
                            else // no domain id was sent
                            {
                                householdId = response.m_user.m_domianID;

                                if (householdId == 0)
                                {
                                    status = ResponseStatus.UserNotIndDomain;
                                }
                            }

                            if (response.m_user.m_eSuspendState == DomainSuspentionStatus.Suspended)
                            {
                                status = ResponseStatus.UserSuspended;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ValidateUser - " + string.Format("Error when validating user {0} in group {1}. ex = {2}, ST = {3}", siteGuid, groupId, ex.Message, ex.StackTrace), ex);
            }

            return status;
        }
        /// <summary>
        /// Validates that a domain exists
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public static ApiObjects.Response.Status ValidateDomain(int groupId, int domainId)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Error validating domain" };

            try
            {
                DomainResponse response = Core.Domains.Module.GetDomainInfo(groupId, domainId);
                status = new ApiObjects.Response.Status(response.Status.Code, response.Status.Message);
            }
            catch (Exception ex)
            {
                log.Error("ValidateDomain - " +
                    string.Format("Error when validating domain {0} in group {1}. ex = {2}, ST = {3}", domainId, groupId, ex.Message, ex.StackTrace),
                    ex);
            }
            return status;
        }

        private static string GetWSUrl(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static ApiObjects.Response.Status SetPartnerConfiguration(int groupId, PartnerConfiguration partnerConfig)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            int intValue = 0;
            bool isSet = false;

            try
            {
                if (partnerConfig == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.NoPartnerConfigurationToUpdate, NO_CONFIGURATION_TO_UPDATE);
                    return response;
                }

                if (string.IsNullOrEmpty(partnerConfig.Value))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.NoConfigurationValueToUpdate, NO_CONFIGURATION_VALUE_UPDATE);
                    return response;
                }

                log.DebugFormat("SetPartnerConfiguration Value: {0}", partnerConfig.Value);

                switch (partnerConfig.Type)
                {
                    case PartnerConfigurationType.DefaultPaymentGateway:
                        if (partnerConfig.Value == "0")
                        {
                            isSet = UpdatePartnerConfig("DEFAULT_PAYMENT_GATEWAY", partnerConfig.Value, groupId);
                        }
                        else if (int.TryParse(partnerConfig.Value, out intValue) && intValue > 0)
                        {
                            // check if payment gateway exist
                            var paymentGateway = BillingDAL.GetPaymentGateway(groupId, intValue, 1, 1);
                            if (paymentGateway == null || paymentGateway.ID <= 0)
                            {
                                response = new ApiObjects.Response.Status((int)eResponseStatus.PaymentGatewayNotExist, PAYMENT_GATEWAY_NOT_EXIST);
                                return response;
                            }

                            isSet = UpdatePartnerConfig("DEFAULT_PAYMENT_GATEWAY", partnerConfig.Value, groupId);
                        }

                        break;
                    case PartnerConfigurationType.EnablePaymentGatewaySelection:
                        // check for int anf bool value
                        if (enablePaymentGatewayInputValues.ContainsKey(partnerConfig.Value.ToLower()))
                        {
                            isSet = UpdatePartnerConfig("ENABLE_PAYMENT_GATEWAY_SELECTION", enablePaymentGatewayInputValues[partnerConfig.Value.ToLower()], groupId);
                        }
                        break;
                    case PartnerConfigurationType.OSSAdapter:
                        if (partnerConfig.Value == "0")
                        {
                            isSet = UpdatePartnerConfig("OSS_ADAPTER", partnerConfig.Value, groupId);
                        }
                        else if (int.TryParse(partnerConfig.Value, out intValue) && intValue > 0)
                        {

                            // check if oss_adapter exist
                            var ossAdapter = ApiDAL.GetOSSAdapter(groupId, intValue);
                            if (ossAdapter == null || ossAdapter.ID <= 0)
                            {
                                response = new ApiObjects.Response.Status((int)eResponseStatus.OSSAdapterNotExist, OSS_ADAPTER_NOT_EXIST);
                                return response;
                            }

                            isSet = UpdatePartnerConfig("OSS_ADAPTER", partnerConfig.Value, groupId);
                        }
                        break;
                    default:
                        break;
                }

                if (isSet)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "partner configuration set changes");

                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "partner configuration failed set changes");
                }

            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}"), ex);
            }
            return response;
        }

        private static bool UpdatePartnerConfig(string colName, string value, int nGroupID)
        {
            bool result = false;

            log.DebugFormat("UpdatePartnerConfig - colName: {0}, value : {1} ", colName, value);
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_parameters");
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM(colName, "=", value);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            result = updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

            return result;
        }


        public static Subscription GetSubscriptionData(int nGroupID, string subID)
        {
            Subscription res = null;
            try
            {
                res = Pricing.Module.GetSubscriptionData(nGroupID, subID, string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                log.Error("GetSubscriptionData - " + string.Format("failed, Exception: {0} , nGroupID : {1} , subID : {2}", ex.ToString(), nGroupID, subID), ex);
                res = null;
            }
            return res;
        }

        internal static byte[] DecodeSecretKey(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }


        internal static bool DataTableExsits(DataTable dataTable)
        {
            return dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0;
        }

        internal static ApiObjects.Country GetCountryByIp(int groupId, string ip)
        {
            ApiObjects.Country res = null;
            try
            {
                res = Core.Api.Module.GetCountryByIp(groupId, ip);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetCountryByIp with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

        internal static string GetIP2CountryCode(int groupId, string ip)
        {
            string res = string.Empty;
            try
            {
                ApiObjects.Country country = GetCountryByIp(groupId, ip);
                res = country != null ? country.Code : res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed Utils.GetIP2CountryCode with groupId: {0}, ip: {1}", groupId, ip), ex);
            }

            return res;
        }

    }
}
