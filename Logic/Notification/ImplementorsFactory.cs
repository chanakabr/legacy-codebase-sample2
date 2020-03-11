using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    /// <summary>
    /// Factory class that retrurn concrete Implementor(each implementor inherits from BaseImplementor) instance
    /// according to the NotificationRequestType of the NotificationRequest.
    /// </summary>
    public static class ImplementorsFactory
    {
        public static BaseImplementor GetImplementor(NotificationRequest request)
        {
            BaseImplementor implementor = null;

            switch (request.Type)
            {
                case NotificationRequestType.Simple:
                {
                    implementor = new SimpleImlementor(request);
                    break;
                }
                
                case NotificationRequestType.BroadCast:
                {
                    implementor = new BroadcastImplementor(request);
                    break;
                }

                default:
                {
                    implementor = new SimpleImlementor(request);
                    break;
                }
            }
            return implementor;
        }
    }
}
