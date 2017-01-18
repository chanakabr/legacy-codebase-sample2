using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;
using ApiObjects.Notification;
using KLogMonitor;

namespace Core.Notification
{
    public class ElisaSMSNotification : SMSNotification
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region members for elisa sms service

        public string m_tariffClass = string.Empty;
        public string m_serviceDescription = string.Empty;
        public string m_operator = string.Empty;

        #endregion



        public ElisaSMSNotification()
            : base()
        {
        }

        public override void Send(NotificationRequest request)
        {
            try
            {
                //Build SMS message 
                NotificationMessage smsMessage = GetSMSMessage(request);
                BuildSMS(smsMessage, request.TriggerType);
                //Get User Phone number 
                m_userPhoneNumber = GetUserPhoneNumber(request);
                //get Elisa Details 
                String result = sendSMSNotification(smsMessage.MessageText);
                smsMessage.Status = NotificationMessageStatus.Successful;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ElisaSMSNotification - Send,  Exception = {0} ", ex.Message));
                throw ex;
            }
        }
        
        //get UserPhone
        private string GetUserPhoneNumber(NotificationRequest request)
        {
            try
            {
                string sPhoneNumber = string.Empty;
                sPhoneNumber = ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("users", "PHONE", int.Parse(request.UserID.ToString()), "USERS_CONNECTION_STRING"));
                sPhoneNumber = sPhoneNumber.Replace(")", string.Empty).Replace("(", string.Empty).Replace("-", string.Empty);

                return sPhoneNumber;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        
        //get values from DB to create the URL 
        private void BuildSMS(NotificationMessage smsMessage, NotificationTriggerType eTriggerTypr)
        {
            //get group parameters for sms
            DataTable dt = DAL.NotificationDal.GetGroupOperator(smsMessage.nGroupID, (int)eTriggerTypr);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                m_username = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["user_name"]);
                m_password = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["password"]);
                m_smsURL = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["smsURL"]);
                string additions = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["additions"]);//get from here all the params 
                JavaScriptSerializer js = new JavaScriptSerializer();
                ElisaSMSNotification obj = js.Deserialize<ElisaSMSNotification>(additions);
                m_tariffClass = obj.m_tariffClass;
                m_operator = obj.m_operator;
                m_serviceDescription = obj.m_serviceDescription;
            }
        }

        private static NotificationMessage GetSMSMessage(NotificationRequest request)
        {
            NotificationMessageType messageType = request.MessageType;
            NotificationRequestAction[] actions = request.Actions;
            string appName = GetAppNameFromConfig(request.GroupID);
            NotificationMessage smsMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, request.UserID, NotificationMessageStatus.NotStarted, request.SmsMessageText, request.Title, appName, string.Empty, 0, request.Actions, request.oExtraParams, request.GroupID);
            return smsMessage;
        }

        // this method is called from news service core
        private String sendSMSNotification(String messageText)
        {
            try
            {
                String sMessageText = messageText;
                String sResponse = "OK";
                List<string> messageSplit = new List<string>();
                List<string> lRespons = new List<string>();
                int nParts = 0; //part message numbers 
                int chunkSize = 140;
                for (int i = 0; i < messageText.Length ; i = i + 140) //140 caracters in each part of the message
                {
                    nParts ++;
                    if (messageText.Length - i < chunkSize)
                        chunkSize = messageText.Length - i;
                    messageSplit.Add(messageText.Substring(i, chunkSize));
                }

                ///Build userdataheader
                ///UDH-DATA: 05 00 03 5F 02 01
                ///05 = 5 bytes follow
                ///00 = indicator for concatenated message
                ///03 = three bytes follow
                ///5F = message identification. Each part has the same value here
                ///02 = the concatenated message has 2 parts
                ///01 = this is part 1
                string userdataheader = "050003";
                Random random = new Random(); // message identification
                int num = random.Next(255);
                string hexString = num.ToString("X").Substring(0,2);
                userdataheader += hexString;
                userdataheader += nParts.ToString("D2");//add leading zero to parts numbers
                
                string userdatabinary = string.Empty;

                for (int i = 1; i <= nParts; i++)
                {
                    userdataheader += i.ToString("D2");//add leading zero to part number
                    // create url to smsbroker
                    String parameters = "destinationaddress=" + HttpUtility.UrlEncode(m_userPhoneNumber);
                    parameters += "&opcode=" + HttpUtility.UrlEncode(m_operator);
                    parameters += "&userdataheader=" + HttpUtility.UrlEncode(userdataheader);
                    parameters += "&userdata=" + HttpUtility.UrlEncode(messageSplit[i-1]);                   

                    parameters += "&tariffclass=" + HttpUtility.UrlEncode(m_tariffClass);
                    parameters += "&serviceDescription=" + HttpUtility.UrlEncode(m_serviceDescription);

                    // add my username and password
                    parameters += "&uid=" + HttpUtility.UrlEncode(m_username);
                    parameters += "&pwd=" + HttpUtility.UrlEncode(m_password);

                    // create connection
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_smsURL + parameters);

                    request.Method = "GET";

                    // send message              
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            lRespons.Add("OK");
                            break;
                        default:
                            lRespons.Add("Failed");
                            break;
                    }
                    userdataheader = userdataheader.Substring(0, userdataheader.Length - 2);// "clean" the part number for next iteration
                    // Releases the resources of the response.
                    response.Close();
                }
                if (lRespons.Contains("Failed"))
                    sResponse = "Failed";
                return sResponse;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("sendSMSNotification Elisa , Exception Message = {0}", ex.Message));
                throw ex;
            }
        }
    }
}
