using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using KLogMonitor;
using System.Reflection;
using Core.Billing;

namespace WS_Billing
{
    public partial class winpl_api : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string WINPLC_RESPONSE_FORMAT = @"<?xml version=""1.0"" standalone=""no""?>
            <!DOCTYPE SMSRESPONSE SYSTEM ""response_generic_v1.dtd"">
            <SMSRESPONSE>
               <REQUESTID>HTTP_T1_ID{0}_R{1}</REQUESTID>
            </SMSRESPONSE>";

        protected void Page_Load(object sender, EventArgs e)
        {
            string sFormParameters = string.Empty;
            Response.Clear();

            try
            {
                Int32 nCount = Request.TotalBytes;
                sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));

                if (string.IsNullOrEmpty(sFormParameters))
                {
                    log.Debug("WINPL request - Empty Request");
                    return;
                }

                log.Debug("WINPL request - Request received: " + sFormParameters);

                HandleWinRequest(sFormParameters);
            }
            catch (Exception ex)
            {
                log.Error("WINPL request exception: " + ex.Message + " || form parameters:" + sFormParameters, ex);
                Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
            }
        }

        public void HandleWinRequest(string sFormParameters)
        {
            Int32 nGroupID = GetGroupID(sFormParameters);
            if (nGroupID == 0)
            {
                log.Debug("WINPL request - Failed to validate request.");
                return;
            }

            // Get request
            string sRequest = null;
            int nReqIndex = sFormParameters.IndexOf("TP_XML=", StringComparison.CurrentCultureIgnoreCase);
            if (nReqIndex != -1)
            {
                nReqIndex += 7;
                int reqEnd = sFormParameters.IndexOf("&", nReqIndex);
                if (reqEnd == -1)
                    sRequest = sFormParameters.Substring(nReqIndex);
                else
                    sRequest = sFormParameters.Substring(nReqIndex, reqEnd - (nReqIndex));
            }
            else
            {
                log.Debug("WINPL request - Request came in an unexpected format. Didn't find the TP_XML part.");
                Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                return;
            }

            // Handle request
            sRequest = HttpUtility.UrlDecode(sRequest);
            try
            {
                // Parse XML
                XmlDocument doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(sRequest);

                XmlNode root = doc.SelectSingleNode("WIN_TPBOUND_MESSAGES");

                log.Debug("WINPL request - " + string.Format("Found {0} messages.", root.ChildNodes.Count));

                foreach (XmlNode smsMsg in root.ChildNodes)
                {
                    // Get phone
                    string sCellNum = null;
                    if (smsMsg.SelectSingleNode("SOURCE_ADDR") != null)
                        sCellNum = smsMsg.SelectSingleNode("SOURCE_ADDR").InnerText;
                    else
                    {
                        log.Debug("WINPL request - SMS cell number came empty");
                        Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                    }

                    // Get sms text
                    string sSMSText = null;
                    if (smsMsg.SelectSingleNode("TEXT") != null)
                        sSMSText = smsMsg.SelectSingleNode("TEXT").InnerText;
                    else
                    {
                        log.Debug("WINPL request - SMS Text came empty");
                        Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                    }

                    // Seperate token from text
                    string[] sepMSG = sSMSText.Split(' ');
                    if (sepMSG.Length != 2)
                    {
                        log.Debug("WINPL request - Error extracting token from message. Message: " + sSMSText);
                        Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                        return;
                    }

                    log.Debug("WINPL request - " +
                        string.Format("Extracted the following request paramteres - Cell Number: {0}, SMS Text: {1}", sCellNum ?? "EMPTY", sSMSText ?? "EMPTY"));

                    // Handle request
                    HandleRequest(sCellNum, sepMSG[1], nGroupID);
                }
            }
            catch (Exception ex)
            {
                log.Error("WINPL request - Failed parsing request to xml. Error Message: " + ex.Message + " || Stack Trace: " + ex.StackTrace, ex);
                Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
            }
        }

        public static Int32 GetSMSBaseDetails(string sUN, string sPass, ref string sURL)
        {
            Int32 nGroupID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += " select * from winpl_group_parameters where is_active=1 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_USERNAME", "=", sUN);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IN_PASSWORD", "=", sPass);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    sURL = selectQuery.Table("query").DefaultView[0].Row["URL"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nGroupID;
        }

        private Int32 GetGroupID(string sFormParameters)
        {
            string sUN = "";
            string sPass = "";
            int nUserIndex = sFormParameters.IndexOf("user=", StringComparison.CurrentCultureIgnoreCase);
            if (nUserIndex != -1)
            {
                nUserIndex += 5;
                sUN = sFormParameters.Substring(nUserIndex, sFormParameters.IndexOf("&", nUserIndex) - (nUserIndex));
            }
            else
            {
                log.Debug("WINPL request - Request came in an unexpected format");
                return 0;
            }

            // Get password
            int nPassIndex = sFormParameters.IndexOf("password=", StringComparison.CurrentCultureIgnoreCase);
            if (nPassIndex != -1)
            {
                nPassIndex += 9;
                sPass = sFormParameters.Substring(nPassIndex,
                    sFormParameters.IndexOf("&", nPassIndex) - (nPassIndex));
            }
            else
            {
                log.Debug("WINPL request - Request came in an unexpected format");
                return 0;
            }
            string sURL = "";
            Int32 nGroupID = GetSMSBaseDetails(sUN, sPass, ref sURL);
            return nGroupID;
        }

        protected Int32 GetSMSTokenEntry(string sCellNum, string sToken, Int32 nGroupID, ref double dPrice, ref string sCurrencyCode)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            selectQuery += "select * from sms_codes where is_active=0 and status=1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SMS_CODE", "=", sToken);
            selectQuery += " and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("CELL_PHONE", "=", sCellNum);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
                    dPrice = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                    sCurrencyCode = selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CODE"].ToString();
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }

        protected bool UpdateSMSCodeStatus(Int32 nID, Int32 nStatus)
        {
            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("sms_codes");
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nStatus);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
                updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;
                return true;
            }
            catch (Exception ex)
            {
                log.Error("WINPL request - UpdateSMSCodeStatus - failed updating sms code status. Error:" + ex.Message, ex);
            }

            return false;
        }

        public void HandleRequest(string sCellNum, string sToken, Int32 nGroupID)
        {
            if (string.IsNullOrEmpty(sCellNum) || string.IsNullOrEmpty(sToken))
            {
                log.Debug("WINPL request - HandleRequest - request parameters are invalid. Phone: " + sCellNum + ", Text: " + sToken);
                Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
            }

            double dPrice = 0.0;
            string sCurrencyCode = "";
            Int32 nSMSTokenEntry = GetSMSTokenEntry(sCellNum, sToken, nGroupID, ref dPrice, ref sCurrencyCode);
            if (nSMSTokenEntry != 0)
            {
                if (dPrice > 0.0)
                {
                    if (SMSHelper.SendPremiumSMS(nGroupID, sCellNum, string.Format("Thank you, you can now proceed to watch the movie, your token is: {0}. Reciept #{1}", sToken, nSMSTokenEntry), dPrice, sCurrencyCode))
                    {
                        log.Debug("WINPL request - Premium billing message sent to mobile: " + sCellNum + ", Text: " + sToken);

                        if (UpdateSMSCodeStatus(nSMSTokenEntry, 2))
                        {
                            Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "0"));
                        }
                        else
                        {
                            //Billing.SMSHelper.SendBulkSMS(nGroupID, sCellNum, "System failed to process your request. Please send again", sCurrencyCode);
                            Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                            return;
                        }
                    }
                    else
                    {
                        log.Debug("WINPL request - Failed sending premium sms to mobile: " + sCellNum + ", Text: " + sToken);
                        Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                    }
                }
                else
                {
                    if (SMSHelper.SendBulkSMS(nGroupID, sCellNum, string.Format("Thank you, you can now proceed to watch the movie, your token is: {0}. Reciept #{1}", sToken, nSMSTokenEntry), sCurrencyCode))
                    {
                        log.Debug("WINPL request - Free billing message sent to mobile: " + sCellNum + ", Text: " + sToken);

                        if (UpdateSMSCodeStatus(nSMSTokenEntry, 2))
                        {
                            Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "0"));
                        }
                        else
                        {
                            //Billing.SMSHelper.SendBulkSMS(nGroupID, sCellNum, "System failed to process your request. Please send again", sCurrencyCode);
                            Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                            return;
                        }
                    }
                    else
                    {
                        log.Debug("WINPL request - Failed sending free sms to user. " + sCellNum + ", Text: " + sToken);
                        Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
                    }
                }
            }
            else
            {
                log.Debug("WINPL request - HandleRequest - Failed to get SMS token. Phone: " + sCellNum + ", Text: " + sToken);
                Response.Write(string.Format(WINPLC_RESPONSE_FORMAT, DateTime.Now.ToString("ddMMyyhhmmss"), "301"));
            }
        }
    }
}
