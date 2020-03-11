using ApiObjects;
using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public class WinPLCSMS: BaseSMS
    {
        public WinPLCSMS(Int32 nGroupID): base(nGroupID)
        {
        }

        public override BillingResponse CheckCode(string sSiteGUID, string sCellPhone, string sCode, string sReferenceCode)
        {
            Int32 nMediaFileID = 0;
            Int32 nMediaID = 0;
            string sSubscriptionCode = "";
            string sPPVCode = "";
            string sPriceCode = "";
            double dPrice = 0.0;
            string sCurrencyCode = "";

            SpliStrtRefference(sReferenceCode, ref nMediaFileID, ref sSubscriptionCode,
                ref sPPVCode, ref sPriceCode, ref dPrice, ref sCurrencyCode);
            
            Int32 nActivationState = 0;
            string sCustomData = "";
            Int32 nID = CheckSMSCode(ref sSubscriptionCode, ref nMediaFileID, sSiteGUID, sCode,
                sCellPhone, ref sPPVCode, sPriceCode, dPrice, sCurrencyCode, true, ref nActivationState , ref sCustomData);
            string sRefference = "mf:" + nMediaFileID.ToString() + " " + "sub:" + sSubscriptionCode + " " + "ppvcode:" + sPPVCode;
            BillingResponse ret = new BillingResponse();
            if (nID != 0)
            {
                long lTransactionID = 0;
                if (nActivationState == 2)
                {
                    ret.m_oStatus = BillingResponseStatus.Success;
                    ret.m_sStatusDescription = sRefference;

                    string sPPVModuleCode = "";
                    bool bIsRecurring = false;
                    double dChargePrice = 0.0;
                    Int32 nStatus = 1;
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

                    nStatus = 0;
                    Core.Billing.Utils.SplitRefference(sCustomData, ref nMediaFileID, ref nMediaID,  ref sSubscriptionCode, ref sPPVCode, ref sRelevantPrePaid, ref sPriceCode,
                            ref dChargePrice, ref sCurrencyCode, ref bIsRecurring, ref sPPVModuleCode, ref nNumberOfPayments ,
                            ref sUserGUID, ref sRelevantSub, ref nMaxNumberOfUses, ref nMaxUsageModuleLifeCycle, 
                            ref nViewLifeCycleSecs , ref sPurchaseType , ref sCountryCd , ref sLanguageCode , ref sDeviceName);

                    double dPer = double.Parse(ODBCWrapper.Utils.GetTableSingleVal("winpl_group_parameters", "add_per", "group_id" , "=" , m_nGroupID).ToString());
                    double dAddPrice = Math.Round(dChargePrice * dPer / 100, 2);
                    double dTotal = Math.Round(dChargePrice + dAddPrice, 2);
                    lTransactionID = Core.Billing.Utils.InsertBillingTransaction(sSiteGUID, "", dChargePrice, sPriceCode,
                            sCurrencyCode, sCustomData, nStatus, "Code OK", bIsRecurring, nMediaFileID, nMediaID, sPPVModuleCode,
                            sSubscriptionCode, sCellPhone, m_nGroupID, 2, nID, dAddPrice, dTotal , 1 , 1 , "" ,
                            sCountryCd, sLanguageCode, sDeviceName , 3 , 2, sRelevantPrePaid);
                    //send purchase mail
                    string sItemName = "";
                    ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery1.SetConnectionKey("MAIN_CONNECTION_STRING");
                    selectQuery1 += "select name from media m, media_files mf where mf.media_id=m.id and ";
                    selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("mf.id", "=", nMediaFileID);
                    if (selectQuery1.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery1.Table("query").DefaultView.Count;
                        if (nCount > 0)
                        {
                            sItemName = selectQuery1.Table("query").DefaultView[0].Row["NAME"].ToString();
                        }
                    }
                    selectQuery1.Finish();
                    selectQuery1 = null;
                    string sPaymentMethod = "SMS (" + sCellPhone + ")";
                    Core.Billing.Utils.SendMail(sPaymentMethod, sItemName, sUserGUID, lTransactionID,
                        String.Format("{0:0.##}", dTotal) , sCurrencyCode, string.Empty,m_nGroupID, string.Empty, eMailTemplateType.Purchase);
                }
                else
                {
                    ret.m_oStatus = BillingResponseStatus.Fail;
                    ret.m_sStatusDescription = "SMS was not sent yet";
                }
                //ret.m_sRecieptCode = nID.ToString();
                ret.m_sRecieptCode = lTransactionID.ToString();
                
            }
            else
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Unrecognized code";
            }
            return ret;
        }

        public string GetDescriptionText(string sCode, string sPhoneNum, double dAmount, string sCurrencyCode)
        {
            object oPreCode = ODBCWrapper.Utils.GetTableSingleVal("winpl_group_parameters", "PRE_CODE", "GROUP_ID" , "=" , m_nGroupID);
            if (oPreCode != DBNull.Value && oPreCode != null)
                sCode = oPreCode.ToString() + " " + sCode;
            string sRet = "Please send the following code <b><font color='#FF6633'>" + sCode + "</font></b> to <b><font color='#FF6633'>" + sPhoneNum + "</font></b> via SMS.\n";
            sRet += "You will be billed by the sum of <b><font color='#FF6633'>" + dAmount.ToString() + " " + sCurrencyCode + "</font></b>\n";
            sRet += "You will then receive a verification code, which you should enter here: ";
            return sRet;
        }
        
        public bool GetSMSDetailsText(string sCellNum, double dPrice, string sCurrencyCode , string sReferenceCode, string sToken, ref string sErr , ref string sSendText , bool bSendSMS)
        {            
            if (dPrice == 0)
            {
                sErr = "Cant charge free";
                return false;
            }

            if (string.IsNullOrEmpty(sCellNum) || string.IsNullOrEmpty(sCurrencyCode))
            {
                sErr = "Invalid parameters: Cell: " + sCellNum + " || Currency: " + sCurrencyCode;
                return false;
            }

            try
            {
                string sSmsCode = "";
                Int32 nCostID = 0;
                Int32 nServiceID = 0;
                string sSMS_CHARGE_NUM = "";
                SMSHelper.GetSMSServiceCode(m_nGroupID, dPrice, sCurrencyCode, ref sSmsCode, ref nCostID, ref nServiceID, ref sSMS_CHARGE_NUM);
                
                if (sSmsCode == "")
                {
                    sErr = "Failed sending bulk sms to user: Cell: " + sCellNum + " || Currency: " + sCurrencyCode;
                    return false;
                }
                sSendText = GetDescriptionText(sToken, sSMS_CHARGE_NUM, dPrice, sCurrencyCode);
            }
            catch (Exception ex)
            {
                sErr = "Failed sending bulk sms to user: Cell: " + sCellNum + " || Currency: " + sCurrencyCode + " || " + ex.Message + " || " + ex.StackTrace;
                return false;
            }

            return true;
        }

        public bool SendConfirmationSMS(string sCellNum, string sToken, Int32 nSMSTokenEntry , double dPrice , string sCurrencyCode, ref string sErr)
        {
            try
            {
                if (Core.Billing.SMSHelper.SendPremiumSMS(m_nGroupID, sCellNum, string.Format("Thank you for your purchase (receipt number: {1}). To watch – please enter code: {0}. ", sToken, nSMSTokenEntry), dPrice, sCurrencyCode) == false)
                {
                    sErr = "Failed sending bulk sms to user: Cell: " + sCellNum + " || Currency: " + sCurrencyCode;
                    return false;
                }
            }
            catch (Exception ex)
            {
                sErr = "Failed sending bulk sms to user: Cell: " + sCellNum + " || Currency: " + sCurrencyCode + " || " + ex.Message + " || " + ex.StackTrace;
                return false;
            }
            return true;
        }

        protected void SpliStrtRefference(string sRefference, ref Int32 nMediaFileID, ref string sSubscriptionCode , ref string sPPVCode ,
            ref string sPriceCode , ref double dPrice , ref string sCurrencyCd)
        {
            string[] spliter = { " " };
            string[] splited = sRefference.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splited.Length; i++)
            {
                string sHeader = splited[i];
                if (sHeader.StartsWith("mf:") == true)
                    nMediaFileID = int.Parse(sHeader.Substring(3));
                if (sHeader.StartsWith("sub:") == true)
                    sSubscriptionCode = sHeader.Substring(4);
                if (sHeader.StartsWith("ppvcode:") == true)
                    sPPVCode = sHeader.Substring(8);
                if (sHeader.StartsWith("pricecode:") == true)
                    sPriceCode = sHeader.Substring(10);
                if (sHeader.StartsWith("price:") == true)
                    dPrice = double.Parse(sHeader.Substring(6));
                if (sHeader.StartsWith("currency:") == true)
                    sCurrencyCd = sHeader.Substring(9);
            }
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

        protected void SplitRefference(string sRefference, ref Int32 nMediaFileID, ref string sSubscriptionCode, ref string sPPVCode,
            ref string sPriceCode, ref double dPrice, ref string sCurrencyCd)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(sRefference);
            System.Xml.XmlNode theRequest = doc.FirstChild;

            string sType = GetSafeParValue(".", "type", ref theRequest);
            string sSubscriptionID = GetSafeValue("s", ref theRequest);
            string scouponcode = GetSafeValue("cc", ref theRequest);
            string sPayNum = GetSafeParValue("//p", "n", ref theRequest);
            string sPayOutOf = GetSafeParValue("//p", "o", ref theRequest);
            string smedia_file = GetSafeValue("mf", ref theRequest);
            string ssub = GetSafeValue("s", ref theRequest);
            string sppvmodule = GetSafeValue("ppvm", ref theRequest);
            string srelevantsub = GetSafeValue("rs", ref theRequest);
            string smnou = GetSafeValue("mnou", ref theRequest);
            string smaxusagemodulelifecycle = GetSafeValue("mumlc", ref theRequest);
            string sviewlifecyclesecs = GetSafeValue("vlcs", ref theRequest);
            string sppvcode = GetSafeValue("ppvm", ref theRequest);
            string spc = GetSafeValue("pc", ref theRequest);
            string spri = GetSafeValue("pri", ref theRequest);
            string scur = GetSafeValue("cu", ref theRequest);

            if (smedia_file != "")
                nMediaFileID = int.Parse(smedia_file);
            sSubscriptionCode = sSubscriptionID;
            sPPVCode = sppvcode;
            sPriceCode = spc;
            if (spri != "")
                dPrice = double.Parse(spri);
            sCurrencyCd = scur;
        }

        public override BillingResponse SendCode(string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters)
        {
            Int32 nID = 0;
            string sErr = "";
            Int32 nMediaFileID = 0;
            string sSubscriptionCode = "";
            string sPPVCode = "";
            string sPriceCode = "";
            double dPrice = 0.0;
            string sCurrencyCode = "";

            SplitRefference(sReferenceCode , ref nMediaFileID , ref sSubscriptionCode , 
                ref sPPVCode , ref sPriceCode , ref dPrice , ref sCurrencyCode);

            // Add to dPrice the commision here
            //----------------------------------------
            double dPer = double.Parse(ODBCWrapper.Utils.GetTableSingleVal("winpl_group_parameters", "add_per", "group_id" , "=" , m_nGroupID).ToString());
            dPrice = Math.Round(dPrice * (100 + dPer) / 100, 2);

            string sToken = "";
            Int32 nActivationState = 0;
            Int32 nSMSPurchaseID = 0;
            string sSentText = "";
            // Check if there is a purchase pending for that media file and cell phone
            bool bPending = IsPendingSMSPurchase(nMediaFileID, sSiteGUID, sCellPhone, dPrice, sCurrencyCode, sSubscriptionCode, sPPVCode, ref sToken, ref nActivationState, ref nSMSPurchaseID);
            if (bPending == false)
            {
                // No pending purchase - create a new token
                Random rnd = new Random();
                sToken = string.Concat(rnd.Next(9), rnd.Next(9), rnd.Next(9), rnd.Next(9));
            }

            if (nActivationState == 0 && GetSMSDetailsText(sCellPhone, dPrice, sCurrencyCode, sReferenceCode, sToken, ref sErr, ref sSentText, true))
            {
                Int32 nAS = 0;

                if (bPending == false)
                    InsertSMSCode(sSubscriptionCode, nMediaFileID, sSiteGUID, sToken, sCellPhone,
                        sPPVCode, sPriceCode, dPrice, sCurrencyCode, sReferenceCode , 2);

                nID = CheckSMSCode(ref sSubscriptionCode, ref nMediaFileID, sSiteGUID, sToken, sCellPhone,
                    ref sPPVCode, sPriceCode, dPrice, sCurrencyCode, false, ref nAS, ref sReferenceCode);
            }

            if (nActivationState == 2 && SendConfirmationSMS(sCellPhone, sToken, nSMSPurchaseID , dPrice, sCurrencyCode, ref sErr))
            {
                sSentText = "You will receive an sms with a verification code, enter the code here: ";
                Int32 nAS = 0;
                nID = CheckSMSCode(ref sSubscriptionCode, ref nMediaFileID, sSiteGUID, sToken, sCellPhone,
                    ref sPPVCode, sPriceCode, dPrice, sCurrencyCode, false, ref nAS , ref sReferenceCode);
            }
            

            BillingResponse ret = new BillingResponse();
            if (sErr == "")
            {
                ret.m_oStatus = BillingResponseStatus.Success;
                ret.m_sStatusDescription = sSentText;
            }
            else
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = sErr;
            }
            ret.m_sRecieptCode = nID.ToString();
            return ret;
        }
    }
}
