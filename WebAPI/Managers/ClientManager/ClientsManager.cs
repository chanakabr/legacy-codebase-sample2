using ConfigurationManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients;

namespace WebAPI.ClientManagers.Client
{
    public class ClientsManager
    {
        #region Members

        private ConcurrentDictionary<ClientType, BaseClient> clients;


        //private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region CTOR

        private ClientsManager()
        {
            clients = new ConcurrentDictionary<ClientType, BaseClient>();
        }

        #endregion

        #region Singleton

        public static ClientsManager Instance
        {
            get { return Nested.Instance; }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as before field init
            static Nested()
            {
            }

            internal static readonly ClientsManager Instance = new ClientsManager();
        }

        public static ApiClient ApiClient()
        {
            return Nested.Instance.GetClient(ClientType.Api) as ApiClient;
        }

        public static BillingClient BillingClient()
        {
            return Nested.Instance.GetClient(ClientType.Billing) as BillingClient;
        }

        public static ConditionalAccessClient ConditionalAccessClient()
        {
            return Nested.Instance.GetClient(ClientType.ConditionalAccess) as ConditionalAccessClient;
        }

        public static DomainsClient DomainsClient()
        {
            return Nested.Instance.GetClient(ClientType.Domains) as DomainsClient;
        }

        public static NotificationsClient NotificationClient()
        {
            return Nested.Instance.GetClient(ClientType.Notification) as NotificationsClient;
        }

        public static PricingClient PricingClient()
        {
            return Nested.Instance.GetClient(ClientType.Pricing) as PricingClient;
        }

        public static SocialClient SocialClient()
        {
            return Nested.Instance.GetClient(ClientType.Social) as SocialClient;
        }

        public static UsersClient UsersClient()
        {
            return Nested.Instance.GetClient(ClientType.Users) as UsersClient;
        }

        public static CatalogClient CatalogClient()
        {
            return Nested.Instance.GetClient(ClientType.Catalog) as CatalogClient;
        }

        private BaseClient GetClient(ClientType clientType)
        {
            BaseClient client = null;

            if (!clients.TryGetValue(clientType, out client) && client == null)
            {
                string serviceUrl = string.Empty;

                switch (clientType)
                {
                    case ClientType.Api:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Api.URL.Value;
                            break;
                        }
                    case ClientType.Billing:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Billing.URL.Value;
                            break;
                        }
                    case ClientType.ConditionalAccess:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.ConditionalAccess.URL.Value;
                            break;
                        }
                    case ClientType.Domains:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Domains.URL.Value;
                            break;
                        }
                    case ClientType.Notification:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Notification.URL.Value;
                            break;
                        }
                    case ClientType.Pricing:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Pricing.URL.Value;
                            break;
                        }
                    case ClientType.Social:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Social.URL.Value;
                            break;
                        }
                    case ClientType.Users:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Users.URL.Value;
                            break;
                        }
                    case ClientType.Catalog:
                        {
                            serviceUrl = ApplicationConfiguration.WebServicesConfiguration.Catalog.URL.Value;
                            break;
                        }
                    default:
                        break;
                }

                client = ClientFactory.GetService(serviceUrl, clientType);

                if (clientType == ClientType.Catalog)
                {
                    ((CatalogClient)client).SignatureKey = ApplicationConfiguration.WebServicesConfiguration.Catalog.SignatureKey.Value;
                    ((CatalogClient)client).CacheDuration = ApplicationConfiguration.WebServicesConfiguration.Catalog.CacheDurationSeconds.IntValue;
                }
                
                clients.TryAdd(clientType, client);
            }

            return client;
        }

        #endregion

       
    }
}