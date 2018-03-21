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

        public RabbitQueue()
        {
            ReadRabbitParameters();
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

            ReadRabbitParameters();
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

                        RabbitConnection.Instance.ResetFailCounter();
                        success = RabbitConnection.Instance.Publish(configurationData, dataToIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed enqueue of message {0}", dataToIndex, ex);
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

        private void ReadRabbitParameters()
        {
            hostName = ApplicationConfiguration.RabbitConfiguration.Default.HostName.Value;
            userName = ApplicationConfiguration.RabbitConfiguration.Default.UserName.Value;
            password = ApplicationConfiguration.RabbitConfiguration.Default.Password.Value;
            port = ApplicationConfiguration.RabbitConfiguration.Default.Port.IntValue.ToString();

            switch (m_eConfigType)
            {
                case ConfigType.DefaultConfig:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.Default.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.Default.Exchange.Value;
                        queue = ApplicationConfiguration.RabbitConfiguration.Default.Queue.Value;
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.Default.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.Default.ExchangeType.Value;
                        break;
                    }
                case ConfigType.PictureConfig:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.Picture.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.Picture.Exchange.Value;
                        queue = ApplicationConfiguration.RabbitConfiguration.Picture.Queue.Value;
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.Picture.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.Picture.ExchangeType.Value;
                        break;
                    }
                case ConfigType.SocialFeedConfig:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.SocialFeed.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.SocialFeed.Exchange.Value;
                        queue = ApplicationConfiguration.RabbitConfiguration.SocialFeed.Queue.Value;
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.SocialFeed.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.SocialFeed.ExchangeType.Value;

                        break;
                    }
                case ConfigType.EPGConfig:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.EPG.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.EPG.Exchange.Value;
                        queue = ApplicationConfiguration.RabbitConfiguration.EPG.Queue.Value;
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.EPG.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.EPG.ExchangeType.Value;

                        break;
                    }
                case ConfigType.ProfessionalServicesNotificationsConfig:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.ProfessionalServices.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.ProfessionalServices.Exchange.Value;
                        queue = ".";
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.ProfessionalServices.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.ProfessionalServices.ExchangeType.Value;
                        
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
                        routingKey = ApplicationConfiguration.RabbitConfiguration.Indexing.RoutingKey.Value;
                        exchange = ApplicationConfiguration.RabbitConfiguration.Indexing.Exchange.Value;
                        queue = ".";
                        virtualHost = ApplicationConfiguration.RabbitConfiguration.Indexing.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.RabbitConfiguration.Indexing.ExchangeType.Value;

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
                        routingKey = ApplicationConfiguration.RabbitConfiguration.PushNotification.RoutingKey.Value;
                        if (string.IsNullOrEmpty(routingKey))
                            routingKey = "*";

                        exchange = ApplicationConfiguration.RabbitConfiguration.PushNotification.Exchange.Value;
                        if (string.IsNullOrEmpty(exchange))
                            exchange = "push_notifications";

                        virtualHost = ApplicationConfiguration.RabbitConfiguration.PushNotification.VirtualHost.Value;
                        if (string.IsNullOrEmpty(virtualHost))
                            virtualHost = "/";

                        exchangeType = ApplicationConfiguration.RabbitConfiguration.PushNotification.ExchangeType.Value;
                        if (string.IsNullOrEmpty(exchangeType))
                            exchangeType = "topic";

                        queue = ".";

                        break;
                    }

                case ConfigType.ImageUpload:
                    {
                        routingKey = ApplicationConfiguration.RabbitConfiguration.ImageUpload.RoutingKey.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            routingKey = ApplicationConfiguration.RabbitConfiguration.Default.RoutingKey.Value;
                        }

                        exchange = ApplicationConfiguration.RabbitConfiguration.ImageUpload.Exchange.Value;

                        queue = ApplicationConfiguration.RabbitConfiguration.ImageUpload.Queue.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            queue = ApplicationConfiguration.RabbitConfiguration.Default.Queue.Value;
                        }

                        virtualHost = ApplicationConfiguration.RabbitConfiguration.ImageUpload.VirtualHost.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            virtualHost = ApplicationConfiguration.RabbitConfiguration.Default.VirtualHost.Value;
                        }

                        exchangeType = ApplicationConfiguration.RabbitConfiguration.ImageUpload.ExchangeType.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            exchangeType = ApplicationConfiguration.RabbitConfiguration.Default.ExchangeType.Value;
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        protected virtual RabbitConfigurationData CreateRabbitConfigurationData()
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
