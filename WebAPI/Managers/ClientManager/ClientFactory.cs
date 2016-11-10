using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;
using WebAPI.Clients;
using WebAPI.Notifications;
using WS_API;
using WS_Users;

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
                    client = new ApiClient() { Module = new API() };
                    break;
                case ClientType.Billing:
                    client = new BillingClient() { Module = new WS_Billing.module() };
                    break;
                case ClientType.ConditionalAccess:
                    client = new ConditionalAccessClient() { Module = new WS_ConditionalAccess.module() };
                    break;
                case ClientType.Domains:
                    client = new DomainsClient() { Module = new WS_Domains.module() };
                    break;
                case ClientType.Pricing:
                    client = new PricingClient() { Module = new WS_Pricing.mdoule() };
                    break;
                case ClientType.Social:
                    client = new SocialClient() { Module = new WS_Social.module() };
                    break;
                case ClientType.Users:
                    client = new UsersClient() { Module = new UsersService() };
                    break;
                case ClientType.Notification:

                    client = new NotificationsClient()
                    {
                        Module = new WebAPI.Notifications.NotificationServiceClient("BasicHttpBinding_INotificationService")
                    };
                    ((NotificationServiceClient)client.Module).Endpoint.Address = new EndpointAddress(url);

                    break;
                case ClientType.Catalog:

                    client = new CatalogClient()
                    {
                        Module = new Catalog.IserviceClient(TCMClient.Settings.Instance.GetValue<string>("CatalogEndPoint"))
                    };
                    ((Catalog.IserviceClient)client.Module).Endpoint.Address = new EndpointAddress(url);

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