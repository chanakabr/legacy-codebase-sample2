using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    public class SMSNotification : NotificationBase, IRequestImp
    {
        public string m_username = string.Empty;
        public string m_password = string.Empty;
        public string m_smsURL = string.Empty;
        public string m_userPhoneNumber = string.Empty;


        public SMSNotification()
            : base()
        {
        }

        public virtual void Send(NotificationRequest request)
        {
           
        }

      
    }
}
