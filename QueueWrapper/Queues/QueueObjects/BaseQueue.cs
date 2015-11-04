using KLogMonitor;
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

        private static readonly KLogger log = new KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                        InsertQueueMessage(celeryData.GroupId, sMessage, routingKey, celeryData.ETA, this.GetType().ToString());
                    }
                }
            }

            return bIsEnqueueSucceeded;
        }
       
        public virtual bool Enqueue(int groupId, string record, string routingKey, DateTime? runDate, string type)
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
                    InsertQueueMessage(groupId, record, routingKey, runDate, type);
                }
            }

            return bIsEnqueueSucceeded;
        }

        private void InsertQueueMessage(int groupId, string messageData, string routingKey, DateTime? excutionDate, string type)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Insert_QueueMessage_NEW");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@messageData", messageData);
                sp.AddParameter("@routingKey", routingKey);
                if (excutionDate.HasValue)
                {
                    sp.AddParameter("@excutionDate", excutionDate.Value);
                }
                sp.AddParameter("@type", type);

                DataSet ds = sp.ExecuteDataSet();
            }

            catch (Exception ex)
            {
                log.ErrorFormat("InsertQueueMessage routingKey {0}, - excutionDate {1}, error: {2}", routingKey, 
                    excutionDate.ToString(), 
                    ex.Message);
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
