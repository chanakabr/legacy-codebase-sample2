using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Logger
{
    public class ConnectionHelper
    {
        #region Members

        private IConnection m_Connection;
        private IModel m_Model;        

        #endregion

        #region CTOR

        private ConnectionHelper()
        {
        }

        #endregion

        #region Singleton

        public static ConnectionHelper Instance
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

            internal static readonly ConnectionHelper Instance = new ConnectionHelper();
        }

        #endregion

        #region Public Functions

        public void Write(string sExchange, string sQueueName, string sMessage, string sRoutingKey, string sHost, string sPassword)
        {
            if (m_Connection == null)
            {
                bool createdNew = false;
                var mutexSecurity = CreateMutex();

                using (Mutex mutex = new Mutex(false, string.Concat("Connection ", "Mutex"), out createdNew, mutexSecurity))
                {
                    try
                    {
                        mutex.WaitOne(-1);
                        if (m_Connection == null)
                        {
                            var factory = new ConnectionFactory() { HostName = sHost, Password = sPassword };
                            this.m_Connection = factory.CreateConnection();
                            this.m_Model = this.m_Connection.CreateModel();
                            //Dictionary<string, int> dict = new Dictionary<string, int>() { { "x-message-ttl", 60000 } };
                            m_Model.QueueDeclare(sQueueName, false, false, false, null);
                            m_Model.QueueBind(sQueueName, sExchange, sRoutingKey);
                        }
                    }
                    catch
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

            if (m_Model != null && m_Connection != null)
            {                
                var body = Encoding.UTF8.GetBytes(sMessage.ToString());
                IBasicProperties properties = m_Model.CreateBasicProperties();                                
                m_Model.BasicPublish(sExchange, sRoutingKey, properties, body);
            }
        }

        internal static MutexSecurity CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            MutexSecurity mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions, AccessControlType.Deny));
            mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.Delete, AccessControlType.Deny));

            return mutexSecurity;
        }

        #endregion

    }
}
