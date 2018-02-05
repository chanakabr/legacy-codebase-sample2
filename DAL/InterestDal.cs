using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Notification;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;

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
        private const string CB_DESIGN_DOC_NOTIFICATION = "notification";

        private static string GetUserInterestKey(int partnerId, int userId)
        {
            return string.Format("user_interests:{0}:{1}", partnerId, userId);
        }

        private static string GetUserListByInterestKey(int groupId, long userId, long interestId)
        {
            return string.Format("user_interests_item:{0}:{1}:{2}", groupId, userId, interestId);
        }

        public static bool SetUserInterestMapping(int groupId, int userId, int notificatonInterestId)
        {
            bool result = false;
            try
            {
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    result = cbManager.Set(GetUserListByInterestKey(groupId, userId, notificatonInterestId), userId);
                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while set user interest notification mapping. number of tries: {0}/{1}. GID: {2}, interest notification ID: {3}. userId: {4}",
                             numOfTries,
                            NUM_OF_TRIES,
                            groupId,
                            notificatonInterestId,
                            userId);
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set user follow notification data. number of tries: {0}/{1}. GID: {2}, interest notification ID: {3}. userId: {4}",
                            numOfTries,
                            NUM_OF_TRIES,
                            groupId,
                            notificatonInterestId,
                            userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting user interest mapping.  GID: {0}, interest notification ID: {1}. userId: {2}. error:{3}",
                  groupId, notificatonInterestId, userId, ex);
            }

            return result;
        }

        public static bool RemoveUserInterestMapping(int groupId, int userId, long notificatonInterestId)
        {
            bool passed = false;
            string userNotificationItemKey = GetUserListByInterestKey(groupId, userId, notificatonInterestId);

            try
            {
                passed = cbManager.Remove(userNotificationItemKey);
                if (passed)
                    log.DebugFormat("Successfully removed {0}", userNotificationItemKey);
                else
                    log.ErrorFormat("Error while removing {0}", userNotificationItemKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing {0}. ex: {1}", userNotificationItemKey, ex);
            }

            return passed;
        }

        public static List<int> GetUsersListbyInterestId(int groupId, int interestId)
        {
            List<int> userIds = null;
            try
            {
                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_NOTIFICATION, "get_users_interests")
                {
                    startKey = new object[] { groupId, interestId },
                    endKey = new object[] { groupId, interestId },
                    staleState = ViewStaleState.False
                };

                // execute request
                userIds = cbManager.View<int>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get users interest notification. GID: {0}, notification ID: {1}, ex: {2}", groupId, interestId, ex);
            }

            return userIds;
        }

        public static InterestNotification InsertTopicInterestNotification(int groupId, string name, string externalId, MessageTemplateType TemplateType, string topicNameValue, string topicInterestId, eAssetTypes assetType, string mailExternalId)
        {
            InterestNotification result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertTopicInterestNotification");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@external_id", externalId);
                sp.AddParameter("@template_type", TemplateType);
                sp.AddParameter("@topic_name_value", topicNameValue);
                sp.AddParameter("@topic_interest_id", topicInterestId);
                sp.AddParameter("@asset_type", (int)assetType);
                sp.AddParameter("@mail_external_id", mailExternalId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotification(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error in InsertTopicInterestNotification. groupId: {0}, name: {1}, externalId: {2}, TemplateType: {3}, topicNameValue: {4}, topicInterestId: {5}, ex: {6}", groupId, name, externalId, TemplateType.ToString(), topicNameValue, topicInterestId, ex);
            }
            return result;
        }

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

        public static List<InterestNotification> GetTopicInterestNotificationsByGroupId(int groupId, List<long> interestNotificationIds)
        {
            List<InterestNotification> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationsByIds");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter<long>("@notificationInterestIds", interestNotificationIds, "id");

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
                log.ErrorFormat("Error at GetTopicInterestNotificationsByGroupId. groupId: {0}, interestNotificationIds: {1}, Error {2}", groupId, JsonConvert.SerializeObject(interestNotificationIds), ex);
            }
            return result;
        }

        public static InterestNotification GetTopicInterestNotificationsById(int groupId, int id)
        {
            InterestNotification result = null;
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
                        result = CreateInterestNotification(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationsById. groupId: {0}, id: {1} . Error {2}", groupId, id, ex);
            }
            return result;
        }

        public static InterestNotification GetTopicInterestNotificationsByTopicNameValue(int groupId, string topicNameValue, eAssetTypes assetType)
        {
            InterestNotification result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationsByTopicNameValue");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@topicNameValue", topicNameValue);
                sp.AddParameter("@asset_type", (int)assetType);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotification(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetTopicInterestNotificationsByTopicNameValue. groupId: {0}, topicNameValue: {1} . Error {2}", groupId, topicNameValue, ex);
            }
            return result;
        }

        public static InterestNotification UpdateTopicInterestNotification(int groupId, int id, string externalId = null, DateTime? lastMessageSentDate = null, string queueName = null, string mailExternalId = null)
        {
            InterestNotification result = null;
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

                if (!string.IsNullOrEmpty(externalId))
                    sp.AddParameter("@mailExternalId", mailExternalId);
                else
                    sp.AddParameter("@mailExternalId", DBNull.Value);

                DataSet ds = sp.ExecuteDataSet();
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotification(ds.Tables[0].Rows[0]);
                }
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

            return result;
        }

        public static bool DeleteTopicInterestNotification(int groupId, long id)
        {
            int affectedRows = 0;
            try
            {
                ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("DeleteTopicInterestNotification");
                spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                spInsertUserNotification.AddParameter("@id", id);
                spInsertUserNotification.AddParameter("@group_id", groupId);
                affectedRows = spInsertUserNotification.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at DeleteTopicInterestNotification. groupId: {0}, id: {1} . Error {2}", groupId, id, ex);
            }
            return affectedRows > 0;
        }

        public static InterestNotificationMessage InsertTopicInterestNotificationMessage(int groupId, string name, string message, DateTime sendTime, int topicInterestNotificationId, int referenceAssetId)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertTopicInterestNotificationMessage");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@group_id", groupId);
                sp.AddParameter("@name", name);
                sp.AddParameter("@message", message);
                sp.AddParameter("@send_time", sendTime);
                sp.AddParameter("@topic_interests_notifications_id", topicInterestNotificationId);
                sp.AddParameter("@reference_asset_id", referenceAssetId);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("error in InsertTopicInterestNotificationMessage. groupId: {0}, name: {1}, message: {2}, sendTime: {3}, topicInterestNotificationId: {4}, ex: {5}", groupId, name, message, sendTime, topicInterestNotificationId, ex);
            }
            return result;
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

        public static InterestNotificationMessage GetTopicInterestNotificationMessageByInterestNotificationId(int groupId, int interestNotificationId, int referenceAssetId)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetTopicInterestNotificationMessageByInterestNotificationId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@Interest_Notification_ID", interestNotificationId);
                sp.AddParameter("@reference_asset_id", referenceAssetId);
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

        public static InterestNotificationMessage UpdateTopicInterestNotificationMessage(int groupId, int id, DateTime? sendTime = null, string message = null, bool? isSent = null, string pushResultMessageId = null, DateTime? responseDate = null)
        {
            InterestNotificationMessage result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateTopicInterestNotificationMessage");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);

                if (sendTime.HasValue)
                    sp.AddParameter("@send_time", sendTime.Value);

                if (!string.IsNullOrEmpty(message))
                    sp.AddParameter("@message", message);

                if (isSent.HasValue)
                    sp.AddParameter("@sent", (bool)isSent ? 1 : 0);

                if (!string.IsNullOrEmpty(pushResultMessageId))
                    sp.AddParameter("@push_result_message_id", pushResultMessageId);

                if (responseDate.HasValue)
                    sp.AddParameter("@push_response_date", responseDate.Value);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                        result = CreateInterestNotificationMessage(ds.Tables[0].Rows[0]);
                }
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
                TopicInterestsNotificationsId = ODBCWrapper.Utils.GetIntSafeVal(row, "topic_interests_notifications_id"),
                ReferenceAssetId = ODBCWrapper.Utils.GetIntSafeVal(row, "reference_asset_id")
            };
        }

        private static InterestNotification CreateInterestNotification(DataRow row)
        {
            int templateType = ODBCWrapper.Utils.GetIntSafeVal(row, "template_type");
            int assetType = ODBCWrapper.Utils.GetIntSafeVal(row, "asset_type");

            return new InterestNotification()
            {
                Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                ExternalPushId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                LastMessageSentDateSec = ODBCWrapper.Utils.GetIntSafeVal(row, "last_message_sent_date_sec"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                QueueName = ODBCWrapper.Utils.GetSafeStr(row, "queue_name"),
                TopicInterestId = ODBCWrapper.Utils.GetSafeStr(row, "topic_interest_id"),
                TopicNameValue = ODBCWrapper.Utils.GetSafeStr(row, "topic_name_value"),
                TemplateType = Enum.IsDefined(typeof(MessageTemplateType), templateType) ? (MessageTemplateType)templateType : MessageTemplateType.None,
                AssetType = Enum.IsDefined(typeof(eAssetTypes), assetType) ? (eAssetTypes)assetType : eAssetTypes.UNKNOWN
            };
        }

        public static bool SetUserInterest(UserInterests userInterests)
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
                    result = cbManager.Set(GetUserInterestKey(userInterests.PartnerId, userInterests.UserId), userInterests, (uint)TimeSpan.FromDays(userInterestTtl).TotalSeconds);
                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting user interest document. number of tries: {0}/{1}. User interest object: {2}",
                             numOfTries,
                            NUM_OF_TRIES,
                            JsonConvert.SerializeObject(userInterests));

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
                            JsonConvert.SerializeObject(userInterests));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting user interest document.  User interest object: {0}, ex: {1}", JsonConvert.SerializeObject(userInterests), ex);
            }

            return result;
        }

        public static UserInterests GetUserInterest(int partnerId, int userId)
        {
            UserInterests userInterests = null;
            eResultStatus status = eResultStatus.ERROR;
            string key = GetUserInterestKey(partnerId, userId);

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_TRIES)
                {
                    userInterests = cbManager.Get<UserInterests>(key, out status);
                    if (userInterests == null)
                    {
                        if (status == eResultStatus.KEY_NOT_EXIST)
                        {
                            return userInterests;
                        }
                        else if (status == eResultStatus.ERROR)
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

            return userInterests;
        }
    }
}
