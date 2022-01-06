using ApiObjects;
using Core.Users;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Objects.ORCARecommendations;
using TVPApiModule.Services;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using InitializationObject = TVPApi.InitializationObject;

namespace TVPApiModule.Objects
{
    public class ImplementationYes : ImplementationBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public ImplementationYes(int nGroupID, InitializationObject initObj)
            : base(nGroupID, initObj)
        {

        }

        public override DomainResponseObject AddDeviceToDomain(string sDeviceName, int nDeviceBrandID)
        {
            return base.AddDeviceToDomain(sDeviceName, nDeviceBrandID);
        }

        public override string ChargeUserForSubscription(double dPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sIP, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            return base.ChargeUserForSubscription(dPrice, sCurrency, sSubscriptionID, sCouponCode, sIP, sExtraParams, sPaymentMethodID, sEncryptedCVV);
        }

        public override string GetMediaLicenseData(int iMediaFileID, int iMediaID)
        {
            return base.GetMediaLicenseData(iMediaFileID, iMediaID);
        }

        public override string GetMediaLicenseLink(InitializationObject initObj, int groupId, int mediaFileID, string baseLink, string clientIP)
        {
            return base.GetMediaLicenseLink(initObj, groupId, mediaFileID, baseLink, clientIP);
        }

        public override OrcaResponse GetRecommendedMediasByGallery(InitializationObject initObj, int groupID, int mediaID, string picSize, int maxParentalLevel, eGalleryType galleryType, string coGuid)
        {
            return base.GetRecommendedMediasByGallery(initObj, groupID, mediaID, picSize, maxParentalLevel, galleryType, coGuid);
        }

        public override bool IsItemPurchased(int iFileID, string sUserGuid)
        {
            return base.IsItemPurchased(iFileID, sUserGuid);
        }

        public override string MediaHit(int nMediaID, int nFileID, string sNPVRID, int nLocationID, long programId, bool isReportingMode = false)
        {
            return base.MediaHit(nMediaID, nFileID, sNPVRID, nLocationID, programId, isReportingMode);
        }

        public override string MediaMark(action eAction, int nMediaType, int nMediaID, int nFileID, string sNPVRID, int nLocationID, long programId, bool isReportingMode = false)
        {
            return base.MediaMark(eAction, nMediaType, nMediaID, nFileID, sNPVRID, nLocationID, programId, isReportingMode);
        }

        public override DomainResponseObject RemoveDeviceToDomain()
        {
            return base.RemoveDeviceToDomain();
        }

        public override UserResponse SetUserDynamicData(InitializationObject initObj, int groupID, string key, string value)
        {
            return base.SetUserDynamicData(initObj, groupID, key, value);
        }

        public override ApiUsersService.LogInResponseData SignIn(string sUsername, string sPassword, NameValueCollection nameValueCollection = null)
        {
            return base.SignIn(sUsername, sPassword, nameValueCollection);
        }
    }
}
