using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class NotificationsRepository : INotificationsRepository
    {
        public bool SubscribeByTag(SubscribeByTagRequest request)
        {
            bool bRes = false;

            bRes = ServicesManager.NotificationService(request.GroupID, request.InitObj.Platform).SubscribeByTag(request.site_guid, request.tags);                          

            return bRes;
        }

        public bool UnsubscribeFollowUpByTag(UnSubscribeByTagRequest request)
        {
            bool bRes = false;

            bRes = ServicesManager.NotificationService(request.GroupID, request.InitObj.Platform).UnsubscribeFollowUpByTag(request.site_guid, request.tags);
            
            return bRes;
        }

    }
}