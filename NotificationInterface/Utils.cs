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
    }
}


