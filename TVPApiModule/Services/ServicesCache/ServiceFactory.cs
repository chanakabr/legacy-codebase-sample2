using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TVPApiModule.Context;
using TVPApiModule.Objects;

namespace TVPApiModule.Services
{
    public static class ServiceFactory
    {        

        public static BaseService GetService(int groupId, PlatformType platform, string url, eService serviceType)
        {
            BaseService service = null;

            switch (serviceType)
            {
                case eService.ApiService:
                    service = new ApiApiService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API() { Url = url } };
                    break;
                case eService.BillingService:
                    service = new ApiBillingService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module() { Url = url } };
                    break;
                case eService.ConditionalAccessService:
                    service = new ApiConditionalAccessService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module() { Url = url } };
                    break;
                case eService.DomainsService:
                    service = new ApiDomainsService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module() { Url = url } };
                    break;
                case eService.NotificationService:
                    service = new ApiNotificationService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Notification.NotificationServiceClient(
                        new BasicHttpBinding() { OpenTimeout = new TimeSpan(0, 10, 0), 
                                              ReceiveTimeout = new TimeSpan(0, 10, 0), 
                                              MaxReceivedMessageSize = 2147483647,
                                              MessageEncoding = WSMessageEncoding.Text }, new EndpointAddress(url))
                    };
                    break;
                case eService.PricingService:
                    service = new ApiPricingService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule() { Url = url } };
                    break;
                case eService.SocialService:
                    service = new ApiSocialService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Social.module() { Url = url } };
                    break;
                case eService.UsersService:
                    service = new ApiUsersService() { m_Module = new TVPPro.SiteManager.TvinciPlatform.Users.UsersService() { Url = url } };
                    break;
                default:
                    break;
            }

            //WSHttpBinding bind = new WSHttpBinding();
            

            if (service != null)
            {
                service.m_groupID = groupId;
                service.m_platform = platform;
            }

            //System.Web.HttpContext.Current.Items.Add("m_wsUserName"

            return service;
        }
    }
}
