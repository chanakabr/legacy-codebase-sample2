using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace RestfulTVPApi.ServiceInterface
{
    public class NotificationsRepository : INotificationsRepository
    {
        public bool SubscribeByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags)
        {
            bool bRes = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SubscribeByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);

                bRes = service.SubscribeByTag(initObj.SiteGuid, tags);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRes;
        }

        public bool UnsubscribeFollowUpByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags)
        {
            bool bRes = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "UnsubscribeFollowUpByTag", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiNotificationService service = new ApiNotificationService(groupId, initObj.Platform);

                bRes = service.UnsubscribeFollowUpByTag(initObj.SiteGuid, tags);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return bRes;
        }

    }
}