using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Notification;
using CouchbaseManager;
using KLogMonitor;

namespace DAL
{
    public class InterestDal
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.NOTIFICATION);
        private const string MESSAGE_BOX_CONNECTION = "MESSAGE_BOX_CONNECTION_STRING";
        private const string CB_DESIGN_DOC_ENGAGEMENT = "interests";
        private const int SLEEP_BETWEEN_RETRIES_MILLI = 1000;
        private const int NUM_OF_TRIES = 3;

        public static bool DeleteTopicInterestNotification(int groupId, long id)
        {
            int affectedRows = 0;

            ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("DeleteTopicInterestsNotification");
            spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertUserNotification.AddParameter("@id", id);
            spInsertUserNotification.AddParameter("@groupId", groupId);
            affectedRows = spInsertUserNotification.ExecuteReturnValue<int>();

            return affectedRows > 0;
        }

        public static InterestNotificationMessage GetTopicInterestNotificationMessageByInterestNotificationId(int groupId, int interestNotificationId)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationMessageByInterestNotificationId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@Interest_Notification_ID", interestNotificationId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationMessageByInterestNotificationId. groupId: {0}, interestNotificationId: {1} . Error {2}", groupId, interestNotificationId, ex);
            }

            return result;

        }

        private static InterestNotificationMessage CreateInterestNotificationMessage(DataRow row)
        {
            return new InterestNotificationMessage()
            {
                Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                Message = ODBCWrapper.Utils.GetSafeStr(row, "MESSAGE"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                SendTime = ODBCWrapper.Utils.GetDateSafeVal(row, "send_time"),
                TopicInterestsNotificationsId = ODBCWrapper.Utils.GetSafeStr(row, "NAME")
            };
        }

        private static InterestNotification CreateInterestNotification(DataRow row)
        {
            int templateType = ODBCWrapper.Utils.GetIntSafeVal(row, "template_type");

            return new InterestNotification()
            {
                Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                ExternalId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                LastMessageSentDateSec = ODBCWrapper.Utils.GetIntSafeVal(row, "last_message_sent_date_sec"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                QueueName = ODBCWrapper.Utils.GetSafeStr(row, "queue_name"),
                TopicInterestId = ODBCWrapper.Utils.GetIntSafeVal(row, "topic_interest_id"),
                TopicNameValue = ODBCWrapper.Utils.GetSafeStr(row, "topic_name_value"),
                TemplateType = Enum.IsDefined(typeof(MessageTemplateType), templateType) ? (MessageTemplateType)templateType : MessageTemplateType.None
            };
        }
    }
}
