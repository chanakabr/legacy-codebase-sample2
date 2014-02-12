using System;
using System.Collections.Generic;
using log4net;
using System.Linq;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using TVPApiModule.Context;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Services
{
    public class ApiNotificationService : BaseService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ApiNotificationService));
        private static object instanceLock = new object();
        //private int m_groupID;
        //private PlatformType m_platform;
        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;
        //private NotificationServiceClient m_Client;

        public ApiNotificationService(int groupID, PlatformType platform)
        {
            //m_Client = new NotificationServiceClient(string.Empty, ConfigManager.GetInstance()
            //                 .GetConfig(groupID, platform)
            //                 .PlatformServicesConfiguration.Data.NotificationService.URL);

            //m_wsUserName = ConfigManager.GetInstance()
            //               .GetConfig(groupID, platform)
            //               .PlatformServicesConfiguration.Data.NotificationService.DefaultUser;
            //m_wsPassword =
            //    ConfigManager.GetInstance()
            //                 .GetConfig(groupID, platform)
            //                 .PlatformServicesConfiguration.Data.NotificationService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiNotificationService()
        {

        }

        //#region Public Static Functions

        //public static ApiNotificationService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.NotificationService) as ApiNotificationService;
        //}

        //#endregion

        public List<Notification> GetDeviceNotifications(string sGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            List<Notification> res = new List<Notification>();

            try
            {
                var notificationMessages = (m_Module as NotificationServiceClient).GetDeviceNotifications(m_wsUserName, m_wsPassword, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
                if (notificationMessages != null)
                {
                    foreach (var message in notificationMessages)
                    {
                        res.Add(new Notification()
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
                                mediaID = message.TagNotificationParams.mediaIDk__BackingField,
                                mediaPicURL = message.TagNotificationParams.mediaPicURLk__BackingField,
                                TagDict = message.TagNotificationParams.TagDictk__BackingField != null ? message.TagNotificationParams.TagDictk__BackingField.Select(x => new TagMetaIntPairArray() { Key = x.Key, Values = x.Value }).ToList() : null,
                                templateEmail = message.TagNotificationParams.templateEmailk__BackingField
                            } : null,
                            publish_date = message.PublishDate,
                            view_status = message.ViewStatus,
                            notification_request_id = message.NotificationRequestID
                        });
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
                res = (m_Module as NotificationServiceClient).SetNotificationMessageViewStatus(m_wsUserName, m_wsPassword, sGuid, notificationRequestID, notificationMessageID, viewStatus);
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
                res = (m_Module as NotificationServiceClient).AddNotificationRequest(m_wsUserName, m_wsPassword, sGuid, triggerType, mediaId);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in AddNotificationRequest, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public bool SubscribeByTag(string sGuid, List<TagMetaPairArray> tags)
        {
            bool res = false;
            try
            {
                Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values);
                res = (m_Module as NotificationServiceClient).SubscribeByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in SubscribeByTag, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public bool UnsubscribeFollowUpByTag(string sGuid, List<TagMetaPairArray> tags)
        {
            bool res = false;
            try
            {
                Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values);
                res = (m_Module as NotificationServiceClient).UnsubscribeFollowUpByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occurred in UnsubscribeFollowUpByTag, Error : {0} Parameters : siteGuid {1}", e.Message, sGuid);
            }

            return res;
        }

        public List<TagMetaPairArray> GetUserStatusSubscriptions(string sGuid)
        {
            Dictionary<string, string[]> clientRes = new Dictionary<string, string[]>();
            List<TagMetaPairArray> finalRes = new List<TagMetaPairArray>();
            try
            {
                clientRes = (m_Module as NotificationServiceClient).GetUserStatusSubscriptions(m_wsUserName, m_wsPassword, sGuid);

                // convert to list
                if (clientRes != null)
                {
                    foreach (var entry in clientRes)
                        finalRes.Add(new TagMetaPairArray() { key = entry.Key, values = entry.Value });

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
