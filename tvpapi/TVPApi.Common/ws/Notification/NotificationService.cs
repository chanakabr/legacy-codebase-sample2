using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using TVPApiModule.Services;
using TVPApiModule.Objects.Authorization;
using Phx.Lib.Log;
using System.Reflection;
using ApiObjects.Notification;
using Notification = TVPApiModule.Objects.Notification;

namespace TVPApiServices
{
    [System.ComponentModel.ToolboxItem(false)]
    public class NotificationService : INotificationService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
