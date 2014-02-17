using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Logger;
using NotificationObj;

namespace NotificationInterface
{
    public class DeviceNotification : NotificationBase, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            _logger.Info(string.Format("{0}Implementor Type={1},Notification messages count={2}", "DeviceNotification.Send", implementor.GetType(), messagesList.Count));
            implementor.Send(messagesList, request.TriggerType != NotificationTriggerType.BadgeUpdate);
        }
    }
}
