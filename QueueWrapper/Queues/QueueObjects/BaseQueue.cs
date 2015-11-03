using System;
using System.Collections.Generic;
using System.Data;
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

                if (bIsEnqueueSucceeded)
                {
                    var celeryData = record as ApiObjects.BaseCeleryData;

                    if (celeryData != null)
                    {
                        string[] keys = routingKey.Split('\\');
                        if (!string.IsNullOrEmpty(keys[0]))
                            InsertQueueMessage(celeryData.GroupId, sMessage, keys[0], celeryData.ETA.GetValueOrDefault());
                    }
                }
            }

            return bIsEnqueueSucceeded;
        }
       
        public virtual bool Enqueue(int groupId, string record, string routingKey, DateTime runDate)
        {
            bool bIsEnqueueSucceeded = false;

            if (!string.IsNullOrEmpty(record))
            {
                if (this.Implementation != null)
                {
                    bIsEnqueueSucceeded = this.Implementation.Enqueue(record, routingKey);
                }

                if (bIsEnqueueSucceeded)
                {
                    string[] keys = routingKey.Split('\\');
                    if (!string.IsNullOrEmpty(keys[0]))
                        InsertQueueMessage(groupId, record, keys[0], runDate);
                }
            }

            return bIsEnqueueSucceeded;
        }

        private void InsertQueueMessage(int groupId, string messageData, string routingKey, DateTime excutionDate)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_QueueMessage");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@messageData", messageData);
                sp.AddParameter("@routingKey", routingKey);                
                sp.AddParameter("@excutionDate", excutionDate);

                DataSet ds = sp.ExecuteDataSet();
            }

            catch (Exception ex)
            {
            }

        }

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
