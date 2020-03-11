using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPApiModule.Objects;
using ApiObjects.Notification;
using Notification = TVPApiModule.Objects.Notification;

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
