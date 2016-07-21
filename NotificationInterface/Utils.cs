using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using NotificationObj;
using ApiObjects;
using System.Data;
using System.Web;
using System.Net;
using System.ServiceModel;

namespace NotificationInterface
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());



        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static string ExtractDate(DateTime date, string format)
        {
            try
            {
                string result = date.ToString(format);
                return result;
            }
            catch (FormatException ex)
            {
                log.Error("", ex);
                return string.Empty;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
                return string.Empty;
            }
        }

        public static MessageAnnouncement GetMessageAnnouncementFromDataRow(DataRow row)
        {
            string timezone = ODBCWrapper.Utils.GetSafeStr(row, "timezone");

            DateTime convertedTime = ODBCWrapper.Utils.ConvertFromUtc(ODBCWrapper.Utils.GetDateSafeVal(row, "start_time"), timezone);
            long startTime = ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(convertedTime);
            ApiObjects.eAnnouncementRecipientsType recipients = ApiObjects.eAnnouncementRecipientsType.Other;
            int dbRecipients = ODBCWrapper.Utils.GetIntSafeVal(row, "recipients");
            if (Enum.IsDefined(typeof(ApiObjects.eAnnouncementRecipientsType), dbRecipients))
                recipients = (ApiObjects.eAnnouncementRecipientsType)dbRecipients;

            eAnnouncementStatus status = eAnnouncementStatus.NotSent;
            int dbStatus = ODBCWrapper.Utils.GetIntSafeVal(row, "sent");
            if (Enum.IsDefined(typeof(eAnnouncementStatus), dbStatus))
                status = (eAnnouncementStatus)dbStatus;

            MessageAnnouncement msg = new MessageAnnouncement(ODBCWrapper.Utils.GetSafeStr(row, "name"),
                                                              ODBCWrapper.Utils.GetSafeStr(row, "message"),
                                                              (ODBCWrapper.Utils.GetIntSafeVal(row, "is_active") == 0) ? false : true,
                                                              startTime,
                                                              timezone,
                                                              recipients,
                                                              status,
                                                              ODBCWrapper.Utils.GetSafeStr(row, "message_reference"),
                                                              ODBCWrapper.Utils.GetIntSafeVal(row, "announcement_id"));

            msg.MessageAnnouncementId = ODBCWrapper.Utils.GetIntSafeVal(row, "id");

            return msg;
        }

        ///<summary>
        /// Base 64 Encoding with URL and Filename Safe Alphabet using UTF-8 character set.
        ///</summary>
        ///<param name="str">The original string</param>
        ///<returns>The Base64 encoded string</returns>
        public static string Base64ForUrlEncode(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return HttpServerUtility.UrlTokenEncode(encbuff);
        }
        ///<summary>
        /// Decode Base64 encoded string with URL and Filename Safe Alphabet using UTF-8.
        ///</summary>
        ///<param name="str">Base64 code</param>
        ///<returns>The decoded string.</returns>
        public static string Base64ForUrlDecode(string str)
        {
            byte[] decbuff = HttpServerUtility.UrlTokenDecode(str);
            return Encoding.UTF8.GetString(decbuff);
        }
    }
}

namespace NotificationInterface.WS_Api
{
    // adding request ID to header
    public partial class API
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}

namespace NotificationInterface.WS_Users
{
    // adding request ID to header
    public partial class UsersService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}
