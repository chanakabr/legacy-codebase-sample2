using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{  
    [ServiceContract()]
    public interface IRequestImp
    {
        [ServiceKnownType(typeof(BaseNotification))]
        [ServiceKnownType(typeof(EmailNotification))]
        [ServiceKnownType(typeof(DeviceNotification))]
        [ServiceKnownType(typeof(PullNotification))]
        [OperationContract]
        void Send(NotificationRequest oNotificationRequest);
    }
}
