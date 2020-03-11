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

        private void ReadRabbitParameters()
        {
            hostName = ApplicationConfiguration.Current.RabbitConfiguration.Default.HostName.Value;
            userName = ApplicationConfiguration.Current.RabbitConfiguration.Default.UserName.Value;
            password = ApplicationConfiguration.Current.RabbitConfiguration.Default.Password.Value;
            port = ApplicationConfiguration.Current.RabbitConfiguration.Default.Port.Value.ToString();

            switch (m_eConfigType)
            {
                case ConfigType.DefaultConfig:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.Default.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.Default.Exchange.Value;
                        queue = ApplicationConfiguration.Current.RabbitConfiguration.Default.Queue.Value;
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.Default.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.Default.ExchangeType.Value;
                        break;
                    }
                case ConfigType.PictureConfig:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.Picture.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.Picture.Exchange.Value;
                        queue = ApplicationConfiguration.Current.RabbitConfiguration.Picture.Queue.Value;
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.Picture.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.Picture.ExchangeType.Value;
                        break;
                    }
                case ConfigType.SocialFeedConfig:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.SocialFeed.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.SocialFeed.Exchange.Value;
                        queue = ApplicationConfiguration.Current.RabbitConfiguration.SocialFeed.Queue.Value;
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.SocialFeed.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.SocialFeed.ExchangeType.Value;

                        break;
                    }
                case ConfigType.EPGConfig:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.EPG.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.EPG.Exchange.Value;
                        queue = ApplicationConfiguration.Current.RabbitConfiguration.EPG.Queue.Value;
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.EPG.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.EPG.ExchangeType.Value;

                        break;
                    }
                case ConfigType.ProfessionalServicesNotificationsConfig:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.ProfessionalServices.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.ProfessionalServices.Exchange.Value;
                        queue = ".";
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.ProfessionalServices.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.ProfessionalServices.ExchangeType.Value;
                        
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
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.Indexing.RoutingKey.Value;
                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.Indexing.Exchange.Value;
                        queue = ".";
                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.Indexing.VirtualHost.Value;
                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.Indexing.ExchangeType.Value;

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
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.PushNotification.RoutingKey.Value;
                        if (string.IsNullOrEmpty(routingKey))
                            routingKey = "*";

                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.PushNotification.Exchange.Value;
                        if (string.IsNullOrEmpty(exchange))
                            exchange = "push_notifications";

                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.PushNotification.VirtualHost.Value;
                        if (string.IsNullOrEmpty(virtualHost))
                            virtualHost = "/";

                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.PushNotification.ExchangeType.Value;
                        if (string.IsNullOrEmpty(exchangeType))
                            exchangeType = "topic";

                        queue = ".";

                        break;
                    }

                case ConfigType.ImageUpload:
                    {
                        routingKey = ApplicationConfiguration.Current.RabbitConfiguration.ImageUpload.RoutingKey.Value;

                        if (string.IsNullOrEmpty(routingKey))
                        {
                            routingKey = ApplicationConfiguration.Current.RabbitConfiguration.Default.RoutingKey.Value;
                        }

                        exchange = ApplicationConfiguration.Current.RabbitConfiguration.ImageUpload.Exchange.Value;

                        queue = ApplicationConfiguration.Current.RabbitConfiguration.ImageUpload.Queue.Value;

                        if (string.IsNullOrEmpty(queue))
                        {
                            queue = ApplicationConfiguration.Current.RabbitConfiguration.Default.Queue.Value;
                        }

                        virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.ImageUpload.VirtualHost.Value;

                        if (string.IsNullOrEmpty(virtualHost))
                        {
                            virtualHost = ApplicationConfiguration.Current.RabbitConfiguration.Default.VirtualHost.Value;
                        }

                        exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.ImageUpload.ExchangeType.Value;

                        if (string.IsNullOrEmpty(exchangeType))
                        {
                            exchangeType = ApplicationConfiguration.Current.RabbitConfiguration.Default.ExchangeType.Value;
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
