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

        #region Data Members

        private int failCountLimit = 3;
        private IConnection m_Connection;
        private IModel m_Model;
        private ReaderWriterLockSlim m_lock;
        private int m_FailCounter;
        private int m_FailCounterLimit;

        #endregion

        #region Get Methods

        public int GetQueueFailCounter()
        {
            return m_FailCounter;
        }

        public int GetQueueFailCountLimit()
        {
            return m_FailCounterLimit;
        }

        #endregion

        #region Ctor and initialization

        private RabbitConnection()
        {
        }

        public static RabbitConnection Instance
        {
            get
            {
                return Nested.Instance;
            }
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

                    // A random value was chosen here. It is only for a case on which we can't read value neither from TCM nor from AppSettings
                    int failCounterLimit = Instance.failCountLimit;

                    try
                    {
                        string sFailCountLimit = Utils.GetTcmConfigValue("queue_fail_limit");
                        bool isParseSucceeded = false;

                        // If reading from TCM failed, try to read from extra.config. Else, convert to int
                        if (string.IsNullOrEmpty(sFailCountLimit))
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
                            failCounterLimit = Instance.failCountLimit;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in nested rabbit", ex);
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

        #region Main Methods

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

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                        {
                            Database = configuration.Exchange
                        })
                        {
                            m_Model.BasicAck(ulong.Parse(sAckId), false);
                        }
                        bResult = true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in Ack rabbit", ex);
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
        public bool Publish(RabbitConfigurationData configuration, string message, long expirationMiliSec = 0)
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
                            this.m_Model = m_Connection.CreateModel();
                        }
                        // If failed, retry until we reach limit - with a new connection
                        catch (OperationInterruptedException ex)
                        {
                            log.ErrorFormat("Failed publishing message to rabbit. Message = {0}, EX = {1}", message, ex);
                            ClearConnection();
                            IncreaseFailCounter();
                            return Publish(configuration, message);
                        }

                        if (this.m_Model != null)
                        {
                            var body = Encoding.UTF8.GetBytes(message.ToString());
                            IBasicProperties properties = m_Model.CreateBasicProperties();

                            // should be "application/json"
                            if (!string.IsNullOrEmpty(configuration.ContentType))
                                properties.ContentType = configuration.ContentType;
                            
                            // set message expiration
                            if (expirationMiliSec != 0)
                                properties.Expiration = expirationMiliSec.ToString();

                            using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null) { Database = configuration.Exchange })
                            {
                                m_Model.BasicPublish(configuration.Exchange, configuration.RoutingKey, properties, body);
                            }

                            log.DebugFormat("Rabbit message sent. configuration.Exchange: {0}, configuration.RoutingKey: {1}, body: {2}",
                                configuration != null && configuration.Exchange != null ? configuration.Exchange : string.Empty,
                                configuration != null && configuration.RoutingKey != null ? configuration.RoutingKey : string.Empty,
                                message);

                            isPublishSucceeded = true;
                            ResetFailCounter();
                        }
                    }
                    catch (OperationInterruptedException ex)
                    {
                        log.ErrorFormat("Failed publishing message to rabbit. Message = {0}, EX = {1}", message, ex);
                        string msg = ex.Message;
                        ClearConnection();
                        return Publish(configuration, message);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed publishing message to rabbit. Message = {0}, EX = {1}", message, ex);
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
                            this.m_Connection = null;
                            this.m_Model = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed closing instance of Rabbit Connection.", ex);
                        m_Connection = null;
                        m_Model = null;
                    }
                    finally
                    {
                        m_Connection = null;
                        m_Model = null;
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        /// <summary>
        /// Reads a message from the queue
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="sAckId"></param>
        /// <returns></returns>
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

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                        {
                            Database = configuration.Exchange
                        })
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
                    log.Error("Error in Subscribe", ex);
                }
            }

            return sMessage;
        }

        public bool IsQueueExist(RabbitConfigurationData configData)
        {
            try
            {
                if (this.GetInstance(configData, QueueAction.Ack) && this.m_Connection != null && this.m_Model != null)
                {
                    var res = this.m_Model.QueueDeclarePassive(configData.QueueName);
                    return res != null;
                }

                return false;
            }
            catch (Exception ex) { return false; }
            finally
            {
                Close();
            }
        }

        public bool AddQueue(RabbitConfigurationData configData, long expirationMiliSec = 0)
        {
            if (IsQueueExist(configData))
            {
                log.Error("AddQueue: Error, queue already exists!");
                return false;
            }

            try
            {
                if (this.GetInstance(configData, QueueAction.Ack) && this.m_Connection != null && this.m_Model != null)
                {
                    Dictionary<string, object> args = null;
                    if (expirationMiliSec > 0)
                    {
                        args = new Dictionary<string, object>();
                        args.Add("x-expires", expirationMiliSec);
                    }

                    QueueDeclareOk res = this.m_Model.QueueDeclare(configData.QueueName, true, false, false,args);
                    this.m_Model.QueueBind(configData.QueueName, "scheduled_tasks", configData.RoutingKey);

                    return res != null && res.QueueName == configData.QueueName;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error("AddQueue: Error - " + ex);
                return false;
            }
            finally
            {
                Close();
            }
        }

        public bool DeleteQueue(RabbitConfigurationData configData)
        {
            if (!IsQueueExist(configData))
            {
                log.Error("DeleteQueue: Error, queue doesn't exist!");
                return false;
            }

            try
            {
                if (this.GetInstance(configData, QueueAction.Ack) && this.m_Connection != null && this.m_Model != null)
                {
                    this.m_Model.QueueDelete(configData.QueueName);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error("DeleteQueue: Error - " + ex);
                return false;
            }
            finally
            {
                Close();
            }
        }

        #endregion

        #region Private Methods

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
                            var factory = new ConnectionFactory()
                            {
                                HostName = configuration.Host,
                                Password = configuration.Password,
                                UserName = configuration.Username,
                            };

                            int port;

                            if (int.TryParse(configuration.Port, out port))
                            {
                                factory.Port = port;
                            }

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
                        log.Error("Failed creating instance of Rabbit Connection.", ex);
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

        #endregion

        #region Dispose

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
