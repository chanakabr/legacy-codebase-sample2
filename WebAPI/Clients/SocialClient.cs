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
            KalturaSocialConfig config = null;
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

            config = AutoMapper.Mapper.Map<KalturaSocialConfig>(response.FacebookConfig);

            return config;
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

            if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
            {
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            else
            {
                if (request.Headers != null &&
                request.Headers[KLogMonitor.Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(KLogMonitor.Constants.REQUEST_ID_KEY, HttpContext.Current.Items[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            return request;
        }
    }
}