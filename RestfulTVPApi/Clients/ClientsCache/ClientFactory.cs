using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients.ClientsCache
{
    public class ClientFactory
    {
        public static BaseClient GetService(int groupId, RestfulTVPApi.Objects.Enums.PlatformType platform, string url, RestfulTVPApi.Objects.Enums.Client serviceType, string serviceUrl)
        {
            BaseClient service = null;

            switch (serviceType)
            {
                case RestfulTVPApi.Objects.Enums.Client.Api:
                    service = new ApiClient() { Module = new TVPPro.SiteManager.TvinciPlatform.api.API() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Billing:
                    service = new BillingClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.ConditionalAccess:
                    service = new ConditionalAccessClient() { Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Domains:
                    service = new DomainsClient() { Module = new Domains.module() { Url = url } };
                    break;
                //case eService.NotificationService:
                //    service = new ApiNotificationService()
                //    {
                //        m_Module = new TVPPro.SiteManager.TvinciPlatform.Notification.NotificationServiceClient(
                //            new BasicHttpBinding()
                //            {
                //                OpenTimeout = new TimeSpan(0, 10, 0),
                //                ReceiveTimeout = new TimeSpan(0, 10, 0),
                //                MaxReceivedMessageSize = 2147483647,
                //                MessageEncoding = WSMessageEncoding.Text
                //            }, new EndpointAddress(url))
                //    };
                //    break;
                case RestfulTVPApi.Objects.Enums.Client.Pricing:
                    service = new PricingClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Social:
                    service = new SocialClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Social.module() { Url = url } };
                    break;
                case RestfulTVPApi.Objects.Enums.Client.Users:
                    service = new UsersClient() { Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService() { Url = url } };
                    break;
                default:
                    break;
            }

            if (service != null)
            {
                service.GroupID = groupId;
                service.Platform = platform;
                service.ServiceKey = serviceUrl;
                service.ServiceType = serviceType;
            }

            return service;
        }
    }
}