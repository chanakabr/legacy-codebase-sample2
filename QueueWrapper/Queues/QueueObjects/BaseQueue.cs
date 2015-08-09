using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public abstract class BaseQueue : IQueueable
    {
        #region Private Members

        private IQueueImpl m_QueueImpl;

        #endregion

        #region CTOR

        public BaseQueue()
        { }

        #endregion

        #region IQueuable

        //public abstract bool Enqueue(ApiObjects.MediaIndexingObjects.QueueObject record, int nGroupId);
        public virtual bool Enqueue(ApiObjects.QueueObject record, string routingKey)
        {
            bool bIsEnqueueSucceeded = false;
            string sMessage = string.Empty;

            if (record != null)
            {
                sMessage = record.ToString();
                if (this.Implementation != null)
                {
                    bIsEnqueueSucceeded = this.Implementation.Enqueue(sMessage, routingKey);
                }
            }

            return bIsEnqueueSucceeded;
        }

        //public abstract T Dequeue<T>(string sQueueName, out string sAckId);
        public virtual T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;

            T objectReturned = default(T);
            if (!string.IsNullOrEmpty(sQueueName))
            {
                objectReturned = this.Implementation.Dequeue<T>(sQueueName, out sAckId);
            }

            return objectReturned;
        }

        #endregion

        #region Getters

        internal IQueueImpl Implementation
        {
            get { return this.m_QueueImpl; }
            set { this.m_QueueImpl = value; }
        }

        #endregion

    }
}
