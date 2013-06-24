using System;
using System.Collections.Generic;
using log4net;
using System.Linq;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace TVPApiModule.Services
{
    public class ApiNotificationService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ApiNotificationService));
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

        public NotificationMessage[] GetDeviceNotifications(string sGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            NotificationMessage[] res = null;
            try
            {
                res = m_Client.GetDeviceNotifications(m_wsUserName, m_wsPassword, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
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
                res = m_Client.SetNotificationMessageViewStatus(m_wsUserName, m_wsPassword, sGuid, notificationRequestID, notificationMessageID, viewStatus);
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
                res = m_Client.AddNotificationRequest(m_wsUserName, m_wsPassword, sGuid, triggerType, mediaId);
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
                res = m_Client.SubscribeByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
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
                res = m_Client.UnsubscribeFollowUpByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
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
                clientRes = m_Client.GetUserStatusSubscriptions(m_wsUserName, m_wsPassword, sGuid);

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
