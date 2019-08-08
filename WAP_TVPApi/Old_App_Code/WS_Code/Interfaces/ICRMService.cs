using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;

namespace TVPApiServices
{
    public interface ICRMService
    {
        DummyChargeUserForSubscriptionResponse DummyChargeUserForSubscription(DummyChargeUserForSubscriptionRequest request);

        DummyChargeUserForMediaFileResponse DummyChargeUserForMediaFile(DummyChargeUserForMediaFileRequest request);
        
        GetUserByUsernameResponse GetUserByUsername(GetUserByUsernameRequest request);

        SearchUsersResponse SearchUsers(SearchUsersRequest request);
    }
}
