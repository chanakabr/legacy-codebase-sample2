using ServiceStack.Net30.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients.ClientsCache
{
    public class ClientsManager
    {
        #region Members

        private ConcurrentDictionary<string, BaseClient> clients;
        private char SPLITTER = '.';
        private readonly int failOverLimit;


        //private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region CTOR

        private ClientsManager()
        {
            clients = new ConcurrentDictionary<string, BaseClient>();
            try
            {
                failOverLimit = TCMClient.Settings.Instance.GetValue<int>("FailOverLimit");
            }
            catch
            {
                failOverLimit = 5;
            }
        }

        #endregion

        #region Singleton

        public static ClientsManager Instance
        {
            get { return Nested.Instance; }
        }

        public int FailOverLimit
        {
            get { return failOverLimit; }
        }
        
        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly ClientsManager Instance = new ClientsManager();
        }

        public static ApiClient ApiService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Api) as ApiClient;
        }

        public static BillingClient BillingService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Billing) as BillingClient;
        }

        public static ConditionalAccessClient ConditionalAccessService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.ConditionalAccess) as ConditionalAccessClient;
        }

        public static DomainsClient DomainsService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Domains) as DomainsClient;
        }

        //public static ApiNotificationService NotificationService(int groupId, PlatformType platform)
        //{
        //    return Nested.Instance.GetService(groupId, platform, eService.NotificationService) as ApiNotificationService;
        //}

        public static PricingClient PricingService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Pricing) as PricingClient;
        }

        public static SocialClient SocialService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Social) as SocialClient;
        }

        public static UsersClient UsersService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            return Nested.Instance.GetService(groupId, platform, RestfulTVPApi.Objects.Enums.Client.Users) as UsersClient;
        }

        private BaseClient GetService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform, RestfulTVPApi.Objects.Enums.Client serviceType)
        {
            /////// Implement GetInstance Logic /////
            string serviceTcmConfigurationKey = string.Format("WebServices{0}{1}", SPLITTER, serviceType);
            string serviceUrl = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "URL"));

            if (!string.IsNullOrEmpty(serviceUrl) && !clients.ContainsKey(serviceUrl))
            {
                BaseClient serviceInserted = ClientFactory.GetService(groupId, platform, serviceUrl, serviceType, serviceUrl);

                if (serviceInserted != null)
                {
                    clients.TryAdd(serviceUrl, serviceInserted);
                }
            }

            BaseClient service = null;
            clients.TryGetValue(serviceUrl, out service);

            if (service != null)
            {
                if (HttpContext.Current.Items != null && !HttpContext.Current.Items.Contains("m_wsUserName") && !HttpContext.Current.Items.Contains("m_wsPassword"))
                {
                    string serviceUser = string.Format("{0}{1}", TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "USER")), groupId);
                    string servicePass = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}{1}{2}", serviceTcmConfigurationKey, SPLITTER, "PASSWORD"));
                    if (!string.IsNullOrEmpty(serviceUser) && !string.IsNullOrEmpty(servicePass))
                    {
                        HttpContext.Current.Items.Add("m_wsUserName", serviceUser);
                        HttpContext.Current.Items.Add("m_wsPassword", servicePass);
                    }
                }
            }

            return service;            
        }

        #endregion

        public BaseClient ResetService(BaseClient service)
        {
            BaseClient removedService = null;
            
            if (service != null && !string.IsNullOrEmpty(service.ServiceKey))
            {
                clients.TryRemove(service.ServiceKey, out removedService);                
            }

            return removedService;
        }

        public BaseClient RestartService(BaseClient service)
        {
            BaseClient removedService = ResetService(service);
            BaseClient insertedService = null;
            
            if (removedService != null)
            {
                insertedService = GetService(service.GroupID, service.Platform, service.ServiceType);
            }

            return insertedService;
        }
    }
}