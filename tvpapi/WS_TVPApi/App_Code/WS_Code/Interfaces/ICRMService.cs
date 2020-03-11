using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPPro.SiteManager.TvinciPlatform.Billing;
using TVPApiModule.Objects;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.TvinciPlatform.Users;

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
