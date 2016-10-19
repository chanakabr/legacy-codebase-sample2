using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using RabbitMQ.Client.Impl;

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
            hostName = Utils.GetConfigValue("hostName");
            userName = Utils.GetConfigValue("userName");
            password = Utils.GetConfigValue("password");
            port = Utils.GetConfigValue("port");

            switch (m_eConfigType)
            {
                case ConfigType.DefaultConfig:
                    {
                        routingKey = Utils.GetConfigValue("routingKey");
                        exchange = Utils.GetConfigValue("exchange");
                        queue = Utils.GetConfigValue("queue");
                        virtualHost = Utils.GetConfigValue("virtualHost");
                        exchangeType = Utils.GetConfigValue("exchangeType");
                        break;
                    }
                case ConfigType.PictureConfig:
                    {
                        routingKey = Utils.GetConfigValue("routingKeyPicture");
                        exchange = Utils.GetConfigValue("exchangePicture");
                        queue = Utils.GetConfigValue("queuePicture");
                        virtualHost = Utils.GetConfigValue("virtualHostPicture");
                        exchangeType = Utils.GetConfigValue("exchangeTypePicture");
                        break;
                    }
                case ConfigType.SocialFeedConfig:
                    {
                        routingKey = Utils.GetConfigValue("routingKeySocialFeed");
                        exchange = Utils.GetConfigValue("exchangeSocialFeed");
                        queue = Utils.GetConfigValue("queueSocialFeed");
                        virtualHost = Utils.GetConfigValue("virtualHostSocialFeed");
                        exchangeType = Utils.GetConfigValue("exchangeTypeSocialFeed");

                        break;
                    }
                case ConfigType.EPGConfig:
                    {
                        routingKey = Utils.GetConfigValue("routingKeyEPG");
                        exchange = Utils.GetConfigValue("exchangeEPG");
                        queue = Utils.GetConfigValue("queueEPG");
                        virtualHost = Utils.GetConfigValue("virtualHostEPG");
                        exchangeType = Utils.GetConfigValue("exchangeTypeEPG");

                        break;
                    }
                case ConfigType.ProfessionalServicesNotificationsConfig:
                    {
                        routingKey = Utils.GetConfigValue("ProfessionalServices.routingKey");
                        exchange = Utils.GetConfigValue("ProfessionalServices.exchange");
                        queue = ".";
                        virtualHost = Utils.GetConfigValue("ProfessionalServices.virtualHost");
                        exchangeType = Utils.GetConfigValue("ProfessionalServices.exchangeType");

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
                        routingKey = Utils.GetConfigValue("IndexingData.routingKey");
                        exchange = Utils.GetConfigValue("IndexingData.exchange");
                        queue = ".";
                        virtualHost = Utils.GetConfigValue("IndexingData.virtualHost");
                        exchangeType = Utils.GetConfigValue("IndexingData.exchangeType");

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
                        routingKey = Utils.GetConfigValue("PushNotifications.routingKey");
                        if (string.IsNullOrEmpty(routingKey))
                            routingKey = "*";

                        exchange = Utils.GetConfigValue("PushNotifications.exchange");
                        if (string.IsNullOrEmpty(exchange))
                            exchange = "push_notifications";

                        virtualHost = Utils.GetConfigValue("PushNotifications.virtualHost");
                        if (string.IsNullOrEmpty(virtualHost))
                            virtualHost = "/";

                        exchangeType = Utils.GetConfigValue("PushNotifications.exchangeType");
                        if (string.IsNullOrEmpty(exchangeType))
                            exchangeType = "topic";

                        queue = ".";

                        break;
                    }

                case ConfigType.ImageUpload:
                    {
                        routingKey = Utils.GetConfigValue("routingKey");
                        exchange = Utils.GetConfigValue("ImageUpload.exchange");
                        queue = Utils.GetConfigValue("queue");
                        virtualHost = Utils.GetConfigValue("virtualHost");
                        exchangeType = Utils.GetConfigValue("exchangeType");
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
