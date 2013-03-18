using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;

namespace TVPApiModule.Interfaces
{
    public interface IImplementation
    {
        ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword);
        DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID);
        string MediaHit(int nMediaID, int nFileID, int nLocationID);
        string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode,
                                            string sIP, string sExtraParams);

        string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, int nLocationID);
    }
}
