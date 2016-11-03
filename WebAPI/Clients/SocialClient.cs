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
using WebAPI.Social;
using WebAPI.Utils;
using WebAPI.Models.Users;
using System.Net;
using System.ServiceModel;
using WebAPI.Models.Catalog;

namespace WebAPI.Clients
{
    public class SocialClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public SocialClient()
        {
        }

        protected WebAPI.Social.module Client
        {
            get
            {
                return (Module as WebAPI.Social.module);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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

        internal KalturaFacebookSocial FBRegister(int groupId, string token, List<WebAPI.Social.KeyValuePair> extraParameters, string ip)
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
                        wsResponse = Client.FBUserRegister(group.SocialCredentials.Username, group.SocialCredentials.Password, token, extraParameters.ToArray(), ip);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
        internal KalturaSocialResponse FBUserRegister(int groupId, string token, List<WebAPI.Social.KeyValuePair> extraParameters, string ip)
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
                        wsResponse = Client.FBUserRegister(group.SocialCredentials.Username, group.SocialCredentials.Password, token, extraParameters.ToArray(), ip);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
                m_lAssetIDs = assetId > 0 ? new int[] { (int)assetId } : null,
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
                request.UserActions = userActions.ToArray();
            }
            else
            {
                request.UserActions = new eUserAction[3] { eUserAction.LIKE, eUserAction.RATES, eUserAction.WATCHES };
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
                log.ErrorFormat("Exception received while calling social service. ws address: {0}, exception: {1}", Client.Url, ex);
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
            WebAPI.Social.SocialPrivacySettingsResponse response = new Social.SocialPrivacySettingsResponse();
            WebAPI.Social.SocialPrivacySettings settings = new Social.SocialPrivacySettings();

            //settings.SocialNetworks = SocialMappings.ConvertSocialNetwork(socialActionConfig);

            settings.SocialNetworks = AutoMapper.Mapper.Map<Social.SocialNetwork[]>(socialActionConfig);

           // Mapper.CreateMap<Social.SocialPrivacySettings, KalturaSocialConfig>().ConstructUsing(ConvertSocialNetwork);



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

            //socialConfig = SocialMappings.ConvertSocialNetwork(response.settings);
            socialConfig = AutoMapper.Mapper.Map<KalturaSocialConfig>(response.settings);
           
            return socialConfig;

        }

        internal KalturaSocialConfig GetUserSocialPrivacySettings(string userId, int groupId)
        {
            KalturaSocialConfig socialConfig = new KalturaSocialConfig();

            WebAPI.Social.SocialPrivacySettingsResponse response = new Social.SocialPrivacySettingsResponse();
          
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
            //socialConfig = SocialMappings.ConvertSocialNetwork(response.settings);
            socialConfig = AutoMapper.Mapper.Map<KalturaSocialConfig>(response.settings);
            return socialConfig;
        }
    }
}

namespace WebAPI.Social
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}