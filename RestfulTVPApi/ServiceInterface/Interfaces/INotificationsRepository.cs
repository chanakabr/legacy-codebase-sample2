using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace RestfulTVPApi.ServiceInterface
{
    public interface INotificationsRepository
    {
        bool SubscribeByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags);

        bool UnsubscribeFollowUpByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags);
    }
}