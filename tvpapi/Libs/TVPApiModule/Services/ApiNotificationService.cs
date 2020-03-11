using System;
using System.Collections.Generic;
using System.Linq;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Objects;
using KLogMonitor;
using System.Reflection;

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
        private NotificationServiceClient m_Client;

        public ApiNotificationService(int groupID, PlatformType platform)
        {
            m_Client = new NotificationServiceClient(string.Empty, ConfigManager.GetInstance()
                             .GetConfig(groupID, platform)
                             .PlatformServicesConfiguration.Data.NotificationService.URL);

            m_wsUserName = ConfigManager.GetInstance()
                           .GetConfig(groupID, platform)
                           .PlatformServicesConfiguration.Data.NotificationService.DefaultUser;
            m_wsPassword =
                ConfigManager.GetInstance()
                             .GetConfig(groupID, platform)
                             .PlatformServicesConfiguration.Data.NotificationService.DefaultPassword;

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
                    var notificationMessages = m_Client.GetDeviceNotifications(m_wsUserName, m_wsPassword, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
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
                                    mediaID = message.TagNotificationParams.mediaIDk__BackingField,
                                    mediaPicURL = message.TagNotificationParams.mediaPicURLk__BackingField,
                                    TagDict = message.TagNotificationParams.TagDictk__BackingField != null ? message.TagNotificationParams.TagDictk__BackingField.Select(x => new TagMetaIntPairArray() { Key = x.Key, Values = x.Value }).ToList() : null,
                                    templateEmail = message.TagNotificationParams.templateEmailk__BackingField
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
                    res = m_Client.SetNotificationMessageViewStatus(m_wsUserName, m_wsPassword, sGuid, notificationRequestID, notificationMessageID, viewStatus);
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
                    res = m_Client.AddNotificationRequest(m_wsUserName, m_wsPassword, sGuid, triggerType, mediaId);
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
                Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Values);
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Client.SubscribeByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
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
                Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Values);
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Client.UnsubscribeFollowUpByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
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
            Dictionary<string, string[]> clientRes = new Dictionary<string, string[]>();
            List<TVPApi.TagMetaPairArray> finalRes = new List<TagMetaPairArray>();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    clientRes = m_Client.GetUserStatusSubscriptions(m_wsUserName, m_wsPassword, sGuid);
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
