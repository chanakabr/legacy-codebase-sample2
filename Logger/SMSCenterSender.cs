using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

namespace Logger
{
    class SMSCenterSender
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        static protected string GetWSUN()
        {
            if (Utils.GetTcmConfigValue("SMS_WS_UN") != string.Empty)
                return Utils.GetTcmConfigValue("SMS_WS_UN");
            return "";
        }

        static protected string GetWSPass()
        {
            if (Utils.GetTcmConfigValue("SMS_WS_PASS") != string.Empty)
                return Utils.GetTcmConfigValue("SMS_WS_PASS");
            return "";
        }

        public static bool SendSMS(string sSenderName, string sMessage, string AppSmsKey)
        {
            try
            {

                string sUserName = GetWSUN();
                string sPassword = GetWSPass();
                string[] sep = { ";" };
                object[] sPhones = Utils.GetTcmConfigValue(AppSmsKey).Split(sep, StringSplitOptions.RemoveEmptyEntries);
                object[] sMails = Utils.GetTcmConfigValue("SMS_WS_MAILS").Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (sPhones.Length > 0)
                {
                    string sSMSMessage = sMessage;
                    //if (sMessage.Length > 65)
                    //sSMSMessage = sMessage.Substring(0, 65) + "...";

                    il.co.smscenter.www.SendSMS sender = new il.co.smscenter.www.SendSMS();
                    sender.Url = Utils.GetTcmConfigValue("SMS_WS_URL");
                    il.co.smscenter.www.SendMessageReturnValues ret = sender.SendMessages(sUserName, sPassword, sSenderName,
                        sPhones, sSMSMessage, sMails, il.co.smscenter.www.SMSOperation.Push, "", il.co.smscenter.www.DeliveryReportMask.MessageExpired, 0, 60);
                    log.Debug("SMS sent - returned: " + ret.ToString() + sMessage);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                log.Error("SMS send - " + ex.Message + "||" + ex.StackTrace, ex);
                return false;
            }
        }
        public static bool SendSMS(string sSenderName, string sMessage)
        {
            return SendSMS(sSenderName, sMessage, "SMS_WS_PHONES");


        }
    }
}
