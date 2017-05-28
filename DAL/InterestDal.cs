using ApiObjects;
using ApiObjects.Notification;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Reflection;
using System.Threading;

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
