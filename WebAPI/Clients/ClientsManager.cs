using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Clients
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
            // not to mark type as beforefieldinit
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
                string serviceTcmConfigurationKey = string.Format("WebServices.{0}", clientType);
                string serviceUrl = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", serviceTcmConfigurationKey, "URL"));

                client = ClientFactory.GetService(serviceUrl, clientType);

                if (clientType == ClientType.Catalog)
                {
                    ((CatalogClient)client).SignatureKey = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", serviceTcmConfigurationKey, "SignatureKey"));
                }
                else
                {
                    client.WSUserName = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", serviceTcmConfigurationKey, "USER"));
                    client.WSPassword = TCMClient.Settings.Instance.GetValue<string>(string.Format("{0}.{1}", serviceTcmConfigurationKey, "PASSWORD"));
                }

                clients.TryAdd(clientType, client);
            }

            return client;
        }

        #endregion

        public BaseClient ResetClient(BaseClient client)
        {
            BaseClient removedClient = null;

            if (client != null && !string.IsNullOrEmpty(client.ServiceKey))
            {
                clients.TryRemove(client.ClientType, out removedClient);
            }

            return removedClient;
        }

        public BaseClient RestartClient(BaseClient client)
        {
            BaseClient removedClient = ResetClient(client);
            BaseClient insertedClient = null;

            if (removedClient != null)
            {
                insertedClient = GetClient(client.ClientType);
            }

            return insertedClient;
        }
    }
}