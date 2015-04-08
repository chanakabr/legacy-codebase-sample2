using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RestfulTVPApi.ServiceInterface
{
    public class NotificationsRepository : INotificationsRepository
    {
        public bool SubscribeByTag(SubscribeByTagRequest request)
        {
            bool bRes = false;

            bRes = ClientsManager.NotificationClient().SubscribeByTag(request.site_guid, request.tags);                          

            return bRes;
        }

        public bool UnsubscribeFollowUpByTag(UnSubscribeByTagRequest request)
        {
            bool bRes = false;

            bRes = ClientsManager.NotificationClient().UnsubscribeFollowUpByTag(request.site_guid, request.tags);
            
            return bRes;
        }

    }
}