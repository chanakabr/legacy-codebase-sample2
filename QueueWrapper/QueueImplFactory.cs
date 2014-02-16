using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public static class QueueImplFactory
    {
        public static IQueueImpl GetQueueImp(QueueType type)
        {
            IQueueImpl queue = null;
            switch (type)
            {
                case QueueType.RabbitQueue:
                    queue = new RabbitQueue();
                    break;
                default:
                    break;
            }

            return queue;
        }        
    }
}
