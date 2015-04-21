using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceInterface
{
    public interface INotificationsRepository
    {
        bool SubscribeByTag(SubscribeByTagRequest request);

        bool UnsubscribeFollowUpByTag(UnSubscribeByTagRequest request);
    }
}