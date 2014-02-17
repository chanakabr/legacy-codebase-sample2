using System;
using System.Collections.Generic;
using System.ServiceModel;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPApiModule.Objects.Responses;

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
        bool SubscribeByTag(InitializationObject initObj, List<TagMetaPairArray> tags);

        [OperationContract]
        bool UnsubscribeFollowUpByTag(InitializationObject initObj, List<TagMetaPairArray> tags);

        [OperationContract]
        List<TagMetaPairArray> GetUserStatusSubscriptions(InitializationObject initObj);
    }
}
