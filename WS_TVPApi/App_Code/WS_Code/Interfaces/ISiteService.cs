using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace TVPApiServices
{
    // NOTE: If you change the interface name "IService" here, you must also update the reference to "IService" in Web.config.
    [ServiceContract]
    public interface ISiteService
    {
        #region SiteMap
        //[OperationContract]
        //TVPApi.SiteMap GetSiteMap(InitializationObject initObj);
        #endregion

        //[OperationContract]
        //TVPApi.PageContext GetPage(InitializationObject initObj, long ID, bool withMenu, bool withFooter);

        //[OperationContract]
        //TVPApi.PageContext GetPageByToken(InitializationObject initObj, Pages token, bool withMenu, bool withFooter);

        //[OperationContract]
        //Menu GetMenu(InitializationObject initObj, long ID);

        [OperationContract]
        Menu GetFooter(InitializationObject initObj, long ID);

        //[OperationContract]
        //Profile GetBottomProfile(InitializationObject initObj, long ID);

        //[OperationContract]
        //List<TVPApi.PageGallery> GetPageGalleries(InitializationObject initObj, long PageID, int pageSize, int start_index);

        [OperationContract]
        PageGallery GetGallery(InitializationObject initObj, long galleryID, long PageID);        

        [OperationContract]
        string DoSocialAction(InitializationObject initObj, string siteGuid, int mediaID, eUserAction socialAction, SocialPlatform socialPlatform, string actionParam);

        [OperationContract]
        bool IsFacebookUser(InitializationObject initObj, string siteGuid);

        [OperationContract]
        string GetSiteGuid(InitializationObject initObj, string userName, string password);

        [OperationContract]
        string GetSiteGuidFromSecured(InitializationObject initObj, string encSiteGuid);

        [OperationContract]
        string PostRegAction(InitializationObject initObj, string actionName);

        [OperationContract]
        bool SendNewPassword(InitializationObject initObj, string sUserName);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject SSOSignIn(InitializationObject initObj, string userName, string password, int providerID);

        [OperationContract]
        bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid);

        //[OperationContract]
        //TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupRules(InitializationObject initObj);

        [OperationContract]
        bool CheckParentalPIN(InitializationObject initObj, string siteGuid, int ruleID, string PIN);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject GetUserDataByCoGuid(InitializationObject initObj, string coGuid, int operatorID);

        [OperationContract]
        bool SetRuleState(InitializationObject initObj, int ruleID, int isActive);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetDomainGroupRules(InitializationObject initObj);

        [OperationContract]
        bool SetDomainGroupRule(InitializationObject initObj, int ruleID, string PIN, int isActive);

        [OperationContract]
        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetEPGProgramRules(InitializationObject initObj, int MediaId, int programId, string IP);

        [OperationContract]
        string[] GetUserStartedWatchingMedias(InitializationObject initObj, string siteGuid, int numOfItems);

        [OperationContract]
        bool CleanUserHistory(InitializationObject initObj, string siteGuid, int[] mediaIDs);
    }
}
