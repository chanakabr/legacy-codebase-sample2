using QueueWrapper.Queues;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace QueueWrapper
{
    public class RabbitConnection : IDisposable
    {
        #region Members

        private IConnection m_Connection;
        private IModel m_Model;

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
            }

            internal static readonly RabbitConnection Instance = new RabbitConnection();
        }

        #endregion

        #region Public Functions

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
            bool bIsInstanceExist = this.GetInstance(configuration, QueueAction.Publish);

            if (m_Connection != null)
            {
                try
                {
                    this.m_Model = m_Connection.CreateModel();
                    if (this.m_Model != null)
                    {
                        var body = Encoding.UTF8.GetBytes(sMessage.ToString());
                        IBasicProperties properties = m_Model.CreateBasicProperties();
                        m_Model.BasicPublish(configuration.Exchange, configuration.RoutingKey, properties, body);
                    }
                }
                catch  (Exception ex)
                {

                }
            }

            return bIsInstanceExist;
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
                            ////string sRabbitEndpoint = Utils.GetConfigValue("queue-endpoint");
                            //if (!string.IsNullOrEmpty(sRabbitEndpoint))
                            //{COMPUTERNAME
                            //var factory = new ConnectionFactory() { HostName = configuration.Host, Password = configuration.Password };// , Endpoint = new AmqpTcpEndpoint(new Uri(sRabbitEndpoint)) };
                            var factory = new ConnectionFactory() { HostName = "ubuntu-2-cache", Password = configuration.Password };//, Endpoint = new AmqpTcpEndpoint(new Uri("http://ubuntu-2-cache:5672")) };

                            this.m_Connection = factory.CreateConnection();
                            if (action.Equals(QueueAction.Subscribe) || action.Equals(QueueAction.Ack))
                            {
                                this.m_Model = this.m_Connection.CreateModel();
                            }
                            bIsGetInstanceSucceeded = true;
                            //}
                        }
                    }
                    catch (Exception ex)
                    {

                        m_Connection = null;
                        m_Model = null;
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
