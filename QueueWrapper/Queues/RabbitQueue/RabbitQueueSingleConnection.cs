using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace QueueWrapper
{
    public class RabbitQueueSingleConnection : RabbitQueue
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected RabbitSingleConnection m_oRabbitConn;
        protected RabbitConfigurationData m_oConfiguration;
        protected string m_sQueueName;
        protected string m_sRoutingKey;

        public RabbitQueueSingleConnection(string sQueueName, string sRoutingKey)
        {
            m_sQueueName = sQueueName;
            m_sRoutingKey = sRoutingKey;
            m_oRabbitConn = null;
            m_oConfiguration = null;
        }

        public bool Start()
        {
            bool bResult = false;

            try
            {
                m_oConfiguration = CreateRabbitConfigurationData();
                if (m_oConfiguration != null)
                {
                    m_oRabbitConn = new RabbitSingleConnection(m_oConfiguration);
                    bResult = m_oRabbitConn.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return bResult;
        }

        protected override RabbitConfigurationData CreateRabbitConfigurationData()
        {
            RabbitConfigurationData config = base.CreateRabbitConfigurationData();

            if (config != null)
            {
                config.QueueName = m_sQueueName;
                config.RoutingKey = m_sRoutingKey;
            }

            return config;
        }

        public override bool Enqueue(string sDataToIndex, string sRouteKey, long expiration = 0)
        {
            bool bIsEnqueueSucceeded = false;
            try
            {
                if (!string.IsNullOrEmpty(sDataToIndex))
                {
                    bIsEnqueueSucceeded = m_oRabbitConn.Publish(sDataToIndex);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }

            return bIsEnqueueSucceeded;
        }

        public override T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;

            T sReturnedData = default(T);
            string sMessage = string.Empty;

            sMessage = m_oRabbitConn.Subscribe(ref sAckId);

            if (!string.IsNullOrEmpty(sMessage))
            {
                sReturnedData = Utils.JsonToObject<T>(sMessage);
            }


            return sReturnedData;
        }

        public override bool Ack(string sQueueName, string sAckId)
        {
            return m_oRabbitConn.Ack(sAckId);
        }

        public override void Dispose()
        {
            if (this.m_oRabbitConn != null)
            {
                this.m_oRabbitConn.Close();
            }
        }
    }
}
