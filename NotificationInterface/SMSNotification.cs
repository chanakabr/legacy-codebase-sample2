using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Logger;
using NotificationObj;

namespace NotificationInterface
{
    public class SMSNotification : NotificationBase, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
