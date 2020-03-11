using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    [DataContract]
    public class FactoryImp : IFactoryImp
    {
        eEmail email = eEmail.EmailNotification;
        eSMS  sms = eSMS.SMSNotification;
      
        public FactoryImp(long groupID)
        {
            GetFactory(groupID);
        }

        public IRequestImp GetTypeImp(eSenderObjectType senderType)
        {  
            //internal logic on which Type to return
            switch (senderType)
            {
                case eSenderObjectType.Device:
                    return new DeviceNotification();
                case eSenderObjectType.SMS:
                    switch (sms)
                    {
                        case eSMS.ElisaSMSNotification:
                            return new ElisaSMSNotification();
                        default:
                            return new SMSNotification();
                    }                    
                case eSenderObjectType.Email:                    
                    switch (email)
                    {
                        case eEmail.ElisaEmailNotification:
                            return new ElisaEmailNotification();
                        default:
                            return new EmailNotification();
                    }
                case eSenderObjectType.Pull:
                    return new PullNotification();                    
                default:
                    return null;
            }
        }

        private void GetFactory(long groupId)
        {           
            DataTable dt = DAL.NotificationDal.GetFactory(groupId);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                int nSms = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["SMSFactory"]);
                sms = (eSMS)nSms;
                int nEmail = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["EmailFactory"]);
                email = (eEmail)nEmail;
            }
        }
    }
}
