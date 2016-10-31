using QueueWrapper.Queues;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using KLogMonitor;
using System.Reflection;

namespace QueueWrapper
{
    public class RabbitSingleConnection : IDisposable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private int FAIL_COUNT_LIMIT = 3;

        private IConnection m_Connection;
        private IModel m_Model;
        private ReaderWriterLockSlim m_lock;
        private int m_FailCounter;
        private int m_FailCounterLimit;
        private RabbitConfigurationData m_Configuration;

        public RabbitSingleConnection(RabbitConfigurationData configuration)
        {
            m_Configuration = configuration;
            m_lock = new ReaderWriterLockSlim();
        }

        public bool Start()
        {
            InitFailCounter();
            bool bResult = InitConnection();

            return bResult;
        }

        public int GetQueueFailCounter()
        {
            return m_FailCounter;
        }

        public int GetQueueFailCountLimit()
        {
            return m_FailCounterLimit;
        }

        public bool Ack(string sAckId)
        {
            bool bResult = false;

            if (m_Connection != null)
            {
                try
                {
                    if (this.m_Model != null)
                    {
                        ulong a = ulong.Parse(sAckId);

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = m_Configuration.Exchange })
                        {
                            m_Model.BasicAck(ulong.Parse(sAckId), false);
                        }
                        bResult = true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
            }

            return bResult;
        }

        public bool Publish(string sMessage)
        {
            bool isPublishSucceeded = false;
            if (m_FailCounter < m_FailCounterLimit) // Check if we reached the writing limit
            {

                if (m_Connection != null)
                {
                    try
                    {
                        this.m_Model = m_Connection.CreateModel();
                        if (this.m_Model != null)
                        {
                            var body = Encoding.UTF8.GetBytes(sMessage.ToString());
                            IBasicProperties properties = m_Model.CreateBasicProperties();
                            using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = m_Configuration.Exchange })
                            {
                                m_Model.BasicPublish(m_Configuration.Exchange, m_Configuration.RoutingKey, properties, body);
                            }
                            isPublishSucceeded = true;
                            ResetFailCounter();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        IncreaseFailCounter();
                        string msg = ex.Message;
                    }
                }
            }

            return isPublishSucceeded;
        }

        public string Subscribe(ref string sAckId)
        {
            string sMessage = string.Empty;

            if (m_Connection != null)
            {
                try
                {
                    if (this.m_Model != null)
                    {
                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = m_Configuration.Exchange })
                        {
                            BasicGetResult bgr = m_Model.BasicGet(m_Configuration.QueueName, false);

                            if (bgr != null && bgr.Body != null && bgr.Body.Length > 0)
                            {
                                byte[] body = bgr.Body;
                                sMessage = Encoding.UTF8.GetString(body);
                                sAckId = bgr.DeliveryTag.ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
            }

            return sMessage;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (this.m_Model != null)
            {
                try
                {
                    this.m_Model.Close();
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
            }

            if (this.m_Connection != null)
            {
                try
                {
                    this.m_Connection.Close();
                }
                catch (Exception ex)
                {
                    log.Error("", ex);
                }
            }

            this.m_Model = null;
            this.m_Connection = null;
        }

        private void ResetFailCounter()
        {
            if (m_FailCounter > 0)
            {
                m_lock.EnterWriteLock();
                try
                {
                    if (m_FailCounter > 0)
                    {
                        this.m_FailCounter = 0;
                    }
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }

        private void IncreaseFailCounter()
        {
            if (m_FailCounter < m_FailCounterLimit)
            {
                m_lock.EnterWriteLock();
                try
                {
                    if (m_FailCounter < m_FailCounterLimit)
                    {
                        this.m_FailCounter++;
                    }
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }

        private bool InitConnection()
        {
            bool bIsGetInstanceSucceeded = false;

            if (this.m_Connection == null)
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew, mutexSecurity))
                {
                    try
                    {
                        mutex.WaitOne(-1);
                        if (this.m_Connection == null)
                        {
                            var factory = new ConnectionFactory() { HostName = m_Configuration.Host, Password = m_Configuration.Password, UserName = m_Configuration.Username };
                            this.m_Connection = factory.CreateConnection();
                            this.m_Model = this.m_Connection.CreateModel();
                            bIsGetInstanceSucceeded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                        m_Connection = null;
                        m_Model = null;
                        IncreaseFailCounter();
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            else
            {
                bIsGetInstanceSucceeded = true;
            }

            return bIsGetInstanceSucceeded;
        }

        private void InitFailCounter()
        {
            this.m_FailCounter = 0;

            int failCounterLimit = this.FAIL_COUNT_LIMIT;  // A random value was chosen here. It is only for a case on which we can't read value neither from TCM nor from AppSettings
            try
            {
                string sFailCountLimit = Utils.GetTcmConfigValue("queue_fail_limit");
                bool isParseSucceeded = false;
                if (string.IsNullOrEmpty(sFailCountLimit)) // If reading from TCM failed, try to read from extra.config. Else, convert to int
                {
                    isParseSucceeded = int.TryParse(ConfigurationManager.AppSettings["queue_fail_limit"], out failCounterLimit);
                }
                else
                {
                    isParseSucceeded = int.TryParse(sFailCountLimit, out failCounterLimit);
                }

                // If any reading failed, set the failCounterLimit as the constant
                if (!isParseSucceeded)
                {
                    failCounterLimit = this.FAIL_COUNT_LIMIT;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            finally
            {
                this.m_FailCounterLimit = failCounterLimit;
            }
        }
    }
}
