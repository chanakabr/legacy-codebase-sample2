using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using KLogMonitor;
using System.Reflection;


namespace Core.Billing
{
    public class SMSHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static bool SendBulkSMS(Int32 nGroupID, string sCellNum, string sText, string sCurrencyCode)
        {
            string sSmsCode = "";
            Int32 nCostID = 0;
            Int32 nServiceID = 0;
            string sSMS_CHARGE_NUM = "";
            GetSMSServiceCode(nGroupID, 0.0, sCurrencyCode, ref sSmsCode, ref nCostID, ref nServiceID, ref sSMS_CHARGE_NUM);
            if (sSmsCode == "")
                return false;
            sText = sText.Replace("!!!SMS_CHARGE_NUM!!!", sSMS_CHARGE_NUM);
            return SendSMS(nGroupID, sCellNum, sText, nCostID, "2", nServiceID);
        }

        public static bool SendPremiumSMS(Int32 nGroupID, string sCellNum, string sText, double dPrice, string sCurrencyCode)
        {
            string sSmsCode = "";
            Int32 nCostID = 0;
            Int32 nServiceID = 0;
            string sSMS_CHARGE_NUM = "";

            log.Debug("SMS Helper - " + string.Format("Sending premium SMS to mobile: {0}, text: {1}, price:{2}", sCellNum, sText, dPrice));

            GetSMSServiceCode(nGroupID, dPrice, sCurrencyCode, ref sSmsCode, ref nCostID, ref nServiceID, ref sSMS_CHARGE_NUM);

            if (sSmsCode == "")
                return false;

            return SendSMS(nGroupID, sCellNum, sText, nCostID, "2", nServiceID);
        }

        public static bool GetSMSBaseDetails(Int32 nGroupID, ref string sUN, ref string sPass, ref string sURL, ref string sXMLFormat)
        {
            bool bRet = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select * from winpl_group_parameters where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    bRet = true;
                    sUN = selectQuery.Table("query").DefaultView[0].Row["USERNAME"].ToString();
                    sPass = selectQuery.Table("query").DefaultView[0].Row["PASSWORD"].ToString();
                    sURL = selectQuery.Table("query").DefaultView[0].Row["URL"].ToString();
                    sXMLFormat = selectQuery.Table("query").DefaultView[0].Row["XML_SCHEMA"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return bRet;
        }

        public static bool SendSMS(Int32 nGroupID, string sCellNum, string sText, int nCostID, string sTypeID, int nServiceID)
        {
            string sUN = "";
            string sPass = "";
            string sURL = "";
            string sXMLFormat = "";
            GetSMSBaseDetails(nGroupID, ref sUN, ref sPass, ref sURL, ref sXMLFormat);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(sURL);
            webRequest.KeepAlive = false;
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            //Daniel - what is the "123_456", sTypeID, nServiceID , nCostID
            string reqContent = string.Concat("User=", sUN,
                "&Password=", sPass,
                "&TargetURL=", System.Web.HttpUtility.UrlEncode(sURL),
                "&RequestID=", "123_456",
                "&WIN_XML=", System.Web.HttpUtility.UrlEncode(string.Format(sXMLFormat, sCellNum, sText, sTypeID, nServiceID, nCostID)));

            using (StreamWriter sw = new StreamWriter(webRequest.GetRequestStream(), new UTF8Encoding(false)))
            {
                sw.Write(reqContent);
                sw.Close();
            }

            try
            {
                using (HttpWebResponse res = (HttpWebResponse)webRequest.GetResponse())
                {
                    Stream receiveStream = res.GetResponseStream();

                    string ret;
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        ret = readStream.ReadToEnd();
                        readStream.Close();
                    }
                    res.Close();

                    if (ret.IndexOf("R0") == -1)
                    {
                        log.Debug("SMS Helper - " + string.Format("An error returned from sms service. The result:{0}", ret));
                        return false;
                    }
                    else
                        log.Debug("SMS Helper - " + string.Format("SMS sent successfully, mobile: {0}, text:{1}, service id:{2}, cost id:{3}, WinPLC returned:{4}", sCellNum, sText, nServiceID, nCostID, ret));
                }
            }
            catch (Exception ex)
            {
                log.Error("SMS Helper - Failed getting response from sms gateway: " + ex.Message, ex);
                return false;
            }

            return true;
        }

        public static void GetSMSServiceCode(Int32 nGroupID, double dPrice, string sCurrencyCode, ref string sSMSCode, ref Int32 nCostID, ref Int32 nServiceID, ref string sSMS_CHARGE_NUM)
        {
            try
            {
                double dRoundPrice = Math.Round(dPrice, 2);
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += string.Format("select *, {0}-PRICE 'DIFF_PRICE' from prices_sms_codes where is_active=1 and status=1 and ", dRoundPrice);
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE", "<=", dRoundPrice);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CURRENCT_CD", "=", sCurrencyCode);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "order by DIFF_PRICE";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sSMSCode = selectQuery.Table("query").DefaultView[0].Row["SMS_CODE"].ToString();
                        nCostID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COST_ID"].ToString());
                        nServiceID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["SERVICE_ID"].ToString());
                        sSMS_CHARGE_NUM = selectQuery.Table("query").DefaultView[0].Row["SMS_CHARGE_NUM"].ToString();

                        log.Debug("SMS Helper - " + string.Format("Got sms service code for group:{0}, price:{1}, currency code:{2} - Got cost id:{3}, service id:{4}", nGroupID, dPrice, sCurrencyCode, nCostID, nServiceID));
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch
            {
                sSMSCode = "";
                nCostID = 0;
                nServiceID = 0;

                log.Debug("SMS Helper - " + string.Format("Failed getting sms service code for group:{0}, price:{1}, currency code:{2}", nGroupID, dPrice, sCurrencyCode));
            }
        }
        /*
        public static bool TryGetService(string theServiceCode, out Service theService)
        {
            theService = null;

            if (SMSConfiguration.Instance == null)
            {
                return false;
            }

            foreach (Service ser in SMSConfiguration.Instance.Data.Services)
            {
                if (ser.ServiceCode.Equals(theServiceCode))
                {
                    theService = ser;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetService(int theServiceID, out Service theService)
        {
            theService = null;

            if (SMSConfiguration.Instance == null)
            {
                return false;
            }

            foreach (Service ser in SMSConfiguration.Instance.Data.Services)
            {
                if (ser.ServiceID == theServiceID)
                {
                    theService = ser;
                    return true;
                }
            }

            return false;
        }
        */
    }
}
