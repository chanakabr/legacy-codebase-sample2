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
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Notification;


namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class NotificationService : System.Web.Services.WebService, INotificationService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(NotificationService));

        [WebMethod(EnableSession = true, Description = "Adds notification request")]
        public bool AddNotificationRequest(InitializationObject initObj, NotificationTriggerType triggerType)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AddNotificationRequest", initObj.ApiUser, initObj.ApiPass,
                                                      SiteHelper.GetClientIP());
            logger.InfoFormat("AddNotificationRequest-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                        return service.AddNotificationRequest(siteGuid, triggerType);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("AddNotificationRequest->", ex);
                }
            }
            else
                logger.ErrorFormat("AddNotificationRequest-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);

            return false;
        }

        [WebMethod(EnableSession = true, Description = "Gets device notifications")]
        public NotificationMessage[] GetDeviceNotifications(InitializationObject initObj, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceNotifications", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetDeviceNotifications-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform, initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);
                        return service.GetDeviceNotifications(siteGuid, initObj.UDID, notificationType, viewStatus, messageCount);
                    }
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
        public bool SetNotificationMessageViewStatus(InitializationObject initObj, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetNotificationMessageViewStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("SetNotificationMessageViewStatus-> [{0}, {1}], Params:[user: {2}]", groupId, initObj.Platform,
                              initObj.SiteGuid);

            if (groupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (Int32.TryParse(initObj.SiteGuid, out siteGuid))
                    {
                        ApiNotificationService service =
                            new ApiNotificationService(groupId, initObj.Platform);
                        return service.SetNotificationMessageViewStatus(siteGuid, notificationRequestID, notificationMessageID, viewStatus);
                    }
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
    }
}
