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
        public bool SubscribeByTag(InitializationObject initObj, string sSiteGUID, List<TagMetaPairArray> tags)
        {
            bool bRes = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SubscribeByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                bRes = ServicesManager.NotificationService(groupId, initObj.Platform).SubscribeByTag(initObj.SiteGuid, tags);              
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRes;
        }

        public bool UnsubscribeFollowUpByTag(InitializationObject initObj, string sSiteGUID, List<TagMetaPairArray> tags)
        {
            bool bRes = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UnsubscribeFollowUpByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                bRes = ServicesManager.NotificationService(groupId, initObj.Platform).UnsubscribeFollowUpByTag(sSiteGUID, tags);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRes;
        }

    }
}