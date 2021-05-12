using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using RabbitMQ.Client.Impl;
using ConfigurationManager;

namespace QueueWrapper
{
    public class RabbitQueue : IQueueImpl
    {
        private static readonly KLogger log = new KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Must Have Members

        private string hostName = string.Empty;
        private string userName = string.Empty;
        private string password = string.Empty;
        private string exchange = string.Empty;

        #endregion

        #region Optional Members

        private string port = string.Empty;
        private string contentType = string.Empty;

        public string ContentType
        {
            get
            {
                return contentType;
            }
            set
            {
                contentType = value;
            }
        }

        #endregion

        #region Essentials for ENQUEUE

        #endregion

        #region Essentials for DEQUEUE


        #endregion

        #region Essentials for Single Connection

        private string queue = string.Empty;
        private string routingKey = string.Empty;

        #endregion

        #region Unclear Members

        private string virtualHost = string.Empty;
        private string exchangeType = string.Empty;

        #endregion

        private ConfigType m_eConfigType = ConfigType.DefaultConfig;

        public RabbitQueue(IApplicationConfiguration applicationConfiguration)
        {
            ReadRabbitParameters(applicationConfiguration);
        }

        public RabbitQueue()
        {
            ReadRabbitParameters(ApplicationConfiguration.Current);
        }


        /// <summary>
        /// Initialize RabbitQueue - with option to specify if default content type is needed
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useDefaultContentType"></param>
        public RabbitQueue(ConfigType type, bool useDefaultContentType = true)
        {
            m_eConfigType = type;

            if (useDefaultContentType)
            {
                this.contentType = "application/json";
            }

            ReadRabbitParameters(ApplicationConfiguration.Current);
        }

        /// <summary>
        /// Initialiazes a Rabbit Connection and enqueues a message to it
        /// </summary>
        /// <param name="dataToIndex"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        public virtual bool Enqueue(string dataToIndex, string routingKey, long expirationMiliSec = 0)
        {
            bool success = false;

            try
            {
                if (!string.IsNullOrEmpty(dataToIndex))
                {
                    RabbitConfigurationData configurationData = CreateRabbitConfigurationData();

                    if (configurationData != null)
                    {
                        configurationData.RoutingKey = routingKey;
                        
                        success = RabbitConnection.Instance.Publish(configurationData, dataToIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed enqueue of message {0}, ex:{1}", dataToIndex, ex);
            }

            return success;
        }

        public virtual T Dequeue<T>(string queueName, out string ackId)
        {
            ackId = string.Empty;

            T returnedObject = default(T);
            string message = string.Empty;

            if (!string.IsNullOrEmpty(queueName))
            {
                RabbitConfigurationData configData = CreateRabbitConfigurationData();

                if (configData != null)
                {
                    configData.QueueName = queueName;

                    message = RabbitConnection.Instance.Subscribe(configData, ref ackId);

                    if (!string.IsNullOrEmpty(message))
                    {
                        returnedObject = Utils.JsonToObject<T>(message);
                    }
                }
            }
            else
            {
                // Write log ?
            }

            return returnedObject;
        }

        public virtual bool Ack(string sQueueName, string sAckId)
        {
            bool bResult = false;

            RabbitConfigurationData configData = CreateRabbitConfigurationData();
            if (configData != null)
            {
                configData.QueueName = sQueueName;
                bResult = RabbitConnection.Instance.Ack(configData, sAckId);
            }

            return bResult;
        }

        public virtual bool IsQueueExist(string name)
        {
            RabbitConfigurationData configData = CreateRabbitConfigurationData();

            if (configData != null)
            {
                configData.QueueName = name;

                return RabbitConnection.Instance.IsQueueExist(configData);
            }

            return false;
        }

        public virtual bool AddQueue(string name, string routingKey, long expirationMiliSec = 0)
        {
            RabbitConfigurationData configData = CreateRabbitConfigurationData();

            if (configData != null)
            {
                configData.QueueName = name;
                configData.RoutingKey = routingKey;
                return RabbitConnection.Instance.AddQueue(configData, expirationMiliSec);
            }

            return false;
        }

        public virtual bool DeleteQueue(string name)
        {
            RabbitConfigurationData configData = CreateRabbitConfigurationData();

            if (configData != null)
            {
                configData.QueueName = name;
                configData.RoutingKey = routingKey;
                return RabbitConnection.Instance.DeleteQueue(configData);
            }

            return false;
        }

        public virtual bool HealthCheck()
        {
            bool result = false;

            try
            {
                RabbitConfigurationData configurationData = CreateRabbitConfigurationData();

                result = RabbitConnection.Instance.HealthCheck(configurationData);
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private void ReadRabbitParameters(IApplicationConfiguration applicationConfiguration)
        {
            var rabbitConfig = applicationConfiguration.RabbitConfiguration;
            hostName = rabbitConfig.Default.HostName.Value;
            userName = rabbitConfig.Default.UserName.Value;
            password = rabbitConfig.Default.Password.Value;
            port = rabbitConfig.Default.Port.Value.ToString();

            switch (m_eConfigType)
            {
                case ConfigType.DefaultConfig:
                    {
                        routingKey = rabbitConfig.Default.RoutingKey.Value;
                        exchange = rabbitConfig.Default.Exchange.Value;
                        queue = rabbitConfig.Default.Queue.Value;
                        virtualHost = rabbitConfig.Default.VirtualHost.Value;
                        exchangeType = rabbitConfig.Default.ExchangeType.Value;
                        break;
                    }
                case ConfigType.PictureConfig:
                    {
                        routingKey = rabbitConfig.Picture.RoutingKey.Value;
                        exchange = rabbitConfig.Picture.Exchange.Value;
                        queue = rabbitConfig.Picture.Queue.Value;
                        virtualHost = rabbitConfig.Picture.VirtualHost.Value;
                        exchangeType = rabbitConfig.Picture.ExchangeType.Value;
                        break;
                    }
                case ConfigType.SocialFeedConfig:
                    {
                        routingKey = rabbitConfig.SocialFeed.RoutingKey.Value;
                        exchange = rabbitConfig.SocialFeed.Exchange.Value;
                        queue = rabbitConfig.SocialFeed.Queue.Value;
                        virtualHost = rabbitConfig.SocialFeed.VirtualHost.Value;
                        exchangeType = rabbitConfig.SocialFeed.ExchangeType.Value;

                        break;
                    }
                case ConfigType.EPGConfig:
                    {
                        routingKey = rabbitConfig.EPG.RoutingKey.Value;
                        exchange = rabbitConfig.EPG.Exchange.Value;
                        queue = rabbitConfig.EPG.Queue.Value;
                        virtualHost = rabbitConfig.EPG.VirtualHost.Value;
                        exchangeType = rabbitConfig.EPG.ExchangeType.Value;

                        break;
                    }
                case ConfigType.ProfessionalServicesNotificationsConfig:
                    {
                        routingKey = rabbitConfig.ProfessionalServices.RoutingKey.Value;
                        exchange = rabbitConfig.ProfessionalServices.Exchange.Value;
                        queue = ".";
                        virtualHost = rabbitConfig.ProfessionalServices.VirtualHost.Value;
                        exchangeType = rabbitConfig.ProfessionalServices.ExchangeType.Value;
                        
                        // default values - to avoid embarrassments 
                        if (string.IsNullOrEmpty(routingKey))
                        {
                            routingKey = "*";
                        }

                        if (string.IsNullOrEmpty(virtualHost))
                        {
                            virtualHost = "/";
                        }

                        if (string.IsNullOrEmpty(exchangeType))
                        {
                            exchangeType = "topic";
                        }

                        if (string.IsNullOrEmpty(exchange))
                        {
                            exchange = "ps_notifications";
                        }

                        break;
                    }
                case ConfigType.IndexingDataConfig:
                    {
                        routingKey = rabbitConfig.Indexing.RoutingKey.Value;
                        exchange = rabbitConfig.Indexing.Exchange.Value;
                        queue = ".";
                        virtualHost = rabbitConfig.Indexing.VirtualHost.Value;
                        exchangeType = rabbitConfig.Indexing.ExchangeType.Value;

                        // default values - to avoid embarrassments 
                        if (string.IsNullOrEmpty(routingKey))
                        {
                            routingKey = "*";
                        }

                        if (string.IsNullOrEmpty(virtualHost))
                        {
                            virtualHost = "/";
                        }

                        if (string.IsNullOrEmpty(exchangeType))
                        {
                            exchangeType = "topic";
                        }

                        if (string.IsNullOrEmpty(exchange))
                        {
                            exchange = "scheduled_tasks";
                        }

                        break;
                    }

                case ConfigType.PushNotifications:
                    {
                        routingKey = rabbitConfig.PushNotification.RoutingKey.Value;
                        if (string.IsNullOrEmpty(routingKey))
                            routingKey = "*";

                        exchange = rabbitConfig.PushNotification.Exchange.Value;
                        if (string.IsNullOrEmpty(exchange))
                            exchange = "push_notifications";

                        virtualHost = rabbitConfig.PushNotification.VirtualHost.Value;
                        if (string.IsNullOrEmpty(virtualHost))
                            virtualHost = "/";

                        exchangeType = rabbitConfig.PushNotification.ExchangeType.Value;
                        if (string.IsNullOrEmpty(exchangeType))
                            exchangeType = "topic";

                        queue = ".";

                        break;
                    }

                case ConfigType.ImageUpload:
                    {
                        routingKey = rabbitConfig.ImageUpload.RoutingKey.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            routingKey = rabbitConfig.Default.RoutingKey.Value;
                        }

                        exchange = rabbitConfig.ImageUpload.Exchange.Value;

                        queue = rabbitConfig.ImageUpload.Queue.Value;

                        if (string.IsNullOrEmpty(queue))
                        {
                            queue = rabbitConfig.Default.Queue.Value;
                        }

                        virtualHost = rabbitConfig.ImageUpload.VirtualHost.Value;

                        if (string.IsNullOrEmpty(virtualHost))
                        {
                            virtualHost = rabbitConfig.Default.VirtualHost.Value;
                        }

                        exchangeType = rabbitConfig.ImageUpload.ExchangeType.Value;

                        if (string.IsNullOrEmpty(exchangeType))
                        {
                            exchangeType = rabbitConfig.Default.ExchangeType.Value;
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        public virtual RabbitConfigurationData CreateRabbitConfigurationData()
        {
            RabbitConfigurationData configData = null;

            // If any of these members is missing, we have nothing to do
            if (string.IsNullOrEmpty(hostName) ||
                string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(exchange))
            {
                log.Error("Could not create Rabbit configuration data. Essential TCM values are missing. Check TCM of hostname, username, password, exchange.");
            }
            else
            {
                configData = new RabbitConfigurationData(exchange, queue, routingKey, hostName, password, exchangeType, virtualHost, userName, port, contentType);
            }

            return configData;
        }

        public virtual void Dispose()
        {
            RabbitConnection.Instance.Dispose();
        }
    }
}
