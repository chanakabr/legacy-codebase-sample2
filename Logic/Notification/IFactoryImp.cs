using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Notification;

namespace Core.Notification
{
    public interface IFactoryImp
    {
        IRequestImp GetTypeImp(eSenderObjectType senderObject);
    }
}
