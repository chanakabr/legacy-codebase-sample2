using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NotificationObj;

namespace NotificationInterface
{
    public interface IFactoryImp
    {
        IRequestImp GetTypeImp(eSenderObjectType senderObject);
    }
}
