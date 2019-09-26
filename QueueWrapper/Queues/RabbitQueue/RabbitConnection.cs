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
using ConfigurationManager;
using System.Collections.Concurrent;

namespace QueueWrapper
{
    public class RabbitConnection : IDisposable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const int DEFAULT_FAIL_COUNTER_LIMIT = 3;

        #region Data Members
        
        private ConcurrentDictionary<string, IConnection> connectionDictionary;
        private ReaderWriterLockSlim m_lock;
        private int failCounterLimit = DEFAULT_FAIL_COUNTER_LIMIT;

        #endregion
        
        #region Ctor and initialization

        private RabbitConnection()
        {
            connectionDictionary = new ConcurrentDictionary<string, IConnection>();
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
                    Instance.failCounterLimit = DEFAULT_FAIL_COUNTER_LIMIT;

                    try
                    {
                        int tcmFailCountLimit = ApplicationConfiguration.QueueFailLimit.IntValue;

                        if (tcmFailCountLimit > 0)
                        {
                            Instance.failCounterLimit = tcmFailCountLimit;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Error in nested rabbit", ex);
                    }
                }
            }

            internal static readonly RabbitConnection Instance = new RabbitConnection();
        }

        #endregion

        #region Main Methods

        public bool Ack(RabbitConfigurationData configuration, string sAckId)
        {
            bool result = false;

            int retryCount = 0;
            IConnection connection;
            this.GetInstance(configuration, QueueAction.Ack, ref retryCount, out connection);

            if (connection != null)
            {
                try
                {
                    var model = this.GetModel(configuration.Host, connection);

                    if (model != null)
                    {
                        ulong a = ulong.Parse(sAckId);

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                        {
                            Database = configuration.Exchange
                        })
                        {
                            model.BasicAck(ulong.Parse(sAckId), false);
                        }

                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error in Ack rabbit", ex);
                }
            }

            return result;
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
            int retryCount = 0;

            // Check if we reached the writing limit
            while (retryCount < this.failCounterLimit && !isPublishSucceeded)
            {
                bool instanecExists = false;
                IConnection connection;
                instanecExists = this.GetInstance(configuration, QueueAction.Publish, ref retryCount, out connection);

                if (connection != null && instanecExists)
                {
                    IModel model = null;

                    try
                    {
                        model = this.GetModel(configuration.Host, connection);

                        if (model == null)
                        {
                            log.ErrorFormat("Could not get model when publishing message {0} to host {1} on retry number {2}",
                                message, configuration.Host, retryCount);
                            retryCount++;
                        }
                        else
                        {
                            var body = Encoding.UTF8.GetBytes(message.ToString());
                            IBasicProperties properties = model.CreateBasicProperties();

                            // should be "application/json"
                            if (!string.IsNullOrEmpty(configuration.ContentType))
                                properties.ContentType = configuration.ContentType;

                            // set message expiration
                            if (expirationMiliSec != 0)
                                properties.Expiration = expirationMiliSec.ToString();

                            properties.DeliveryMode = 2;
                            properties.Persistent = true;

                            using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                            {
                                Database = configuration.Exchange
                            })
                            {
                                model.BasicPublish(configuration.Exchange, configuration.RoutingKey, properties, body);
                            }

                            log.DebugFormat("Rabbit message sent. configuration.Exchange: {0}, configuration.RoutingKey: {1}, body: {2}",
                                configuration != null && configuration.Exchange != null ? configuration.Exchange : string.Empty,
                                configuration != null && configuration.RoutingKey != null ? configuration.RoutingKey : string.Empty,
                                message);

                            isPublishSucceeded = true;
                        }
                    }
                    catch (OperationInterruptedException ex)
                    {
                        log.ErrorFormat("OperationInterruptedException - Failed publishing message to rabbit. Message = {0}, EX = {1}, fail counter = {2}",
                            message, ex, retryCount);

                        ClearConnection(configuration.Host);
                        retryCount++;
                    }
                    catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                    {
                        log.ErrorFormat("BrokerUnreachableException - Failed publishing message to rabbit on publish. Message = {0}, EX = {1}, fail counter = {2}",
                            message, ex, retryCount);

                        ClearConnection(configuration.Host);
                        retryCount++;

                        Thread.Sleep(1000);
                    }
                    catch (RabbitMQClientException ex)
                    {
                        log.ErrorFormat("RabbitMQClientException - " +
                            "Failed publishing message to rabbit on publish. Message = {0}, EX = {1}, fail counter = {2}",
                            message, ex, retryCount);

                        ClearConnection(configuration.Host);
                        retryCount++;
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Failed publishing message to rabbit on publish. Message = {0}, EX = {1}, fail counter = {2}", message,
                            ex, retryCount);

                        ClearConnection(configuration.Host);
                        retryCount++;
                    }
                    finally
                    {
                        if (model != null && model.IsOpen)
                        {
                            model.Close();
                            model.Dispose();
                        }
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
                    retryCount++; 
                }
            }

            return isPublishSucceeded;
        }

        private IModel GetModel(string host, IConnection connection)
        {
            IModel model = null;

            if (connection != null)
            {
                try
                {
                    model = connection.CreateModel();
                }
                catch (OperationInterruptedException ex)
                {
                    ClearConnection(host);
                    log.ErrorFormat("Failed publishing message to rabbit on CreateModel(). OperationInterruptedException ex = {0}", ex);
                }
                catch (RabbitMQClientException ex)
                {
                    ClearConnection(host);
                    log.ErrorFormat("Failed publishing message to rabbit on CreateModel(). ex type = {1}, ex = {0}", ex, ex.GetType());
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed publishing message to rabbit on CreateModel(). ex type = {1}, ex = {0}", ex, ex.GetType());
                }
            }

            return model;
        }

        private void ClearConnection(string host)
        {
            if (this.connectionDictionary != null && this.connectionDictionary.ContainsKey(host))
            {
                bool createdNew = false;
                #if !NETSTANDARD2_0
                var mutexSecurity = Utils.CreateMutex();
                #endif

                IConnection connection;
                connectionDictionary.TryRemove(host, out connection);

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew))
                {
                    #if !NETSTANDARD2_0
                    mutex.SetAccessControl(mutexSecurity);
                    #endif
                    try
                    {
                        mutex.WaitOne(-1);

                        if (connection != null)
                        {
                            if (connection.IsOpen)
                            {
                                connection.Close();
                            }

                            connection.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed closing instance of Rabbit Connection.", ex);
                    }
                    finally
                    {
                        connection = null;
                    }

                    mutex.ReleaseMutex();
                }
            }
        }
        private void ClearConnections()
        {
            if (this.connectionDictionary != null)
            {
                bool createdNew = false;

                #if !NETSTANDARD2_0
                var mutexSecurity = Utils.CreateMutex();
                #endif

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew))
                {
                    #if !NETSTANDARD2_0
                    mutex.SetAccessControl(mutexSecurity);
                    #endif
                    try
                    {
                        mutex.WaitOne(-1);

                        foreach (var item in connectionDictionary)
                        {
                            if (item.Value.IsOpen)
                            {
                                item.Value.Close();
                            }

                            item.Value.Dispose();
                        }

                        connectionDictionary.Clear();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed closing instance of Rabbit Connection.", ex);
                    }
                    
                    mutex.ReleaseMutex();
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

            int retryCount = 0;
            IConnection connection;
            this.GetInstance(configuration, QueueAction.Subscribe, ref retryCount, out connection);

            if (connection != null)
            {
                try
                {
                    var model = this.GetModel(configuration.Host, connection);

                    if (model != null)
                    {

                        using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_RABBITMQ, null, null, null, null)
                        {
                            Database = configuration.Exchange
                        })
                        {
                            BasicGetResult bgr = model.BasicGet(configuration.QueueName, false);
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

        public bool IsQueueExist(RabbitConfigurationData configuration)
        {
            IModel model = null;

            try
            {
                IConnection connection;
                int retryCount = 0;
                if (this.GetInstance(configuration, QueueAction.Ack, ref retryCount, out connection) && connection != null)
                {
                    model = this.GetModel(configuration.Host, connection);
                    var res = model.QueueDeclarePassive(configuration.QueueName);

                    return res != null;
                }

                return false;
            }
            catch { return false; }
        }

        public bool AddQueue(RabbitConfigurationData configuration, long expirationMiliSec = 0)
        {
            if (IsQueueExist(configuration))
            {
                log.Error("AddQueue: Error, queue already exists!");
                return false;
            }

            IModel model = null;

            try
            {
                IConnection connection;
                int retryCount = 0;
                if (this.GetInstance(configuration, QueueAction.Ack, ref retryCount, out connection) && connection != null)
                {
                    Dictionary<string, object> args = null;
                    if (expirationMiliSec > 0)
                    {
                        args = new Dictionary<string, object>();
                        args.Add("x-expires", expirationMiliSec);
                    }

                    model = this.GetModel(configuration.Host, connection);

                    QueueDeclareOk res = model.QueueDeclare(configuration.QueueName, true, false, false,args);
                    model.QueueBind(configuration.QueueName, "scheduled_tasks", configuration.RoutingKey);
                    
                    return res != null && res.QueueName == configuration.QueueName;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error("AddQueue: Error - " + ex);
                return false;
            }
        }

        public bool DeleteQueue(RabbitConfigurationData configuration)
        {
            if (!IsQueueExist(configuration))
            {
                log.Error("DeleteQueue: Error, queue doesn't exist!");
                return false;
            }

            IModel model = null;
            try
            {
                IConnection connection;
                int retryCount = 0;
                if (this.GetInstance(configuration, QueueAction.Ack, ref retryCount, out connection) && connection != null)
                {
                    model = this.GetModel(configuration.Host, connection);
                    model.QueueDelete(configuration.QueueName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                log.Error("DeleteQueue: Error - " + ex);
                return false;
            }
        }

        #endregion

        #region Private Methods

        private bool GetInstance(RabbitConfigurationData configuration, QueueAction action, ref int retryCount, out IConnection connection)
        {
            bool getInstanceSucceeded = false;

            connection = null;

            if (configuration == null || string.IsNullOrEmpty(configuration.Host))
            {
                return false;
            }
            
            if (!connectionDictionary.ContainsKey(configuration.Host))
            {
                bool createdNew = false;
                #if !NETSTANDARD2_0
                var mutexSecurity = Utils.CreateMutex();
                #endif

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew))
                {
                    #if !NETSTANDARD2_0
                    mutex.SetAccessControl(mutexSecurity);
                    #endif

                    try
                    {
                        mutex.WaitOne(-1);
                        if (!connectionDictionary.ContainsKey(configuration.Host))
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

                            connection = factory.CreateConnection();

                            connectionDictionary[configuration.Host] = connection;
                            getInstanceSucceeded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failed creating instance of Rabbit Connection.", ex);
                        retryCount++;
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            else
            {
                connection = connectionDictionary[configuration.Host];
                getInstanceSucceeded = true;
            }

            return getInstanceSucceeded;
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            ClearConnections();
        }
        
        #endregion
    }
}
