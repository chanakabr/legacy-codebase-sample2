using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects.Notification;

namespace Core.Notification
{
    public class DeviceNotification : NotificationBase, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DeviceNotification()
            : base()
        {
        }

        public void Send(NotificationRequest request)
        {
            // get implementer - simple or broadcast
            BaseImplementor implementor = ImplementorsFactory.GetImplementor(request);
            //get Notification Message Object for Device (push) 
            List<NotificationMessage> messagesList = implementor.GetMessages(request); // one pull message for each device            
            log.Info(string.Format("{0}Implementor Type={1},Notification messages count={2}", "DeviceNotification.Send", implementor.GetType(), messagesList.Count));
            implementor.Send(messagesList, request.TriggerType != NotificationTriggerType.BadgeUpdate);
        }
    }
}
