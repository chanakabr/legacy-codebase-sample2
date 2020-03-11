using System;
using System.Collections.Generic;
using System.Linq;
using TVPApi;
using TVPApiModule.Objects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Notification;
using Notification = TVPApiModule.Objects.Notification;
using TVPApiModule.Manager;

namespace TVPApiModule.Services
{
    public class ApiNotificationService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object instanceLock = new object();
        private int m_groupID;
        private PlatformType m_platform;
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        public ApiNotificationService(int groupID, PlatformType platform)
        {
            m_wsUserName = GroupsManager.GetGroup(groupID).NotificationsCredentials.Username;
            m_wsPassword = GroupsManager.GetGroup(groupID).NotificationsCredentials.Password;

            m_groupID = groupID;
            m_platform = platform;
        }

        public List<Notification> GetDeviceNotifications(string sGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            List<Notification> res = new List<Notification>();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var notificationMessages = Core.Notification.Module.GetDeviceNotifications(m_groupID, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
                    if (notificationMessages != null)
                    {
                        foreach (var message in notificationMessages)
                        {
                            res.Add(new Notification()
                            {
                                Actions = message.Actions != null ? message.Actions : null,
                                AppName = message.AppName,
                                DeviceID = message.DeviceID,
                                ID = message.ID,
                                MessageText = message.MessageText,
                                nGroupID = message.nGroupID,
                                NotificationID = message.NotificationID,
                                NotificationMessageID = message.NotificationMessageID,
                                Status = message.Status,
                                Title = message.Title,
                                Type = message.Type,
                                UdID = message.UdID,
                                UserID = message.UserID,
                                TagNotificationParams = message.TagNotificationParams != null ? new ExtraParameters()
                                {
                                    mediaID = message.TagNotificationParams.mediaID,
                                    mediaPicURL = message.TagNotificationParams.mediaPicURL,
                                    TagDict = message.TagNotificationParams.TagDict != null ? message.TagNotificationParams.TagDict.Select(
                                        x => new TagMetaIntPairArray() { Key = x.Key, Values = x.Value.ToArray() }).ToList() : null,
                                    templateEmail = message.TagNotificationParams.templateEmail
                                } : null,
                                PublishDate = message.PublishDate,
                                ViewStatus = message.ViewStatus,
                                NotificationRequestID = message.NotificationRequestID
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetDeviceNotifications, Error : {0} Parameters : siteGuid {1}, sDeviceUDID: {2}", e.Message, sGuid, sDeviceUDID);
            }

            return res;
        }

        public bool SetNotificationMessageViewStatus(string sGuid, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Notification.Module.SetNotificationMessageViewStatus(m_groupID, sGuid, notificationRequestID, notificationMessageID, viewStatus);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in SetNotificationMessageViewStatus, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public bool AddNotificationRequest(string sGuid, NotificationTriggerType triggerType, int mediaId)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Notification.Module.AddNotificationRequest(m_groupID, sGuid, triggerType, mediaId);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in AddNotificationRequest, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public bool SubscribeByTag(string sGuid, List<TVPApi.TagMetaPairArray> tags)
        {
            bool res = false;
            try
            {
                Dictionary<string, List<string>> dictTags = tags.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Values.ToList());
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Notification.Module.SubscribeByTag(m_groupID, sGuid, dictTags);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in SubscribeByTag, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public bool UnsubscribeFollowUpByTag(string sGuid, List<TVPApi.TagMetaPairArray> tags)
        {
            bool res = false;
            try
            {
                Dictionary<string, List<string>> dictTags = tags.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Values.ToList());
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Notification.Module.UnsubscribeFollowUpByTag(m_groupID, sGuid, dictTags);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in UnsubscribeFollowUpByTag, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public List<TVPApi.TagMetaPairArray> GetUserStatusSubscriptions(string sGuid)
        {
            Dictionary<string, List<string>> clientRes = new Dictionary<string, List<string>>();
            List<TVPApi.TagMetaPairArray> finalRes = new List<TagMetaPairArray>();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    clientRes = Core.Notification.Module.GetUserStatusSubscriptions(m_groupID, sGuid);
                }

                // convert to list
                if (clientRes != null)
                {
                    foreach (var entry in clientRes)
                        finalRes.Add(new TagMetaPairArray() { Key = entry.Key, Values = entry.Value });

                    return finalRes;
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in GetUserStatusSubscriptions, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return finalRes;
        }
    }
}
