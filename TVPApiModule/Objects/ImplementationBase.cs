using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Interfaces;
using TVPApiModule.Manager;
using TVPApiModule.Services;
using TVPApiModule.Context;
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
            bool isSingleLogin = ConfigManager.GetInstance()
                                       .GetConfig(_nGroupID, _initObj.Platform)
                                       .SiteConfiguration.Data.Features.SingleLogin.SupportFeature;

            return new TVPApiModule.Services.ApiUsersService(_nGroupID, _initObj.Platform).SignIn(sUsername, sPassword, _initObj.UDID, string.Empty, isSingleLogin);
        }

        
        public virtual TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            return new TVPApiModule.Services.ApiDomainsService(_nGroupID, _initObj.Platform).AddDeviceToDomain(_initObj.DomainID, _initObj.UDID, sDeviceName, nDeviceBrandID);

        }

        public virtual TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceToDomain()
        {
            return new TVPApiModule.Services.ApiDomainsService(_nGroupID, _initObj.Platform).RemoveDeviceToDomain(_initObj.DomainID, _initObj.UDID);

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

        public virtual bool IsItemPurchased(int iFileID, string sUserGuid)
        {
            bool bRet = false;

            IEnumerable<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> prices = new ApiConditionalAccessService(_nGroupID, _initObj.Platform).GetItemsPrice(new int[] { iFileID }, sUserGuid, true);

            TVPApiModule.Objects.Responses.MediaFileItemPricesContainer mediaPrice = null;
            foreach (TVPApiModule.Objects.Responses.MediaFileItemPricesContainer mp in prices)
            {
                if (mp.media_file_id == iFileID)
                {
                    mediaPrice = mp;
                    break;
                }
            }

            if (mediaPrice != null && mediaPrice.item_prices != null && mediaPrice.item_prices.Length > 0)
            {
                TVPApiModule.Context.PriceReason priceReason = (TVPApiModule.Context.PriceReason)mediaPrice.item_prices[0].price_reason;

                bRet = mediaPrice.item_prices[0].price.price == 0 &&
                       (priceReason == TVPApiModule.Context.PriceReason.PPVPurchased ||
                        priceReason == TVPApiModule.Context.PriceReason.SubscriptionPurchased ||
                        priceReason == TVPApiModule.Context.PriceReason.PrePaidPurchased ||
                        priceReason == TVPApiModule.Context.PriceReason.Free);
            }
            else
            {
                bRet = true;
            }

            return bRet;
        }

        public virtual string GetMediaLicenseData(int iMediaFileID, int iMediaID)
        {
            string sRet = string.Empty;

            return sRet;
        }
    }
}
