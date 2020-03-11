using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace TVPApiServices
{
    [ServiceContract]
    public interface INotificationService
    {
        [OperationContract]
        List<Notification> GetDeviceNotifications(InitializationObject initObj, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, Nullable<int> messageCount);

        [OperationContract]
        bool SetNotificationMessageViewStatus(InitializationObject initObj, Nullable<long> notificationRequestID, Nullable<long> notificationMessageID, NotificationMessageViewStatus viewStatus);

        [OperationContract]
        bool SubscribeByTag(InitializationObject initObj, List<TVPApi.TagMetaPairArray> tags);

        [OperationContract]
        bool UnsubscribeFollowUpByTag(InitializationObject initObj, List<TVPApi.TagMetaPairArray> tags);

        [OperationContract]
        List<TVPApi.TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj);
    }
}
