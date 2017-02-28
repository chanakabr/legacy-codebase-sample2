using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;
using WebAPI.Clients;

namespace WebAPI.ClientManagers.Client
{
    public class ClientFactory
    {
        public static BaseClient GetService(string url, ClientType clientType)
        {
            BaseClient client = null;

            switch (clientType)
            {
                case ClientType.Api:
                    client = new ApiClient();
                    break;
                case ClientType.Billing:
                    client = new BillingClient();
                    break;
                case ClientType.ConditionalAccess:
                    client = new ConditionalAccessClient();
                    break;
                case ClientType.Domains:
                    client = new DomainsClient();
                    break;
                case ClientType.Pricing:
                    client = new PricingClient();
                    break;
                case ClientType.Social:
                    client = new SocialClient();
                    break;
                case ClientType.Users:
                    client = new UsersClient();
                    break;
                case ClientType.Notification:
                    client = new NotificationsClient();
                    break;
                case ClientType.Catalog:
                    client = new CatalogClient();
                    break;
                default:
                    break;
            }

            if (client != null)
            {
                client.ClientType = clientType;
            }

            return client;
        }
    }
}