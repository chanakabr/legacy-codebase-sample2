using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;


namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class NotificationService : System.Web.Services.WebService, INotificationService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod(EnableSession = true, Description = "Gets device notifications")]
        [PrivateMethod]
        public List<Notification> GetDeviceNotifications(InitializationObject initObj, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceNotifications", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetDeviceNotifications-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                    return service.GetDeviceNotifications(initObj.SiteGuid, initObj.UDID, notificationType == NotificationMessageType.All ? NotificationMessageType.Pull : notificationType, viewStatus, messageCount);
                }
                catch (Exception ex)
                {
                    logger.Error("GetDeviceNotifications->", ex);
                }
            }
            else
                logger.ErrorFormat("GetDeviceNotifications-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser,
                                   initObj.ApiPass);

            return null;
        }

        [WebMethod(EnableSession = true, Description = "Sets notification message view status")]
        [PrivateMethod]
        public bool SetNotificationMessageViewStatus(InitializationObject initObj, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetNotificationMessageViewStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("SetNotificationMessageViewStatus-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                    return service.SetNotificationMessageViewStatus(initObj.SiteGuid, notificationRequestID, notificationMessageID, viewStatus);
                }
                catch (Exception ex)
                {
                    logger.Error("SetNotificationMessageViewStatus->", ex);
                }
            }
            else
                logger.ErrorFormat("SetNotificationMessageViewStatus-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return false;
        }


        [WebMethod(EnableSession = true, Description = "Followup by tag")]
        [PrivateMethod]
        public bool SubscribeByTag(InitializationObject initObj, List<TVPApi.TagMetaPairArray> tags)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SubscribeByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("SubscribeByTag-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                    return service.SubscribeByTag(initObj.SiteGuid, tags);
                }
                catch (Exception ex)
                {
                    logger.Error("SubscribeByTag->", ex);
                }
            }
            else
                logger.ErrorFormat("SubscribeByTag-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return false;
        }

        [WebMethod(EnableSession = true, Description = "Unsubscribe Followup by tag")]
        [PrivateMethod]
        public bool UnsubscribeFollowUpByTag(InitializationObject initObj, List<TVPApi.TagMetaPairArray> tags)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UnsubscribeFollowUpByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("UnsubscribeFollowUpByTag-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                    return service.UnsubscribeFollowUpByTag(initObj.SiteGuid, tags);
                }
                catch (Exception ex)
                {
                    logger.Error("UnsubscribeFollowUpByTag->", ex);
                }
            }
            else
                logger.ErrorFormat("UnsubscribeFollowUpByTag-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return false;
        }

        [WebMethod(EnableSession = true, Description = "Gets the user status subscription")]
        [PrivateMethod]
        public List<TVPApi.TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserStatusSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetUserStatusSubscriptions-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                    return service.GetUserStatusSubscriptions(initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    logger.Error("GetUserStatusSubscriptions->", ex);
                }
            }
            else
                logger.ErrorFormat("GetUserStatusSubscriptions-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return null;
        }
    }
}
