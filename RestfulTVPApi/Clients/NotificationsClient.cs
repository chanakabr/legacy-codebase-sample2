//using RestfulTVPApi.Clients.ClientsCache;
//using RestfulTVPApi.Objects.Responses;
//using ServiceStack.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

//namespace RestfulTVPApi.Clients
//{
//    public class NotificationsClient : BaseClient
//    {
//        #region Variables

//        private readonly ILog logger = LogManager.GetLogger(typeof(NotificationsClient));
//        private static object instanceLock = new object();
//        //private int m_groupID;
//        //private PlatformType m_platform;
//        //private string m_wsUserName = string.Empty;
//        //private string m_wsPassword = string.Empty;
//        //private NotificationServiceClient m_Client;

//        #endregion

//        #region CTOR

//        public NotificationsClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
//        {
//            //m_Client = new NotificationServiceClient(string.Empty, ConfigManager.GetInstance()
//            //                 .GetConfig(groupID, platform)
//            //                 .PlatformServicesConfiguration.Data.NotificationService.URL);

//            //m_wsUserName = ConfigManager.GetInstance()
//            //               .GetConfig(groupID, platform)
//            //               .PlatformServicesConfiguration.Data.NotificationService.DefaultUser;
//            //m_wsPassword =
//            //    ConfigManager.GetInstance()
//            //                 .GetConfig(groupID, platform)
//            //                 .PlatformServicesConfiguration.Data.NotificationService.DefaultPassword;

//            //m_groupID = groupID;
//            //m_platform = platform;
//        }

//        public NotificationsClient()
//        {

//        }

//        #endregion

//        #region Properties

//        protected RestfulTVPApi.Notification.NotificationServiceClient Notification
//        {
//            get 
//            {
//                return (m_Module as NotificationServiceClient);
//            }
//        }

//        #endregion

//        //#region Public Static Functions

//        //public static ApiNotificationService Instance(int groupId, PlatformType platform)
//        //{
//        //    return BaseService.Instance(groupId, platform, eService.NotificationService) as ApiNotificationService;
//        //}

//        //#endregion

//        public List<Notification> GetDeviceNotifications(string sGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
//        {
//            List<Notification> res = new List<Notification>();

//            res = Execute(() =>
//                {
//                    var notificationMessages = Notification.GetDeviceNotifications(m_wsUserName, m_wsPassword, sGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
//                    if (notificationMessages != null)
//                    {
//                        foreach (var message in notificationMessages)
//                        {
//                            res.Add(new Notification()
//                            {
//                                actions = message.Actions != null ? message.Actions : null,
//                                app_name = message.AppName,
//                                DeviceID = message.DeviceID,
//                                id = message.ID,
//                                message_text = message.MessageText,
//                                nGroupID = message.nGroupID,
//                                notification_id = message.NotificationID,
//                                notification_message_id = message.NotificationMessageID,
//                                status = message.Status,
//                                title = message.Title,
//                                type = message.Type,
//                                udid = message.UdID,
//                                user_id = message.UserID,
//                                tag_notification_params = message.TagNotificationParams != null ? new ExtraParameters()
//                                {
//                                    mediaID = message.TagNotificationParams.mediaIDk__BackingField,
//                                    mediaPicURL = message.TagNotificationParams.mediaPicURLk__BackingField,
//                                    TagDict = message.TagNotificationParams.TagDictk__BackingField != null ? message.TagNotificationParams.TagDictk__BackingField.Select(x => new TagMetaIntPairArray() { Key = x.Key, Values = x.Value }).ToList() : null,
//                                    templateEmail = message.TagNotificationParams.templateEmailk__BackingField
//                                } : null,
//                                publish_date = message.PublishDate,
//                                view_status = message.ViewStatus,
//                                notification_request_id = message.NotificationRequestID
//                            });
//                        }
//                    }

//                    return res;
//                }) as List<Notification>;

//            return res;
//        }

//        public bool SetNotificationMessageViewStatus(string sGuid, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
//        {
//            bool res = false;

//            res = Convert.ToBoolean(Execute(() =>
//                {
//                    res = Notification.SetNotificationMessageViewStatus(m_wsUserName, m_wsPassword, sGuid, notificationRequestID, notificationMessageID, viewStatus);
//                    return res;
//                }));

//            return res;
//        }

//        public bool AddNotificationRequest(string sGuid, NotificationTriggerType triggerType, int mediaId)
//        {
//            bool res = false;

//            res = Convert.ToBoolean(Execute(() =>
//                {
//                    res = Notification.AddNotificationRequest(m_wsUserName, m_wsPassword, sGuid, triggerType, mediaId);
//                    return res;
//                }));

//            return res;
//        }

//        public bool SubscribeByTag(string sGuid, List<TagMetaPairArray> tags)
//        {
//            bool res = false;

//            res = Convert.ToBoolean(Execute(() =>
//                {
//                    Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values);
//                    res = Notification.SubscribeByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);

//                    return res;
//                }));

//            return res;
//        }

//        public bool UnsubscribeFollowUpByTag(string sGuid, List<TagMetaPairArray> tags)
//        {
//            bool res = false;

//            res = Convert.ToBoolean(Execute(() =>
//                {
//                    Dictionary<string, string[]> dictTags = tags.ToDictionary((keyItem) => keyItem.key, (valueItem) => valueItem.values);
//                    res = Notification.UnsubscribeFollowUpByTag(m_wsUserName, m_wsPassword, sGuid, dictTags);

//                    return res;
//                }));

//            return res;
//        }

//        public List<TagMetaPairArray> GetUserStatusSubscriptions(string sGuid)
//        {
//            //Dictionary<string, string[]> clientRes = new Dictionary<string, string[]>();
//            List<TagMetaPairArray> finalRes = new List<TagMetaPairArray>();

//            finalRes = Execute(() =>
//                {
//                    Dictionary<string, string[]> clientRes = new Dictionary<string, string[]>();
//                    clientRes = Notification.GetUserStatusSubscriptions(m_wsUserName, m_wsPassword, sGuid);

//                    // convert to list
//                    if (clientRes != null)
//                    {
//                        foreach (var entry in clientRes)
//                            finalRes.Add(new TagMetaPairArray() { key = entry.Key, values = entry.Value });

//                        return finalRes;
//                    }

//                    return finalRes;
//                }) as List<TagMetaPairArray>;

//            return finalRes;            
//        }
//    }
//}