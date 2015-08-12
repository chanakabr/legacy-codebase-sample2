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

        internal KalturaFacebookResponse FBUserData(int groupId, string token)
        {
            FacebookResponse wsResponse = null;
            KalturaFacebookResponse clientResponse = new KalturaFacebookResponse();

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
            clientResponse = AutoMapper.Mapper.Map<KalturaFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaFacebookResponse FBUserRegister(int groupId, string token, List<WebAPI.Social.KeyValuePair> extraParameters, string ip)
        {
            {
                FacebookResponse wsResponse = null;
                KalturaFacebookResponse clientResponse = new KalturaFacebookResponse();

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
                clientResponse = AutoMapper.Mapper.Map<KalturaFacebookResponse>(wsResponse.ResponseData);

                return clientResponse;
            }
        }

        internal KalturaFacebookResponse FBUserMerge(int groupId, string token, string username, string password, string facebookId)
        {
            FacebookResponse wsResponse = null;
            KalturaFacebookResponse clientResponse = new KalturaFacebookResponse();

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
            clientResponse = AutoMapper.Mapper.Map<KalturaFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaFacebookResponse FBUserUnmerge(int groupId, string token, string username, string password)
        {
            FacebookResponse wsResponse = null;
            KalturaFacebookResponse clientResponse = new KalturaFacebookResponse();

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
            clientResponse = AutoMapper.Mapper.Map<KalturaFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal KalturaUser FBUserSignin(int groupId, string token, string udid)
        {
            WebAPI.Models.Users.KalturaUser user = null;
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

            user = AutoMapper.Mapper.Map<WebAPI.Models.Users.KalturaUser>(response.user);

            return user;
        }

        internal KalturaFacebookConfig GetFacebookConfig(int groupId)
        {
            KalturaFacebookConfig config = null;
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

            config = AutoMapper.Mapper.Map<KalturaFacebookConfig>(response.FacebookConfig);

            return config;
        }
    }
}