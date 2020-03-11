using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Tvinci.MultiClient.Configuration;
using System.Threading;
using Tvinci.Configuration;
using System.Configuration;
using TVinciShared;

namespace Tvinci.MultiClient
{
    public  class MultiClientHelper
    {
        public class UserContext
        {
            public string ConfigurationIdentifier { get; set; }
            public string ClientIdentifier { get; set; }

			public UserContext()
			{
				ConfigurationIdentifier = string.Empty;
				ClientIdentifier = string.Empty;
			}									
        }

        public static MultiClientHelper Instance = new MultiClientHelper();
        private ConfigurationManager<MultiClientData> m_configuration = new ConfigurationManager<MultiClientData>();
        private MultiClientHelper()
        {
            string configPath = System.Configuration.ConfigurationManager.AppSettings["Tvinci.MultiClient.Configuration.Path"];

            if (string.IsNullOrEmpty(configPath))
            {
                MultiClientData data = new MultiClientData();

                data.Definitions.Mode = Mode.Prohibited;
                data.Definitions.Defaults.ConfigurationID = "Default";
                data.Definitions.ShowExitButton = false;

                m_configuration.SyncFromConfiguration(data);
            }
            else
            {
                m_configuration.SyncFromFile(configPath, true);
            }                                                
        }

        public delegate List<Client> ExtractClientsListDelegate();

        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();        
        List<Client> m_clients = new List<Client>();
        public ExtractClientsListDelegate ExtractClientsList { get; set; }

        public Client[] GetClientList()
        {
            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    Client[] result = new Client[m_clients.Count];
                    m_clients.CopyTo(result);

                    return result;
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }

            return null;
        }

        public bool IsClientActive(string clientToken)
        {
            if (string.IsNullOrEmpty(clientToken))
            {
                return false;
            }

            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    return (m_clients.Find(user => clientToken.ToString() == user.ClientID) != null);

                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }

            return false;
        }

        
        public void Sync()
        {
            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    if (Data.Definitions.Mode == Mode.Prohibited)
                    {
                        return;
                    }

                    m_clients = null;

                    if (ExtractClientsList != null)
                    {
                        m_clients = ExtractClientsList();
                    }

                    if (m_clients == null)
                    {
                        m_clients = new List<Client>();
                    }
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }
        }

        public MultiClientData Data
        {
            get
            {
                return m_configuration.Data;
            }
        }


        public bool TryGetClient(string identifier, out Client client)
        {            
            if (identifier == null) identifier = string.Empty;

            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {

                    client = m_clients.Find(user => identifier.ToLower() == user.ClientID.ToLower());
                    if (client != null)
                    {
                        return true;
                    }
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }

            client = null;
            return false;
        }

        public UserContext ActiveUserClient
        {
            get
            {

                UserContext result ;
                if (HttpContext.Current.Session != null)
                {
                    result = HttpContext.Current.Session.Get("TVP.MultiClient.UserClient") as UserContext;
                    if (result == null)
                    {
                        result = new UserContext();
                        result.ClientIdentifier = Data.Definitions.Defaults.ClientID;
                        result.ConfigurationIdentifier = Data.Definitions.Defaults.ConfigurationID;
                        HttpContext.Current.Session.Set("TVP.MultiClient.UserClient", result);
                    }
                }
                else
                {
                    result = new UserContext();
                    result.ClientIdentifier = Data.Definitions.Defaults.ClientID;
                    result.ConfigurationIdentifier = Data.Definitions.Defaults.ConfigurationID;
                }
                
                return result;                
            }            
        }
        
        internal bool ValidateConfigurationRestriction()
        {
            if (string.IsNullOrEmpty(ActiveUserClient.ConfigurationIdentifier))
            {
                ActiveUserClient.ConfigurationIdentifier = Data.Definitions.Defaults.ConfigurationID;
            }
            
            switch (Data.Definitions.Mode)
            {
                case Mode.Prohibited:
                    ActiveUserClient.ClientIdentifier = string.Empty;
                    break;
                case Mode.Required:
                    if (string.IsNullOrEmpty(ActiveUserClient.ClientIdentifier))
                    {
                        return false;
                    }
                    break;
                case Mode.ForceDefault:
                    if (string.IsNullOrEmpty(Data.Definitions.Defaults.ClientID))
                    {
                        throw new Exception("Default client was not assigned. Cannot force default");
                    }

                    ActiveUserClient.ClientIdentifier = Data.Definitions.Defaults.ClientID;
                    break;
                default:
                    break;
            }

            return true;
        }

        //public bool TryLoginClient(string clientID)
        //{
        //    if (string.IsNullOrEmpty(clientID))
        //    {
        //        return false;
        //    }

        //    Client client;
        //    if (TryGetClient(clientID, out client))
        //    {
        //        ActiveUserClient.ClientIdentifier = client.ClientID;

        //        if (!string.IsNullOrEmpty(client.ConfigurationID))
        //        {
        //            ActiveUserClient.ConfigurationIdentifier = client.ConfigurationID;
        //        }

        //        SetCookieToClient();
        //        return true;
        //    }

        //    return false;
        //}

        const string cookieKeyToken = "Tvinci.TVP";

        //internal void SetCookieToClient()
        //{
        //    if (!Data.Definitions.ClientPersistByCookie)
        //    {
        //        return;
        //    }

        //    string clientID = MultiClientHelper.Instance.ActiveUserClient.ClientIdentifier;

        //    if (string.IsNullOrEmpty(clientID))
        //    {
        //        RemoveCookieOfClient();
        //    }
        //    else
        //    {
        //        HttpCookie cookie = new HttpCookie(cookieKeyToken);
        //        cookie.Values.Add("ClientID", clientID);
        //        cookie.Expires = DateTime.Now.AddYears(1);
        //        HttpContext.Current.Response.Cookies.Add(cookie);
        //    }

        //}

        //internal void RemoveCookieOfClient()
        //{
        //    if (HttpContext.Current.Request.Cookies[cookieKeyToken] != null)
        //    {
        //        HttpContext.Current.Response.Cookies[cookieKeyToken].Expires = DateTime.Now.AddDays(-1);
        //    }
        ////}

        //internal string GetClientIDFromCookie()
        //{
        //    if (!Data.Definitions.ClientPersistByCookie)
        //    {
        //        return string.Empty;
        //    }

        //    if (HttpContext.Current.Request.Cookies[cookieKeyToken] != null)
        //    {
        //        return HttpContext.Current.Request.Cookies[cookieKeyToken]["ClientID"];
        //    }

        //    return string.Empty;
        //}

        //public void RemoveCurrentClient()
        //{
        //    ActiveUserClient.ClientIdentifier = string.Empty;
        //    RemoveCookieOfClient();
        //}
    }
}
