using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using KLogMonitor;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Social;
using WebAPI.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;
using WebAPI.Models.Users;
using System.Net;
using System.ServiceModel;
using WebAPI.Models.Catalog;
using ApiObjects;
using Social;
using Social.Requests;
using ObjectsConvertor.Mapping;
using ApiObjects.Social;
using WebAPI.Filters;

namespace WebAPI.Clients
{
    public class SocialClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public SocialClient()
        {
        }

        protected WS_Social.module Client
        {
            get
            {
                return (Module as WS_Social.module);
            }
        }

        internal KalturaFacebookSocial FBData(int groupId, string token)
        {
            FacebookResponse wsResponse = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserData(group.SocialCredentials.Username, group.SocialCredentials.Password, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            KalturaFacebookSocial clientResponse = AutoMapper.Mapper.Map<KalturaFacebookSocial>(wsResponse.ResponseData);

            return clientResponse;
        }

        [Obsolete]
        internal KalturaSocialResponse FBUserData(int groupId, string token)
        {
            FacebookResponse wsResponse = null;
            KalturaSocialResponse clientResponse = new KalturaSocialResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserData(group.SocialCredentials.Username, group.SocialCredentials.Password, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            clientResponse = AutoMapper.Mapper.Map<KalturaSocialResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaFacebookSocial FBUserDataByUserId(int groupId, string userId)
        {
            FacebookResponse wsResponse = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserDataByUserId(group.SocialCredentials.Username, group.SocialCredentials.Password, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            KalturaFacebookSocial clientResponse = AutoMapper.Mapper.Map<KalturaFacebookSocial>(wsResponse.ResponseData);
            return clientResponse;
        }

        internal KalturaFacebookSocial FBRegister(int groupId, string token, List<KeyValuePair> extraParameters, string ip)
        {
            {
                FacebookResponse wsResponse = null;

                // get group ID
                Group group = GroupsManager.GetGroup(groupId);

                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        // fire request
                        wsResponse = Client.FBUserRegister(group.SocialCredentials.Username, group.SocialCredentials.Password, token, extraParameters, ip);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                    ErrorUtils.HandleWSException(ex);
                }

                if (wsResponse == null)
                {
                    // general exception
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }

                if (wsResponse.Status.Code != (int)StatusCode.OK)
                {
                    // internal web service exception
                    throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
                }

                // convert response
                KalturaFacebookSocial clientResponse = AutoMapper.Mapper.Map<KalturaFacebookSocial>(wsResponse.ResponseData);

                return clientResponse;
            }
        }

        [Obsolete]
        internal KalturaSocialResponse FBUserRegister(int groupId, string token, List<KeyValuePair> extraParameters, string ip)
        {
            {
                FacebookResponse wsResponse = null;
                KalturaSocialResponse clientResponse = new KalturaSocialResponse();

                // get group ID
                Group group = GroupsManager.GetGroup(groupId);

                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        // fire request
                        wsResponse = Client.FBUserRegister(group.SocialCredentials.Username, group.SocialCredentials.Password, token, extraParameters, ip);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                    ErrorUtils.HandleWSException(ex);
                }

                if (wsResponse == null)
                {
                    // general exception
                    throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
                }

                if (wsResponse.Status.Code != (int)StatusCode.OK)
                {
                    // internal web service exception
                    throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
                }

                // convert response
                clientResponse = AutoMapper.Mapper.Map<KalturaSocialResponse>(wsResponse.ResponseData);

                return clientResponse;
            }
        }

        [Obsolete]
        internal KalturaSocialResponse FBUserMerge(int groupId, string token, string username, string password, string facebookId)
        {
            FacebookResponse wsResponse = null;
            KalturaSocialResponse clientResponse = new KalturaSocialResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserMerge(group.SocialCredentials.Username, group.SocialCredentials.Password, token, facebookId, username, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            clientResponse = AutoMapper.Mapper.Map<KalturaSocialResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaFacebookSocial FBUserMerge(int groupId, string userId, string token)
        {
            FacebookResponse wsResponse = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserMergeByUserId(group.SocialCredentials.Username, group.SocialCredentials.Password, userId, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            KalturaFacebookSocial clientResponse = AutoMapper.Mapper.Map<KalturaFacebookSocial>(wsResponse.ResponseData);

            return clientResponse;
        }

        [Obsolete]
        internal KalturaSocialResponse FBUserUnmerge(int groupId, string token, string username, string password)
        {
            FacebookResponse wsResponse = null;
            KalturaSocialResponse clientResponse = new KalturaSocialResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserUnmerge(group.SocialCredentials.Username, group.SocialCredentials.Password, token, username, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            clientResponse = AutoMapper.Mapper.Map<KalturaSocialResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaFacebookSocial FBUserUnmerge(int groupId, string userId)
        {
            FacebookResponse wsResponse = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserUnmergeByUserId(group.SocialCredentials.Username, group.SocialCredentials.Password, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            KalturaFacebookSocial clientResponse = AutoMapper.Mapper.Map<KalturaFacebookSocial>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaOTTUser FBUserSignin(int groupId, string token, string udid)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
            FBSignin response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Client.FBUserSignin(group.SocialCredentials.Username, group.SocialCredentials.Password, token, Utils.Utils.GetClientIP(), udid, group.ShouldSupportSingleLogin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.status.Code, response.status.Message);
            }

            user = AutoMapper.Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }

        internal KalturaSocialConfig GetFacebookConfig(int groupId)
        {
            KalturaSocialFacebookConfig config = null;
            FacebookConfigResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Client.FBConfig(group.SocialCredentials.Username, group.SocialCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            config = AutoMapper.Mapper.Map<KalturaSocialFacebookConfig>(response.FacebookConfig);

            return config;
        }

        internal KalturaSocialFriendActivityListResponse GetFriendsActions(int groupId, string userId, long assetId, KalturaAssetType assetType, List<KalturaSocialActionType> actions, int pageSize, int pageIndex)
        {
            KalturaSocialFriendActivityListResponse friendsActivity = new KalturaSocialFriendActivityListResponse();
            SocialActivityResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            GetFriendsActionsRequest request = new GetFriendsActionsRequest()
            {
                m_eAssetType = eAssetType.MEDIA,
                m_eSocialPlatform = SocialPlatform.FACEBOOK,
                m_lAssetIDs = assetId > 0 ? new List<int> { (int)assetId } : null,
                m_nNumOfRecords = pageSize,
                m_nStartIndex = pageIndex, // make sure its right
                m_sSiteGuid = userId,
            };
            eUserAction wsAction;
            if (actions != null && actions.Count > 0)
            {
                var userActions = new List<eUserAction>();
                foreach (var action in actions)
                {
                    wsAction = SocialMappings.ConvertSocialAction(action);
                    userActions.Add(wsAction);
                }
                request.UserActions = userActions;
            }
            else
            {
                request.UserActions = new List<eUserAction>() { eUserAction.LIKE, eUserAction.RATES, eUserAction.WATCHES };
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Client.GetFriendsActions(group.SocialCredentials.Username, group.SocialCredentials.Password, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling social service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            friendsActivity.Objects = AutoMapper.Mapper.Map<List<KalturaSocialFriendActivity>>(response.SocialActivity);
            friendsActivity.TotalCount = response.TotalCount;

            return friendsActivity;
        }

        internal KalturaSocialConfig SetUserActionShareAndPrivacy(int groupId, string userId, KalturaSocialUserConfig socialActionConfig)
        {
            KalturaSocialConfig socialConfig = new KalturaSocialConfig();
            ApiObjects.Social.SocialPrivacySettingsResponse response = new ApiObjects.Social.SocialPrivacySettingsResponse();
            ApiObjects.Social.SocialPrivacySettings settings = new ApiObjects.Social.SocialPrivacySettings();

            settings.SocialNetworks = AutoMapper.Mapper.Map<ApiObjects.Social.SocialNetwork[]>(socialActionConfig).ToList();

            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // change status to Status - first in social ws 
                    response = Client.SetUserSocialPrivacySettings(group.SocialCredentials.Username, group.SocialCredentials.Password, userId, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetUserActionShare.  groupID: {0}, userId: {1}, exception: {2}", groupId, userId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            socialConfig = AutoMapper.Mapper.Map<KalturaSocialConfig>(response.settings);
           
            return socialConfig;

        }

        internal KalturaSocialConfig GetUserSocialPrivacySettings(string userId, int groupId)
        {
            KalturaSocialConfig socialConfig = new KalturaSocialConfig();

            ApiObjects.Social.SocialPrivacySettingsResponse response = new ApiObjects.Social.SocialPrivacySettingsResponse();
          
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // change status to Status - first in social ws 
                    response = Client.GetUserSocialPrivacySettings(group.SocialCredentials.Username, group.SocialCredentials.Password, userId);                    
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserSocialPrivacySettings groupID: {0}, userId: {1}, exception: {2}", groupId, userId, ex);
                ErrorUtils.HandleWSException(ex);
            }
            
            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }
            socialConfig = AutoMapper.Mapper.Map<KalturaSocialConfig>(response.settings);
            return socialConfig;
        }

        internal KalturaUserSocialActionResponse AddSocialAction(int groupId, string userId, string udid, KalturaSocialAction socialAction)
        {
            KalturaUserSocialActionResponse actionResponse = new KalturaUserSocialActionResponse();
            UserSocialActionResponse response = new UserSocialActionResponse();
            UserSocialActionRequest request = new UserSocialActionRequest();

            request = AutoMapper.Mapper.Map<ApiObjects.Social.UserSocialActionRequest>(socialAction);

            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // change status to Status - first in social ws 
                    request.SiteGuid = userId;
                    request.DeviceUDID = udid;
                    response = Client.AddUserSocialAction(group.SocialCredentials.Username, group.SocialCredentials.Password, request);
                    
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddSocialAction.  groupID: {0}, socialAction: {1}, exception: {2}", groupId, socialAction.ToString(), ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }
                      
            actionResponse = AutoMapper.Mapper.Map<KalturaUserSocialActionResponse>(response);            

            return actionResponse;  
        }

        private string GetAllFails(List<NetworkActionStatus> NetworksStatus)
        {            
            List<string> fails = new List<string>();
            foreach (NetworkActionStatus item in NetworksStatus)
            {
                if (item.Status.Code != (int)StatusCode.OK)
                {
                    fails.Add(string.Format("network - {0}: status code - {1}: status message - {2}", item.Network != null ? item.Network.ToString() : "null" , item.Status.Code, item.Status.Message));
                }
            }
            string result = string.Join(",", fails);
            return result;
        }
    }
}
