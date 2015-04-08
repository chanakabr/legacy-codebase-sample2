using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace RestfulTVPApi.Clients.Utils
{
    public class ClientFactory
    {
        public static BaseClient GetService(string url, RestfulTVPApi.Objects.Enums.Client clientType, string serviceUrl)
        {
            BaseClient client = null;

            switch (clientType)
            {
                case RestfulTVPApi.Objects.Enums.Client.Api:
                    client = new ApiClient() { Module = new TVPPro.SiteManager.TvinciPlatform.api.API() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Billing:
                    client = new BillingClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.ConditionalAccess:
                    client = new ConditionalAccessClient() { Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Domains:
                    client = new DomainsClient() { Module = new Domains.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Pricing:
                    client = new PricingClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Social:
                    client = new SocialClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Social.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Users:
                    client = new UsersClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Notification:
                    client = new NotificationsClient()
                    {
                        Module = new RestfulTVPApi.Notification.NotificationServiceClient(
                            new BasicHttpBinding()
                            {
                                OpenTimeout = new TimeSpan(0, 10, 0),
                                ReceiveTimeout = new TimeSpan(0, 10, 0),
                                MaxReceivedMessageSize = 2147483647,
                                MessageEncoding = WSMessageEncoding.Text
                            }, new EndpointAddress(url))
                    };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Catalog:
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
                client.ServiceKey = serviceUrl;
                client.ClientType = clientType;
            }

            return client;
        }
    }
}