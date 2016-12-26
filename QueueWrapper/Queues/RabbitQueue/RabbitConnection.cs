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
    public class RabbitConnection : IDisposable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private int FAIL_COUNT_LIMIT = 3;

        private IConnection m_Connection;
        private IModel m_Model;
        private ReaderWriterLockSlim m_lock;
        private int m_FailCounter;
        private int m_FailCounterLimit;

        private RabbitConnection()
        {
        }

        public static RabbitConnection Instance
        {
            get { return Nested.Instance; }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
                if (Instance != null)
                {
                    Instance.m_lock = new ReaderWriterLockSlim();
                    Instance.m_FailCounter = 0;

                    int failCounterLimit = Instance.FAIL_COUNT_LIMIT;  // A random value was chosen here. It is only for a case on which we can't read value neither from TCM nor from AppSettings
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
                            failCounterLimit = Instance.FAIL_COUNT_LIMIT;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                    }
                    finally
                    {
                        Instance.m_FailCounterLimit = failCounterLimit;
                    }
                }
            }

            internal static readonly RabbitConnection Instance = new RabbitConnection();
        }

        public int GetQueueFailCounter()
        {
            return m_FailCounter;
        }

        public int GetQueueFailCountLimit()
        {
            return m_FailCounterLimit;
        }

        public bool Ack(RabbitConfigurationData configuration, string sAckId)
        {
            bool bResult = false;

            this.GetInstance(configuration, QueueAction.Ack);

            if (m_Connection != null)
            {
                try
                {
                    if (this.m_Model != null)
                    {
                        ulong a = ulong.Parse(sAckId);

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = configuration.Exchange })
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

        /// <summary>
        /// Publishes a message to the Rabbit queue
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Publish(RabbitConfigurationData configuration, string message)
        {
            bool isPublishSucceeded = false;

            // Check if we reached the writing limit
            if (m_FailCounter < m_FailCounterLimit)
            {
                bool instanecExists = false;
                instanecExists = this.GetInstance(configuration, QueueAction.Publish);

                if (m_Connection != null && instanecExists)
                {
                    try
                    {
                        try
                        {
                            if (this.m_Model != null)
                            {
                                this.m_Model.Dispose();
                                this.m_Model = null;
                            }

                            this.m_Model = m_Connection.CreateModel();
                        }
                        // If failed, retry until we reach limit - with a new connection
                        catch (OperationInterruptedException ex)
                        {
                            log.ErrorFormat("Failed publishing message to rabbit. Message = {0}", message, ex);
                            ClearConnection();
                            IncreaseFailCounter();
                            return Publish(configuration, message);
                        }

                        if (this.m_Model != null)
                        {
                            var body = Encoding.UTF8.GetBytes(message.ToString());
                            IBasicProperties properties = m_Model.CreateBasicProperties();
                            if (configuration.setContentType)
                                properties.ContentType = "application/json";

                            using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                            {
                                Database = configuration.Exchange
                            })
                            {
                                m_Model.BasicPublish(configuration.Exchange, configuration.RoutingKey, properties, body);
                            }

                            isPublishSucceeded = true;
                            ResetFailCounter();

                            m_Model.Dispose();
                            m_Model = null;
                        }
                    }
                    catch (OperationInterruptedException ex)
                    {
                        log.ErrorFormat("Failed publishing message to rabbit. Message = {0}", message, ex);
                        string msg = ex.Message;
                        ClearConnection();
                        return Publish(configuration, message);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed publishing message to rabbit. Message = {0}", message, ex);
                        IncreaseFailCounter();
                        string msg = ex.Message;
                    }
                }
                else
                {
                    string host = string.Empty;
                    
                    if (configuration != null)
                    {
                        host = configuration.Host;
                    }

                    log.ErrorFormat("RabbitConnection: No instance/connection to host {0}", host);
                }
            }

            return isPublishSucceeded;
        }

        public void ResetFailCounter()
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

        private void ClearConnection()
        {
            if (this.m_Connection != null)
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew, mutexSecurity))
                {
                    try
                    {
                        mutex.WaitOne(-1);

                        if (this.m_Connection != null)
                        {
                            this.m_Connection.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed closing instance of Rabbit Connection.", ex);
                    }
                    finally
                    {
                        m_Connection = null;
                        m_Model = null;
                    }

                    try
                    {
                        if (this.m_Model != null)
                        {
                            this.m_Model.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed closing instance of Rabbit Connection (model).", ex);
                    }
                    finally
                    {
                        m_Model = null;
                    }

                    mutex.ReleaseMutex();
                }
            }
        }

        public string Subscribe(RabbitConfigurationData configuration, ref string sAckId)
        {
            string sMessage = string.Empty;

            this.GetInstance(configuration, QueueAction.Subscribe);

            if (m_Connection != null)
            {
                try
                {
                    if (this.m_Model != null)
                    {

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = configuration.Exchange })
                        {
                            BasicGetResult bgr = m_Model.BasicGet(configuration.QueueName, false);
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

        private bool GetInstance(RabbitConfigurationData configuration, QueueAction action)
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
                            var factory = new ConnectionFactory() { HostName = configuration.Host, Password = configuration.Password, UserName = configuration.Username };

                            this.m_Connection = factory.CreateConnection();
                            if (action.Equals(QueueAction.Subscribe) || action.Equals(QueueAction.Ack))
                            {
                                this.m_Model = this.m_Connection.CreateModel();
                            }
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

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (this.m_Connection != null && this.m_Connection.IsOpen)
            {
                this.m_Connection.Close();
                this.m_Connection = null;
            }
            if (this.m_Model != null && this.m_Model.IsOpen)
            {
                this.m_Model.Close();
                this.m_Model = null;
            }
        }
    }
}
