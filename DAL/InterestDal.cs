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
        private const int TTL_USER_INTEREST_DAYS = 30;

        public static List<InterestNotification> GetTopicInterestNotificationsByGroupId(int groupId)
        {
            List<InterestNotification> result = new List<InterestNotification>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationsByGroupId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                            result.Add(CreateInterestNotification(row));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationsByGroupId. groupId: {0}, Error {1}", groupId, ex);
            }
            return result;
        }

        public static InterestNotificationMessage GetTopicInterestNotificationsById(int groupId, int id)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationsById");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@ID", id);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationsById. groupId: {0}, id: {1} . Error {2}", groupId, id, ex);
            }
            return result;
        }

        public static InterestNotificationMessage GetTopicInterestNotificationsByTopicNameValue(int groupId, string topicNameValue)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationsByTopicNameValue");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@topicNameValue", topicNameValue);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationsByTopicNameValue. groupId: {0}, topicNameValue: {1} . Error {2}", groupId, topicNameValue, ex);
            }
            return result;
        }

        public static bool DeleteTopicInterestNotification(int groupId, long id)
        {
            int affectedRows = 0;
            try
            {
                ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("DeleteTopicInterestsNotification");
                spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                spInsertUserNotification.AddParameter("@id", id);
                spInsertUserNotification.AddParameter("@groupId", groupId);
                affectedRows = spInsertUserNotification.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at DeleteTopicInterestNotification. groupId: {0}, id: {1} . Error {2}", groupId, id, ex);
            }
            return affectedRows > 0;
        }

        public static bool UpdateTopicInterestNotification(int groupId, int id, string externalId = null, DateTime? lastMessageSentDate = null, string queueName = null)
        {
            int rowCount = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_TopicInterestNotification");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);
                if (!string.IsNullOrEmpty(externalId))
                    sp.AddParameter("@externalId", externalId);
                else
                    sp.AddParameter("@externalId", DBNull.Value);

                if (lastMessageSentDate.HasValue)
                    sp.AddParameter("@lastMessageSentDateSec", lastMessageSentDate.Value);

                if (!string.IsNullOrEmpty(queueName))
                    sp.AddParameter("@queueName", queueName);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at UpdateTopicInterestNotification. groupId: {0}, id: {1}, external ID: {2}, lastMessageSentDate: {3}, queueName: {4}, ex: {5}",
                    groupId,
                    id,
                    externalId,
                    lastMessageSentDate,
                    queueName,
                    ex);
            }

            return rowCount > 0;
        }

        public static InterestNotificationMessage GetTopicInterestsNotificationMessageById(int groupId, int id)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestsNotificationMessageById");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@ID", id);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestsNotificationMessageById. groupId: {0}, id: {1} . Error {2}", groupId, id, ex);
            }
            return result;
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

        public static bool UpdateTopicInterestNotificationMessage(int groupId, int id, string message = null, bool? isSent = null, string pushResultMessageId = null, DateTime? responseDate = null)
        {
            int rowCount = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateTopicInterestNotificationMessage");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);

                if (!string.IsNullOrEmpty(message))
                    sp.AddParameter("@message", message);

                if (isSent.HasValue)
                    sp.AddParameter("@sent", (bool)isSent ? 1 : 0);

                if (!string.IsNullOrEmpty(pushResultMessageId))
                    sp.AddParameter("@push_result_message_id", pushResultMessageId);

                if (responseDate.HasValue)
                    sp.AddParameter("@push_response_date", responseDate.Value);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at UpdateTopicInterestNotificationMessage. groupId: {0}, id: {1}, message: {2}, isSent: {3}, pushResultMessageId: {4}, responseDate: {5}, ex: {6}",
                    groupId,
                    id,
                    message,
                    isSent,
                    pushResultMessageId,
                    responseDate != null ? responseDate.ToString() : "null",
                    ex);
            }

            return rowCount > 0;
        }

        private static InterestNotificationMessage CreateInterestNotificationMessage(DataRow row)
        {
            return new InterestNotificationMessage()
            {
                Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                Message = ODBCWrapper.Utils.GetSafeStr(row, "MESSAGE"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "NAME"),
                SendTime = ODBCWrapper.Utils.GetDateSafeVal(row, "send_time"),
                TopicInterestsNotificationsId = ODBCWrapper.Utils.GetSafeStr(row, "topic_interests_notifications_id")
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

        public static int InsertTopicInterestNotification(int groupId, string name, string externalId, MessageTemplateType TemplateType, string topicNameValue, int topicInterestId)
        {
            int id = 0;
            try
            {
                ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("InsertTopicInterestNotification");
                spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                spInsert.AddParameter("@group_id", groupId);
                spInsert.AddParameter("@name", name);
                spInsert.AddParameter("@external_id", externalId);
                spInsert.AddParameter("@template_type", TemplateType);
                spInsert.AddParameter("@topic_name_value", topicNameValue);
                spInsert.AddParameter("@topic_interest_id", topicInterestId);

                id = spInsert.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error in InsertTopicInterestNotification. groupId: {0}, name: {1}, externalId: {2}, TemplateType: {3}, topicNameValue: {4}, topicInterestId: {5}, ex: {6}", groupId, name, externalId, TemplateType.ToString(), topicNameValue, topicInterestId, ex);
            }
            return id;
        }

        public static int InsertTopicInterestNotificationMessage(int groupId, string name, string message, DateTime sendTime, int topicInterestNotificationId)
        {
            int id = 0;
            try
            {
                ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("InsertTopicInterestNotificationMessage");
                spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                spInsert.AddParameter("@group_id", groupId);
                spInsert.AddParameter("@name", name);
                spInsert.AddParameter("@message", message);
                spInsert.AddParameter("@send_time", sendTime);
                spInsert.AddParameter("@message_type", topicInterestNotificationId);

                id = spInsert.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error in InsertTopicInterestNotificationMessage. groupId: {0}, name: {1}, message: {2}, sendTime: {3}, topicInterestNotificationId: {4}, ex: {5}", groupId, name, message, sendTime, topicInterestNotificationId, ex);
            }
            return id;
        }

        private static string GetUserInterestKey(int partnerId, int userId)
        {
            return string.Format("user_interests:{0}:{1}", partnerId, userId);
        }

        public static bool SetUserInterest(UserInterest userInterest)
        {
            bool result = false;
            try
            {
                // get user interest TTL
                int userInterestTtl = TCMClient.Settings.Instance.GetValue<int>("ttl_user_interest_days");
                if (userInterestTtl == 0)
                    userInterestTtl = TTL_USER_INTEREST_DAYS;

                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    result = cbManager.Set(GetUserInterestKey(userInterest.PartnerId, userInterest.UserId), userInterest, (uint)TimeSpan.FromDays(userInterestTtl).TotalSeconds);
                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting user interest document. number of tries: {0}/{1}. User interest object: {2}",
                             numOfTries,
                            NUM_OF_TRIES,
                            JsonConvert.SerializeObject(userInterest));

                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set user interest document. number of tries: {0}/{1}. User interest object {2}",
                            numOfTries,
                            NUM_OF_TRIES,
                            JsonConvert.SerializeObject(userInterest));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting user interest document.  User interest object: {0}, ex: {1}", JsonConvert.SerializeObject(userInterest), ex);
            }

            return result;
        }

        public static UserInterest GetUserInterest(int partnerId, int userId)
        {
            UserInterest userInterest = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            string key = GetUserInterestKey(partnerId, userId);

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    userInterest = cbManager.Get<UserInterest>(key, out status);
                    if (userInterest == null)
                    {
                        if (status != Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while getting user interest data. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_TRIES,
                                key);

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        result = true;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully received user interest data. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_TRIES,
                            key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user interest data. key: {0}, ex: {1}", key, ex);
            }

            return userInterest;
        }
    }
}
