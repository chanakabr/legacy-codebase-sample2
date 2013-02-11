using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Social;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPApiServices
{
    [ServiceContract]
    public interface IUsersService
    {
        [OperationContract]
        UserResponseObject ChangeUserPassword(InitializationObject initObj, string sUN, string sOldPass, string sPass);

        [OperationContract]
        UserResponseObject GetUserByFacebookID(InitializationObject initObj, string facebookId);

        [OperationContract]
        UserResponseObject GetUserByUsername(InitializationObject initObj, string userName);

        [OperationContract]
        void Logout(InitializationObject initObj, string sSiteGuid);
    }
}
