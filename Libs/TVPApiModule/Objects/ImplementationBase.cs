using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Interfaces;
using TVPApiModule.Services;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.DataEntities;
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
        
        public virtual Services.ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword, 
                                                                         System.Collections.Specialized.NameValueCollection nameValueCollection = null)
        {
            bool isSingleLogin = TVPApi.ConfigManager.GetInstance()
                                       .GetConfig(_nGroupID, _initObj.Platform)
                                       .SiteConfiguration.Data.Features.SingleLogin.SupportFeature;

            return new TVPApiModule.Services.ApiUsersService(_nGroupID, _initObj.Platform).SignIn(sUsername, sPassword, _initObj.UDID, string.Empty, isSingleLogin, nameValueCollection);
        }
        
        public virtual DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            return new TVPApiModule.Services.ApiDomainsService(_nGroupID, _initObj.Platform).AddDeviceToDomain(_initObj.DomainID, _initObj.UDID, sDeviceName, nDeviceBrandID);

        }

        public virtual DomainResponseObject RemoveDeviceToDomain()
        {
            return new TVPApiModule.Services.ApiDomainsService(_nGroupID, _initObj.Platform).RemoveDeviceToDomain(_initObj.DomainID, _initObj.UDID);

        }

        public virtual string MediaHit(int nMediaID, int nFileID, string sNPVRID, int nLocationID, long programId, bool isReportingMode = false)
        {
            return ActionHelper.MediaHit(_initObj, _nGroupID, _initObj.Platform, nMediaID, nFileID, nLocationID, sNPVRID, programId, isReportingMode);
        }
        
        public virtual string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams, 
                                                        string sPaymentMethodID, string sEncryptedCVV)
        {
            return new ApiConditionalAccessService(_nGroupID, _initObj.Platform).ChargeUserForSubscription(dPrice, sCurrency, sSubscriptionID, sCouponCode, sIP, 
                                                   _initObj.SiteGuid, sExtraParams, _initObj.UDID, sPaymentMethodID, sEncryptedCVV);
        }

        public virtual string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, string sNPVRID, int nLocationID, long programId, bool isReportingMode = false)
        {
            return ActionHelper.MediaMark(_initObj, _nGroupID, _initObj.Platform, eAction, nLocationID, sNPVRID, programId, nMediaID, nFileID, isReportingMode);
        }

        public virtual bool IsItemPurchased(int iFileID, string sUserGuid)
        {
            bool bRet = false;

            MediaFileItemPricesContainer[] prices = new ApiConditionalAccessService(_nGroupID, _initObj.Platform).GetItemsPrice(new int[] { iFileID }, sUserGuid, true);

            MediaFileItemPricesContainer mediaPrice = null;
            foreach (MediaFileItemPricesContainer mp in prices)
            {
                if (mp.m_nMediaFileID == iFileID)
                {
                    mediaPrice = mp;
                    break;
                }
            }

            if (mediaPrice != null && mediaPrice.m_oItemPrices != null && mediaPrice.m_oItemPrices.Length > 0)
            {
                TVPApi.PriceReason priceReason = (TVPApi.PriceReason)mediaPrice.m_oItemPrices[0].m_PriceReason;

                bRet = mediaPrice.m_oItemPrices[0].m_oPrice.m_dPrice == 0 &&
                       (priceReason == TVPApi.PriceReason.PPVPurchased ||
                        priceReason == TVPApi.PriceReason.SubscriptionPurchased ||
                        priceReason == TVPApi.PriceReason.PrePaidPurchased ||
                        priceReason == TVPApi.PriceReason.Free);
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

        public virtual TVPApiModule.Helper.OrcaResponse GetRecommendedMediasByGallery(InitializationObject initObj, int groupID, int mediaID, string picSize, int maxParentalLevel, eGalleryType galleryType, string coGuid)
        {
            return null;
        }

        public virtual string GetMediaLicenseLink(InitializationObject initObj, int groupId, int mediaFileID, string baseLink, string clientIP)
        {
            return new ApiConditionalAccessService(groupId, initObj.Platform).GetMediaLicenseLink(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
        }

        public virtual TVPApiModule.yes.tvinci.ITProxy.RecordAllResult RecordAll(string accountNumber, string channelCode, string recordDate, string recordTime, string versionId, string serialNumber)
        {
            return null;
        }

        public virtual TVPApiModule.yes.tvinci.ITProxy.STBData[] GetMemirDetails(string accountNumber, string serviceAddressId)
        {
            return null;
        }

        public virtual UserResponse SetUserDynamicData(InitializationObject initObj, int groupID, string key, string value)
        {
            UserResponse retVal = null;
            if (new ApiUsersService(groupID, initObj.Platform).SetUserDynamicData(initObj.SiteGuid, key, value))
            {
                retVal = new UserResponse()
                {
                    ResponseStatus = TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK,
                    Message = TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.OK.ToString()
                };
            }
            else
            {
                retVal = new UserResponse()
                {
                    ResponseStatus = TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.InternalError,
                    Message = TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus.InternalError.ToString()
                };
            }
            return retVal;
        }
    }
}
