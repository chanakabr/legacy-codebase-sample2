using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Notification;
using RestfulTVPApi.Objects.Responses;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients
{
    public class NotificationsClient : BaseClient
    {
        #region Variables

        private readonly ILog logger = LogManager.GetLogger(typeof(NotificationsClient));
        private static object instanceLock = new object();

        #endregion

        #region CTOR

        public NotificationsClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
           
        }

        public NotificationsClient()
        {

        }

        #endregion

        #region Properties

        protected RestfulTVPApi.Notification.NotificationServiceClient Notification
        {
            get
            {
                return (Module as RestfulTVPApi.Notification.NotificationServiceClient);
            }
        }

        #endregion


        public List<RestfulTVPApi.Objects.Responses.Notification> GetDeviceNotifications(string sGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            List<RestfulTVPApi.Objects.Responses.Notification> res = new List<RestfulTVPApi.Objects.Responses.Notification>();

            res = Execute(() =>
                {
                    var notificationMessages = Notification.GetDeviceNotifications(WSUserName, WSPassword, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
                    if (notificationMessages != null)
                    {
                        foreach (var message in notificationMessages)
                        {
                            res.Add(new RestfulTVPApi.Objects.Responses.Notification()
                            {
                                actions = message.Actions != null ? message.Actions : null,
                                app_name = message.AppName,
                                DeviceID = message.DeviceID,
                                id = message.ID,
                                message_text = message.MessageText,
                                nGroupID = message.nGroupID,
                                notification_id = message.NotificationID,
                                notification_message_id = message.NotificationMessageID,
                                status = message.Status,
                                title = message.Title,
                                type = message.Type,
                                udid = message.UdID,
                                user_id = message.UserID,
                                tag_notification_params = message.TagNotificationParams != null ? new ExtraParameters()
                                {
                                    media_id = message.TagNotificationParams.mediaIDk__BackingField,
                                    media_pic_url = message.TagNotificationParams.mediaPicURLk__BackingField,
                                    tag_dict = message.TagNotificationParams.TagDictk__BackingField != null ? message.TagNotificationParams.TagDictk__BackingField.Select(x => new TagMetaIntPairArray() { key = x.Key, values = x.Value }).ToList() : null,
                                    template_email = message.TagNotificationParams.templateEmailk__BackingField
                                } : null,
                                publish_date = message.PublishDate,
                                view_status = message.ViewStatus,
                                notification_request_id = message.NotificationRequestID
                            });
                        }
                    }

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.Notification>;

            return res;
        }

        public bool SetNotificationMessageViewStatus(string sGuid, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Notification.SetNotificationMessageViewStatus(WSUserName, WSPassword, sGuid, notificationRequestID, notificationMessageID, viewStatus);
                    return res;
                }));

            return res;
        }

        public bool AddNotificationRequest(string sGuid, NotificationTriggerType triggerType, int mediaId)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Notification.AddNotificationRequest(WSUserName, WSPassword, sGuid, triggerType, mediaId);
                    return res;
                }));

            return res;
        }

        public bool SubscribeByTag(string sGuid, List<TagMetaPairArray> tags)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    Dictionary<string, List<string>> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values != null ? valueItem.values.ToList() : null);
                    res = Notification.SubscribeByTag(WSUserName, WSPassword, sGuid, dictTags);

                    return res;
                }));

            return res;
        }

        public bool UnsubscribeFollowUpByTag(string sGuid, List<TagMetaPairArray> tags)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    Dictionary<string, List<string>> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values != null ? valueItem.values.ToList() : null);
                    res = Notification.UnsubscribeFollowUpByTag(WSUserName, WSPassword, sGuid, dictTags);

                    return res;
                }));

            return res;
        }

        public List<TagMetaPairArray> GetUserStatusSubscriptions(string sGuid)
        {
            //Dictionary<string, string[]> clientRes = new Dictionary<string, string[]>();
            List<TagMetaPairArray> finalRes = new List<TagMetaPairArray>();

            finalRes = Execute(() =>
                {
                    Dictionary<string, List<string>> clientRes = new Dictionary<string, List<string>>();
                    clientRes = Notification.GetUserStatusSubscriptions(WSUserName, WSPassword, sGuid);

                    // convert to list
                    if (clientRes != null)
                    {
                        foreach (var entry in clientRes)
                            finalRes.Add(new TagMetaPairArray() { key = entry.Key, values = entry.Value != null ? entry.Value.ToArray() : null });

                        return finalRes;
                    }

                    return finalRes;
                }) as List<TagMetaPairArray>;

            return finalRes;
        }
    }
}