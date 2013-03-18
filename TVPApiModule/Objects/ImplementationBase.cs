using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiModule.Objects
{
    public class ImplementationBase : IImplementation
    {
        protected InitializationObject _initObj;
        protected int _nGroupID;


        public ImplementationBase(int nGroupID, InitializationObject initObj)
        {
            _nGroupID = nGroupID;
            _initObj = initObj;
        }


        public virtual Services.ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword)
        {
            bool isSingleLogin = TVPApi.ConfigManager.GetInstance()
                                       .GetConfig(_nGroupID, _initObj.Platform)
                                       .SiteConfiguration.Data.Features.SingleLogin.SupportFeature;

            return new TVPApiModule.Services.ApiUsersService(_nGroupID, _initObj.Platform).SignIn(sUsername, sPassword, _initObj.UDID, string.Empty, isSingleLogin);
        }

        
        public virtual DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            return new TVPApiModule.Services.ApiDomainsService(_nGroupID, _initObj.Platform).AddDeviceToDomain(_initObj.DomainID, _initObj.UDID, sDeviceName, nDeviceBrandID);

        }

        
        public virtual string MediaHit(int nMediaID, int nFileID, int nLocationID)
        {
            return ActionHelper.MediaHit(_initObj, _nGroupID, _initObj.Platform, nMediaID, nFileID, nLocationID);
        }

        
        public virtual string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams)
        {
            return new ApiConditionalAccessService(_nGroupID, _initObj.Platform).ChargeUserForSubscription(dPrice, sCurrency, sSubscriptionID, sCouponCode, sIP, _initObj.SiteGuid, sExtraParams, _initObj.UDID);
        }

        public virtual string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, int nLocationID)
        {
            return ActionHelper.MediaMark(_initObj, _nGroupID, _initObj.Platform, eAction, nMediaType, nMediaID, nFileID, nLocationID);
        }
    }
}
