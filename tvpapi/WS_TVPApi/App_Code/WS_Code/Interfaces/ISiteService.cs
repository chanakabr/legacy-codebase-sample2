using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Authorization;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService" here, you must also update the reference to "IService" in Web.config.
    [ServiceContract]
    public interface ISiteService
    {
        #region SiteMap
        [OperationContract]
        TVPApi.SiteMap GetSiteMap(InitializationObject initObj);
        #endregion

        [OperationContract]
        TVPApi.PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter);

        [OperationContract]
        TVPApi.PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter);

        [OperationContract]
        Menu GetMenu(InitializationObject initObj, long ID);

        [OperationContract]
        Menu GetFooter(InitializationObject initObj, long ID);

        [OperationContract]
        Profile GetBottomProfile(InitializationObject initObj, long ID);

        [OperationContract]
        List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, long PageID, int pageSize, int start_index);

        [OperationContract]
        PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID);

        [OperationContract]
        DoSocialActionResponse DoSocialAction(InitializationObject initObj, int mediaID, eUserAction socialAction, SocialPlatform socialPlatform, string actionParam);

        [OperationContract]
        bool IsFacebookUser(InitializationObject initObj);

        [OperationContract]
        string GetSiteGuid(InitializationObject initObj, string userName, string password);

        [OperationContract]
        string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid);

        [OperationContract]
        string PostRegAction(InitializationObject initObj, string actionName);

        [OperationContract]
        bool SendNewPassword(InitializationObject initObj, string sUserName);

        [OperationContract]
        TVPApiModule.Services.ApiUsersService.LogInResponseData SSOSignIn(InitializationObject initObj, string userName, string password, int providerID);

        [OperationContract]
        bool SetUserGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive, string siteGuid);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupRules(InitializationObject initObj);

        [OperationContract]
        bool CheckParentalPIN(InitializationObject initObj, int ruleID, string PIN);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID);

        [OperationContract]
        bool SetRuleState(InitializationObject initObj, int ruleID, int isActive, string siteGuid);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetDomainGroupRules(InitializationObject initObj);

        [OperationContract]
        bool SetDomainGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetEPGProgramRules(InitializationObject initObj, int MediaId, int programId, string IP);

        [OperationContract]
        string[] GetUserStartedWatchingMedias(InitializationObject initObj, int numOfItems);

        [OperationContract]
        bool CleanUserHistory(InitializationObject initObj, int[] mediaIDs);

        [OperationContract]
        TVPApiModule.yes.tvinci.ITProxy.RecordAllResult RecordAll(InitializationObject initObj, string accountNumber, string channelCode, string recordDate, string recordTime, string versionId, string serialNumber);

        [OperationContract]
        TVPApiModule.yes.tvinci.ITProxy.STBData[] GetAccountSTBs(InitializationObject initObj, string accountNumber, string serviceAddressId);

        //[OperationContract]
        //string GenerateDeviceToken(InitializationObject initObj, string appId);

        //[OperationContract]
        //object ExchangeDeviceToken(InitializationObject initObj, string appId, string appSecret, string deviceToken);

        [OperationContract]
        object RefreshAccessToken(InitializationObject initObj, string refreshToken);

        [OperationContract]
        TVPApiModule.Objects.Responses.RegionsResponse GetRegions(InitializationObject initObj, string[] externalRegionIds);

        [OperationContract]
        TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalRules(InitializationObject initObj);

        [OperationContract]
        TVPApiModule.Objects.Responses.ParentalRulesResponse GetDomainParentalRules(InitializationObject initObj);

        [OperationContract]
        TVPApiModule.Objects.Responses.ParentalRulesResponse GetUserParentalRules(InitializationObject initObj, string siteGuid);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.Status SetUserParentalRules(InitializationObject initObj, string siteGuid, long ruleId, int isActive);
        
        [OperationContract]
        TVPApiModule.Objects.Responses.Status SetDomainParentalRules(InitializationObject initObj, long ruleId, int isActive);

        [OperationContract]
        TVPApiModule.Objects.Responses.PinResponse GetParentalPIN(InitializationObject initObj, string siteGuid, int? ruleId);

        [OperationContract]
        TVPApiModule.Objects.Responses.Status SetParentalPIN(InitializationObject initObj, string siteGuid, string pin, int? ruleId);

        [OperationContract]
        TVPApiModule.Objects.Responses.PurchaseSettingsResponse GetPurchaseSettings(InitializationObject initObj, string siteGuid);

        [OperationContract]
        TVPApiModule.Objects.Responses.Status SetPurchaseSettings(InitializationObject initObj, string siteGuid, int setting);

        [OperationContract]
        TVPApiModule.Objects.Responses.PurchaseSettingsResponse GetPurchasePIN(InitializationObject initObj, string siteGuid);

        [OperationContract]
        TVPApiModule.Objects.Responses.Status SetPurchasePIN(InitializationObject initObj, string siteGuid, string pin);

        [OperationContract]
        TVPApiModule.Objects.Responses.Status ValidateParentalPIN(InitializationObject initObj, string siteGuid, string pin, int? ruleId);

        [OperationContract]
        TVPApiModule.Objects.Responses.Status ValidatePurchasePIN(InitializationObject initObj, string siteGuid, string pin);

        [OperationContract]
        TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalMediaRules(InitializationObject initObj, string siteGuid, long mediaId);

        [OperationContract]
        TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalEPGRules(InitializationObject initObj, string siteGuid, long epgId);

        [OperationContract]
        object ChangeUser(InitializationObject initObj, string siteGuid);

        [OperationContract]
        GenericRulesResponse GetMediaRules(InitializationObject initObj, string siteGuid, long mediaId);

        [OperationContract]
        GenericRulesResponse GetEpgRules(InitializationObject initObj, string siteGuid, long epgId, long channelMediaId);
    }
}
