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

namespace QueueWrapper
{
    public class RabbitConnection : IDisposable
    {

        #region CONST

        private int FAIL_COUNT_LIMIT = 3;

        #endregion

        #region Members

        private IConnection m_Connection;
        private IModel m_Model;
        private ReaderWriterLockSlim m_lock; 
        private int m_FailCounter;
        private int m_FailCounterLimit;

        #endregion

        #region CTOR

        private RabbitConnection()
        {
        }

        #endregion

        #region Singleton

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
                        if (string.IsNullOrEmpty(sFailCountLimit))
                        {
                            bool isParseSucceeded = int.TryParse(ConfigurationManager.AppSettings["queue_fail_limit"], out failCounterLimit);
                            if (!isParseSucceeded)
                            {
                                failCounterLimit = Instance.FAIL_COUNT_LIMIT;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                    finally
                    {
                        Instance.m_FailCounterLimit = failCounterLimit;
                    }
                }
            }

            internal static readonly RabbitConnection Instance = new RabbitConnection();            
        }

        #endregion

        #region Public Functions

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
                        m_Model.BasicAck(ulong.Parse(sAckId), false);
                        bResult = true;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return bResult;
        }

        public bool Publish(RabbitConfigurationData configuration, string sMessage)
        {
            bool isPublishSucceeded = false;
            if (m_FailCounter < m_FailCounterLimit) // Check if we reached the writing limit
            {
                bool bIsInstanceExist = false;
                bIsInstanceExist = this.GetInstance(configuration, QueueAction.Publish);

                if (m_Connection != null && bIsInstanceExist)
                {
                    try
                    {
                        this.m_Model = m_Connection.CreateModel();
                        if (this.m_Model != null)
                        {
                            var body = Encoding.UTF8.GetBytes(sMessage.ToString());
                            IBasicProperties properties = m_Model.CreateBasicProperties();
                            m_Model.BasicPublish(configuration.Exchange, configuration.RoutingKey, properties, body);
                            isPublishSucceeded = true;
                            ResetFailCounter();
                        }
                    }
                    catch (Exception ex)
                    {
                        IncreaseFailCounter();
                        string msg = ex.Message;
                    }
                }
            }

            return isPublishSucceeded;
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
                        BasicGetResult bgr = m_Model.BasicGet(configuration.QueueName, false);

                        if (bgr != null && bgr.Body != null && bgr.Body.Length > 0)
                        {
                            byte[] body = bgr.Body;
                            sMessage = Encoding.UTF8.GetString(body);
                            sAckId = bgr.DeliveryTag.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {

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
                            var factory = new ConnectionFactory() { HostName = configuration.Host, Password = configuration.Password };

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

        #endregion
    }
}
