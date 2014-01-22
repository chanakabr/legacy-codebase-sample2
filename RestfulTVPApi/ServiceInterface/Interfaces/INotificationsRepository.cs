using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;

namespace RestfulTVPApi.ServiceInterface
{
    public interface INotificationsRepository
    {
        bool SubscribeByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags);

        bool UnsubscribeFollowUpByTag(InitializationObject initObj, string sSiteGUID, List<TVPApi.TagMetaPairArray> tags);
    }
}