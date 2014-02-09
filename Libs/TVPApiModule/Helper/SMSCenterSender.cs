using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace TVPApi
{
    private readonly ILog m_logger = log4net.LogManager.GetLogger("ClientSideService");

    class SMSCenterSender
    {
        static protected string GetWSUN()
        {
            if (ConfigurationManager.AppSettings["SMS_WS_UN"] != null &&
                ConfigurationManager.AppSettings["SMS_WS_UN"].ToString() != "")
                return ConfigurationManager.AppSettings["SMS_WS_UN"].ToString();
            return "";
        }

        static protected string GetWSPass()
        {
            if (ConfigurationManager.AppSettings["SMS_WS_PASS"] != null &&
                ConfigurationManager.AppSettings["SMS_WS_PASS"].ToString() != "")
                return ConfigurationManager.AppSettings["SMS_WS_PASS"].ToString();
            return "";
        }

        public static bool SendSMS(string sSenderName , string sMessage)
        {
            try
            {
                string sUserName = GetWSUN();
                string sPassword = GetWSPass();
                string[] sep = { ";" };
                object[] sPhones = ConfigurationManager.AppSettings["SMS_WS_PHONES"].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                object[] sMails = ConfigurationManager.AppSettings["SMS_WS_MAILS"].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (sPhones.Length > 0)
                {
                    string sSMSMessage = sMessage;
                    //if (sMessage.Length > 65)
                        //sSMSMessage = sMessage.Substring(0, 65) + "...";

                    SMSCenter.SendSMS sender = new SMSCenter.SendSMS();
                    sender.Url = ConfigurationManager.AppSettings["SMS_WS_URL"];
                    SMSCenter.SendMessageReturnValues ret = sender.SendMessages(sUserName, sPassword, sSenderName,
                        sPhones, sSMSMessage, sMails, SMSCenter.SMSOperation.Push, "", SMSCenter.DeliveryReportMask.MessageExpired, 0, 60);
                    // TODO: Logger.Log("SMS sent - returnd: " + ret.ToString(), sMessage, "SMSer");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // TODO: Logger.Log("SMS send", ex.Message + "||" + ex.StackTrace, "SMSer");
                return false;
            }
        }
    }
}
