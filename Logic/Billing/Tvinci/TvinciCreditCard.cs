using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class TvinciCreditCard : BaseCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciCreditCard(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            string sSCUserName = "";
            string sSecret = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select * from sc_group_parameters where is_active=1 and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sSecret = selectQuery.Table("query").DefaultView[0].Row["CLIENT_SECRET"].ToString();
                    sSCUserName = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            string sToMD5 = sSCUserName + sSecret + sUserIP + sRandom;
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            md5Provider = new MD5CryptoServiceProvider();
            byte[] originalBytes = UTF8Encoding.Default.GetBytes(sToMD5);
            byte[] encodedBytes = md5Provider.ComputeHash(originalBytes);
            return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
        }

        public override string GetClientMerchantSig(string sParams)
        {
            string retVal = string.Empty;
            string[] FIELDS_MERCHANT_SIG = { "paymentAmount", "currencyCode", "shipBeforeDate", "merchantReference", "skinCode", "merchantAccount", "sessionValidity", "shopperEmail",
                                            "shopperReference", "recurringContract", "allowedMethods", "blockedMethods", "shopperStatement", "merchantReturnData", "billingAddressType", "offset"};

            string[] FIELDS_MERCHANT_SIG_SHA256 = { "paymentAmount", "currencyCode", "shipBeforeDate", "merchantReference", "skinCode", "merchantAccount", "sessionValidity", "shopperEmail",
                                            "shopperReference", "recurringContract", "allowedMethods", "blockedMethods", "shopperStatement", "merchantReturnData", "billingAddressType", "offset",
                                            "brandCode", "shopperLocale", "orderData"};

            /// Expected order of parameters for SHA1:
            /// "paymentAmount ; currencyCode; shipBeforeDate ; merchantReference ; skinCode ; merchantAccount ; sessionValidity ; shopperEmail ;shopperReference ;
            ///  recurringContract ; allowedMethods ; blockedMethods; shopperStatement ; merchantReturnData ; billingAddressType ; offset"
            ///  
            /// Expected order of parameters for SHA256:
            /// "paymentAmount ; currencyCode; shipBeforeDate ; merchantReference ; skinCode ; merchantAccount ; sessionValidity ; shopperEmail ;shopperReference ;
            ///  recurringContract ; allowedMethods ; blockedMethods; shopperStatement ; merchantReturnData ; billingAddressType ; offset ; brandCode ; shopperLocale ; orderData"

            try
            {
                string[] sParamsArr = sParams.Split(';');
                if (sParamsArr != null && sParamsArr.Length > 4)
                {
                    string skinCode = sParamsArr[4];
                    string hmacKey = string.Empty;
                    string hmacSecret = string.Empty;

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select top(1) hmac_key, group_secret from adyen_group_parameters where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("skin_code", "=", skinCode);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            hmacKey = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "hmac_key", 0);
                            hmacSecret = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "group_secret", 0);
                        }
                    }

                    selectQuery.Finish();
                    selectQuery = null;

                    if (string.IsNullOrEmpty(hmacKey))
                    {
                        log.ErrorFormat("hmacKey is not valid for skinCode: {0}, will use old hash instead", skinCode);
                        StringBuilder signingStringSB = new StringBuilder();
                        for (int i = 0; i < FIELDS_MERCHANT_SIG.Length && i < sParamsArr.Length; i++)
                        {
                            signingStringSB.Append(sParamsArr[i]);
                        }
                        // Generate the signing string
                        string signingString = signingStringSB.ToString();

                        // Values are always transferred using UTF-8 encoding
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

                        // Calculate the HMAC
                        HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
                        retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signingString)));
                        myhmacsha1.Clear();
                    }
                    else
                    {
                        Dictionary<string, string> keyValueMap = new Dictionary<string, string>();
                        for (int i = 0; i < FIELDS_MERCHANT_SIG_SHA256.Length && i < sParamsArr.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sParamsArr[i]))
                            {
                                // replace ':' to '\\:' according to Adyen requirements
                                keyValueMap.Add(FIELDS_MERCHANT_SIG_SHA256[i], sParamsArr[i].Replace(":", "\\:"));
                            }
                        }

                        // get the keys ordered by abc asc according to Adyen requirements
                        keyValueMap = keyValueMap.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        List<string> keysAndValues = new List<string>(keyValueMap.Keys);
                        keysAndValues.AddRange(keyValueMap.Values);
                        // Generate the signing string
                        string signingString = string.Join(":", keysAndValues.ToArray());

                        // Values are always transferred using UTF-8 encoding     
                        using (HMACSHA256 newHmacsha = new HMACSHA256(Utils.DecodeSecretKey(hmacKey)))
                        {
                            // Calculate the HMAC
                            retVal = System.Convert.ToBase64String(newHmacsha.ComputeHash(Encoding.UTF8.GetBytes(signingString)));
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error on AdyenCreditCard.GetClientMerchantSig, Exception message: {0}, stackTrace: {1}, source: {2}", ex.Message, ex.StackTrace, ex.Source);
            }

            return retVal;
        }

        protected Int32 InsertNewSCTransaction(string sSiteGUID, string sDigits, double dPrice, string sCurrency, string sCustomData)
        {
            Int32 nRet = 0;
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("sc_transactions");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_GUID", "=", sSiteGUID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("LAST_FOUR_DIGITS", "=", sDigits);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "=", dPrice);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCY_CODE", "=", sCurrency);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CUSTOMDATA", "=", sCustomData);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 1);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from sc_transactions where is_active=1 and ";
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
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CUSTOMDATA", "=", sCustomData);
            selectQuery += " order by id desc";
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

        protected void GetSCParameters(ref string sSCUrl, ref string sUN, ref string sPass)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from sc_group_parameters where is_active=1 and status=1 and ";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
            selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    sSCUrl = selectQuery.Table("query").DefaultView[0].Row["URL"].ToString();
                    sUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                    sPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
        }



        public string GetSaleURL(string sSiteGUID, UserBasicData userData, string sToken,
            Int32 nExpM, Int32 nExpY, double dPrice, string sCurrency, string sUserIP, string sCustomData,
            string sIssueNum, string sStartM, string sStartY, string sIssuerBank, ref Int32 nTransactionLocalID, int transactionID)
        {
            string sSCUser = "";
            string sSCPass = "";
            string sURL = "";
            GetSCParameters(ref sURL, ref sSCUser, ref sSCPass);
            string sRet = sURL;
            if (sRet.EndsWith("?") == false)
                sRet += "?";
            sRet += "sg_FirstName=" + System.Web.HttpUtility.UrlEncode(userData.m_sFirstName) + "&";
            sRet += "sg_Rebill=1&";
            if (transactionID > 0)
            {
                sRet += string.Format("{0}={1}&", "sg_TransactionID", transactionID.ToString());
            }
            sRet += "sg_LastName=" + System.Web.HttpUtility.UrlEncode(userData.m_sLastName) + "&";
            sRet += "sg_Address=" + System.Web.HttpUtility.UrlEncode(userData.m_sAddress) + "&";
            sRet += "sg_City=" + System.Web.HttpUtility.UrlEncode(userData.m_sCity) + "&";
            sRet += "sg_Zip=" + System.Web.HttpUtility.UrlEncode(userData.m_sZip) + "&";
            if (sIssueNum.Trim() != "")
                sRet += "sg_DC_Issue=" + System.Web.HttpUtility.UrlEncode(sIssueNum) + "&";
            if (sStartM.Trim() != "")
                sRet += "sg_DC_StartMon=" + System.Web.HttpUtility.UrlEncode(sStartM) + "&";
            if (sStartY.Trim() != "")
                sRet += "sg_DC_StartYear=" + System.Web.HttpUtility.UrlEncode(sStartY) + "&";
            if (sIssuerBank.Trim() != "")
                sRet += "sg_IssuingBankName=" + System.Web.HttpUtility.UrlEncode(sIssuerBank) + "&";

            if (userData.m_Country != null)
                sRet += "sg_Country=" + System.Web.HttpUtility.UrlEncode(userData.m_Country.m_sCountryCode) + "&";
            if (userData.m_State != null)
                sRet += "sg_State=" + System.Web.HttpUtility.UrlEncode(userData.m_State.m_sStateCode) + "&";

            sRet += "sg_Phone=" + System.Web.HttpUtility.UrlEncode(userData.m_sPhone) + "&";
            sRet += "sg_IPAddress=" + System.Web.HttpUtility.UrlEncode(sUserIP) + "&";
            sRet += "sg_Email=" + System.Web.HttpUtility.UrlEncode(userData.m_sEmail) + "&";
            sRet += "sg_TransType=Sale&";
            sRet += "sg_Amount=" + dPrice.ToString() + "&";
            sRet += "sg_Currency=" + sCurrency + "&";
            sRet += "sg_ClientLoginID=" + sSCUser + "&";
            sRet += "sg_ClientPassword=" + sSCPass + "&";

            bool bSaved = false;
            string sDigits = GetUserDigits(sSiteGUID, ref bSaved);
            //Custom data change
            Int32 nCustomDataID = Utils.AddCustomData(sCustomData);
            //nTransactionLocalID = InsertNewSCTransaction(sSiteGUID , sDigits, dPrice , sCurrency , sCustomData);
            nTransactionLocalID = InsertNewSCTransaction(sSiteGUID, sDigits, dPrice, sCurrency, sCustomData);

            sRet += "sg_ClientUniqueID=" + nTransactionLocalID.ToString() + "&";
            //For test account only!!
            //sRet += "sg_Version=4.0.2&";
            sRet += "sg_Version=2.0.1&";
            sRet += "sg_ResponseFormat=4&";
            sRet += "sg_Is3dTrans=0&";
            sRet += "sg_CCToken=" + System.Web.HttpUtility.UrlEncode(sToken) + "&";
            if (m_isMultiUserCC)
            {
                sRet += "sg_TokenID=" + sSiteGUID + "&";
            }
            sRet += "sg_CustomData=" + System.Web.HttpUtility.UrlEncode(nCustomDataID.ToString()) + "&";
            sRet += "sg_ExpMonth=";
            if (nExpM < 10)
                sRet += "0";
            sRet += nExpM.ToString() + "&";
            sRet += "sg_ExpYear=";
            if (nExpY < 10)
                sRet += "0";
            sRet += nExpY.ToString();

            return sRet;
        }

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();
            UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
            if (uObj.m_RespStatus != ResponseStatus.OK)
            {
                ret = new BillingResponse();
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
                ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "User does not have a token";
                return ret;
            }
            string nExpYearStr = string.Format("20{0}", nExpY);
            int expYear = int.Parse(nExpYearStr);
            if (expYear < DateTime.Now.Year || (expYear == DateTime.Now.Year && nExpM < DateTime.Now.Month))
            {
                ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.ExpiredCard;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Card is expired";
                return ret;
            }

            UserBasicData uBasicData = uObj.m_user.m_oBasicData;
            Int32 nTransactionLocalID = 0;
            string sSaleURL = GetSaleURL(sSiteGUID, uBasicData, sToken, nExpM, nExpY, dChargePrice,
                sCurrencyCode, sUserIP, sCustomData, sIssueNum, sStartM, sStartY, sIssuerBank, ref nTransactionLocalID, transID);
            Int32 nStatus = 0;
            log.Debug("send to safecharge: " + sSaleURL);
            string sResp = TVinciShared.Notifier.SendGetHttpReq(sSaleURL, ref nStatus);
            log.Debug("returned from safecharge: " + sResp);
            if (nStatus != 200)
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Proccessor is down";
                return ret;
            }
            else
                ret = GetBillingResponse(sResp, nTransactionLocalID, sSiteGUID, nPaymentNumber, nNumberOfPayments, sUserIP);

            return ret;
        }

        //public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, int transactionID)
        //{
        //    UsersService u = new UsersService();
        //    string sIP = "1.1.1.1";
        //    string sWSUserName = "";
        //    string sWSPass = "";
        //    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
        //    string sWSURL = Utils.GetWSURL("users_ws");
        //    if (sWSURL != "")
        //        u.Url = sWSURL;
        //    BillingResponse ret = new BillingResponse();
        //    UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
        //    if (uObj.m_RespStatus != ResponseStatus.OK)
        //    {
        //        ret = new BillingResponse();
        //        ret.m_oStatus = BillingResponseStatus.UnKnownUser;
        //        ret.m_sRecieptCode = "";
        //        ret.m_sStatusDescription = "Unknown or active user";
        //        return ret;
        //    }

        //    Int32 nExpM = 1;
        //    Int32 nExpY = 10;
        //    string sIssueNum = "";
        //    string sStartM = "";
        //    string sStartY = "";
        //    string sIssuerBank = "";
        //    string sToken = GetUserToken(sSiteGUID, ref nExpM, ref nExpY, ref sStartM, ref sStartY, ref sIssueNum, ref sIssuerBank);
        //    if (sToken == "")
        //    {
        //        ret = new BillingResponse();
        //        ret.m_oStatus = BillingResponseStatus.Fail;
        //        ret.m_sRecieptCode = "";
        //        ret.m_sStatusDescription = "User does not have a token";
        //        return ret;
        //    }

        //    UserBasicData uBasicData = uObj.m_user.m_oBasicData;
        //    Int32 nTransactionLocalID = 0;
        //    string sSaleURL = GetSaleURL(sSiteGUID, uBasicData, sToken, nExpM, nExpY, dChargePrice,
        //        sCurrencyCode, sUserIP, sCustomData, sIssueNum, sStartM, sStartY, sIssuerBank, ref nTransactionLocalID, transactionID);
        //    Int32 nStatus = 0;
        //    string sResp = TVinciShared.Notifier.SendGetHttpReq(sSaleURL, ref nStatus);
        //    if (nStatus != 200)
        //    {
        //        ret.m_oStatus = BillingResponseStatus.ExternalError;
        //        ret.m_sRecieptCode = "";
        //        ret.m_sStatusDescription = "Proccessor is down";
        //        return ret;
        //    }
        //    else
        //        ret = GetBillingResponse(sResp, nTransactionLocalID, sSiteGUID, nPaymentNumber, nNumberOfPayments);

        //    return ret;
        //}

        protected BillingResponse GetBillingResponse(string sResp, Int32 nTransactionLocalID, string sSiteGUID,
            Int32 nPaymentNumber, Int32 nNumberOfPayments, string sUserIP)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sResp);
            System.Xml.XmlNode theRoot = doc.FirstChild;
            string sClientLoginID = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "ClientLoginID");
            string sClientUniqueID = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "ClientUniqueID");
            string sTransactionID = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "TransactionID");
            string sStatus = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "Status");
            string sAuthCode = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "AuthCode");
            string sAVSCode = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "AVSCode");
            string sCVV2Reply = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "CVV2Reply");
            string sReason = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "Reason");
            string sErrCode = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "ErrCode");
            string sExErrCode = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "ExErrCode");
            //Customdata change
            string sCustomDataID = TVinciShared.XmlUtils.GetNodeValue(ref theRoot, "CustomData");
            Int32 nCustomDataID = int.Parse(sCustomDataID);
            string sCustomData = Utils.GetCustomData(nCustomDataID);

            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("sc_transactions");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_TRANSACTIONID", "=", sTransactionID);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_STATUS", "=", sStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AUTHCODE", "=", sAuthCode);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_AVSCODE", "=", sAVSCode);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CVV2REPLY", "=", sCVV2Reply);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_REASON", "=", sReason);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_ERRORCODE", "=", int.Parse(sErrCode));
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_EXTERRORCODE", "=", int.Parse(sExErrCode));
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_ACTIVE", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTransactionLocalID);
            updateQuery += " and ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SC_CUSTOMDATA", "=", sCustomData);
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;


            BillingResponse ret = new BillingResponse();
            if (sStatus.Trim().ToLower() == "approved" ||
                sStatus.Trim().ToLower() == "success")
            {
                ret.m_oStatus = BillingResponseStatus.Success;
                ret.m_sRecieptCode = nTransactionLocalID.ToString() + " | " + sTransactionID;
                ret.m_sStatusDescription = "OK";
            }
            else if (sStatus.Trim().ToLower() == "declined")
            {
                DeleteUserCreditCardDigits(sSiteGUID);
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = sReason;
            }
            else if (sStatus.Trim().ToLower() == "error")
            {
                DeleteUserCreditCardDigits(sSiteGUID);
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = "Fraud";
            }
            else
            {
                DeleteUserCreditCardDigits(sSiteGUID);
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = "Unknown";
            }

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

            string sCountryCd = "";
            string sLanguageCode = "";
            string sDeviceName = "";
            string prePaidCode = string.Empty;
            if (!string.IsNullOrEmpty(sUserIP))
            {
                sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                if (m_nGroupID == 109 || m_nGroupID == 110 || m_nGroupID == 111)
                {
                    log.Debug("Billing IP - " + sUserGUID + " : " + sCountryCd);
                }
            }
            else
            {
                if (m_nGroupID == 109 || m_nGroupID == 110 || m_nGroupID == 111)
                {
                    log.Debug("Billing IP - User IP is empty");
                }
            }
            Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID, ref sSubscriptionCode, ref sPPVCode, ref prePaidCode, ref sPriceCode,
                    ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments,
                    ref sUserGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, ref nViewLifeCycleSecs, ref sPurchaseType,
                    ref sCountryCd, ref sLanguageCode, ref sDeviceName);
            bool bSaved = false;
            string sLastDigits = GetUserDigits(sSiteGUID, ref bSaved);
            long lTransactionID = Utils.InsertBillingTransaction(sSiteGUID, sLastDigits, dChargePrice, sPriceCode,
                    sCurrencyCode, sCustomData, (int)(ret.m_oStatus), sErrCode + "|" + sExErrCode + "|" + sReason, bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                    sSubscriptionCode, "", m_nGroupID, 1, nTransactionLocalID, 0.0, dChargePrice, nPaymentNumber, nNumberOfPayments, "",
                    sCountryCd, sLanguageCode, sDeviceName, 1, 1, prePaidCode);
            ret.m_sRecieptCode = lTransactionID.ToString();
            return ret;
        }

        public override bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID)
        {
            return true;
        }
    }
}
