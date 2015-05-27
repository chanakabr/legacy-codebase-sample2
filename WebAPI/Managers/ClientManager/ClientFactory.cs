using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using WebAPI.Clients;
using WebAPI.Users;

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
                    client = new ApiClient() { Module = new Api.API() { Url = url } };
                    break;
                case ClientType.Billing:
                    client = new BillingClient() { Module = new Billing.module() { Url = url } };
                    break;
                case ClientType.ConditionalAccess:
                    client = new ConditionalAccessClient() { Module = new ConditionalAccess.module() { Url = url } };
                    break;
                case ClientType.Domains:
                    client = new DomainsClient() { Module = new Domains.module() { Url = url } };
                    break;
                case ClientType.Pricing:
                    client = new PricingClient() { Module = new Pricing.mdoule() { Url = url } };
                    break;
                case ClientType.Social:
                    client = new SocialClient() { Module = new Social.module() { Url = url } };
                    break;
                case ClientType.Users:
                    client = new UsersClient() { Module = new UsersService() { Url = url } };
                    break;
                case ClientType.Notification:
                    client = new NotificationsClient()
                    {
                        Module = new WebAPI.Notifications.NotificationServiceClient(
                            new BasicHttpBinding()
                            {
                                OpenTimeout = new TimeSpan(0, 10, 0),
                                ReceiveTimeout = new TimeSpan(0, 10, 0),
                                MaxReceivedMessageSize = 2147483647,
                                MessageEncoding = WSMessageEncoding.Text
                            }, new EndpointAddress(url))
                    };
                    break;
                case ClientType.Catalog:
                    client = new CatalogClient()
                    {
                        Module = new Catalog.IserviceClient(string.Empty, url)
                    };
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