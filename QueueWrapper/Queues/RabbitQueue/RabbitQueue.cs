using QueueWrapper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueueWrapper
{
    public class RabbitQueue : IQueueImpl
    {
        #region Members

        private string m_sHostName = string.Empty;
        private string m_sUserName = string.Empty;
        private string m_sPassword = string.Empty;
        private string m_sPort = string.Empty;
        private string m_sRoutingKey = string.Empty;
        private string m_sExchange = string.Empty;
        private string m_sQueue = string.Empty;
        private string m_sVirtualHost = string.Empty;
        private string m_sExchangeType = string.Empty;
        private bool m_bSetContentType = false;
        private ConfigType m_eConfigType = ConfigType.DefaultConfig;

        #endregion

        #region CTOR

        public RabbitQueue()
        {
            ReadRabbitParameters();
        }

        //the parameter will ensure that the config values are the ones relevent for the speceific Queue Type 
        public RabbitQueue(ConfigType eType, bool bSetContentType)
        {
            m_eConfigType = eType;
            m_bSetContentType = bSetContentType;
            ReadRabbitParameters();
        }

        #endregion

        #region IQueuable Methods

        public virtual bool Enqueue(string sDataToIndex, string sRouteKey)
        {
            bool bIsEnqueueSucceeded = false;
            try
            {
                if (!string.IsNullOrEmpty(sDataToIndex))
                {
                    RabbitConfigurationData configData = CreateRabbitConfigurationData();
                    configData.RoutingKey = sRouteKey;

                    if (configData != null)
                    {
                        bIsEnqueueSucceeded = RabbitConnection.Instance.Publish(configData, sDataToIndex);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return bIsEnqueueSucceeded;
        }

        public virtual T Dequeue<T>(string sQueueName, out string sAckId)
        {
            sAckId = string.Empty;

            T sReturnedData = default(T);
            string sMessage = string.Empty;

            if (!string.IsNullOrEmpty(sQueueName))
            {
                RabbitConfigurationData configData = CreateRabbitConfigurationData();
                if (configData != null)
                {
                    configData.QueueName = sQueueName;
                    sMessage = RabbitConnection.Instance.Subscribe(configData, ref sAckId);

                    if (!string.IsNullOrEmpty(sMessage))
                    {
                        sReturnedData = Utils.JsonToObject<T>(sMessage);
                    }
                }
            }
            else
            {
                // Write log ?
            }

            return sReturnedData;
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

        #endregion

        #region Private Methods

        private void ReadRabbitParameters()
        {
            m_sHostName = Utils.GetConfigValue("hostName");
            m_sUserName = Utils.GetConfigValue("userName");
            m_sPassword = Utils.GetConfigValue("password");
            m_sPort = Utils.GetConfigValue("port");

            switch (m_eConfigType)
            {
                case ConfigType.DefaultConfig:
                    {
                        m_sRoutingKey = Utils.GetConfigValue("routingKey");
                        m_sExchange = Utils.GetConfigValue("exchange");
                        m_sQueue = Utils.GetConfigValue("queue");
                        m_sVirtualHost = Utils.GetConfigValue("virtualHost");
                        m_sExchangeType = Utils.GetConfigValue("exchangeType");
                        break;
                    }
                case ConfigType.PictureConfig:
                    {
                        m_sRoutingKey = Utils.GetConfigValue("routingKeyPicture");
                        m_sExchange = Utils.GetConfigValue("exchangePicture");
                        m_sQueue = Utils.GetConfigValue("queuePicture");
                        m_sVirtualHost = Utils.GetConfigValue("virtualHostPicture");
                        m_sExchangeType = Utils.GetConfigValue("exchangeTypePicture");
                        break;
                    }
                case ConfigType.SocialFeedConfig:
                    {
                        m_sRoutingKey = Utils.GetConfigValue("routingKeySocialFeed");
                        m_sExchange = Utils.GetConfigValue("exchangeSocialFeed");
                        m_sQueue = Utils.GetConfigValue("queueSocialFeed");
                        m_sVirtualHost = Utils.GetConfigValue("virtualHostSocialFeed");
                        m_sExchangeType = Utils.GetConfigValue("exchangeTypeSocialFeed");

                        break;
                    }
                case ConfigType.EPGConfig:
                    {
                        m_sRoutingKey = Utils.GetConfigValue("routingKeyEPG");
                        m_sExchange = Utils.GetConfigValue("exchangeEPG");
                        m_sQueue = Utils.GetConfigValue("queueEPG");
                        m_sVirtualHost = Utils.GetConfigValue("virtualHostEPG");
                        m_sExchangeType = Utils.GetConfigValue("exchangeTypeEPG");

                        break;
                    }
                case ConfigType.ProfessionalServicesNotificationsConfig:
                    {
                        m_sRoutingKey = "*";
                        m_sExchange = Utils.GetConfigValue("ProfessionalServices.exchange");
                        m_sQueue = ".";
                        m_sVirtualHost = Utils.GetConfigValue("ProfessionalServices.virtualHost");
                        m_sExchangeType = Utils.GetConfigValue("ProfessionalServices.exchangeType");

                        break;
                    }
                default:
                    break;
            }
        }

        protected virtual RabbitConfigurationData CreateRabbitConfigurationData()
        {

            RabbitConfigurationData configData = null;

            if (!string.IsNullOrEmpty(this.m_sHostName) && !string.IsNullOrEmpty(this.m_sUserName) && !string.IsNullOrEmpty(this.m_sPassword) && !string.IsNullOrEmpty(this.m_sPort)
                && !string.IsNullOrEmpty(this.m_sRoutingKey) && !string.IsNullOrEmpty(this.m_sExchange) && !string.IsNullOrEmpty(this.m_sQueue) && !string.IsNullOrEmpty(this.m_sVirtualHost)
                && !string.IsNullOrEmpty(this.m_sExchangeType))
            {
                configData = new RabbitConfigurationData(m_sExchange, m_sQueue, m_sRoutingKey, m_sHostName, m_sPassword, m_sExchangeType, m_sVirtualHost, m_sUserName, m_sPort);
                if (m_bSetContentType)
                {
                    configData.setContentType = true;
                }
            }

            return configData;
        }

        public virtual void Dispose()
        {
            RabbitConnection.Instance.Dispose();
        }

        #endregion



    }
}
