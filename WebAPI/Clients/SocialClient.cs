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

        internal ClientFacebookResponse FBUserData(int groupId, string token)
        {
            FacebookResponse wsResponse = null;
            ClientFacebookResponse clientResponse = new ClientFacebookResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Client.FBUserData(group.SocialCredentials.Username, group.SocialCredentials.Password, token, "0");
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
            clientResponse = AutoMapper.Mapper.Map<ClientFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal ClientFacebookResponse FBUserRegister(int groupId, string token, List<WebAPI.Social.KeyValuePair> extraParameters, string ip)
        {
            {
                FacebookResponse wsResponse = null;
                ClientFacebookResponse clientResponse = new ClientFacebookResponse();

                // get group ID
                Group group = GroupsManager.GetGroup(groupId);

                try
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                    {
                        // fire request
                        wsResponse = Client.FBUserRegister(group.SocialCredentials.Username, group.SocialCredentials.Password, token, "0", extraParameters.ToArray(), ip);
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
                clientResponse = AutoMapper.Mapper.Map<ClientFacebookResponse>(wsResponse.ResponseData);

                return clientResponse;
            }
        }

        internal ClientFacebookResponse FBUserMerge(int groupId, string token, string username, string password, string facebookId)
        {
            FacebookResponse wsResponse = null;
            ClientFacebookResponse clientResponse = new ClientFacebookResponse();

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
            clientResponse = AutoMapper.Mapper.Map<ClientFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }

        internal ClientFacebookResponse FBUserUnmerge(int groupId, string token, string username, string password)
        {
            FacebookResponse wsResponse = null;
            ClientFacebookResponse clientResponse = new ClientFacebookResponse();

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
            clientResponse = AutoMapper.Mapper.Map<ClientFacebookResponse>(wsResponse.ResponseData);

            return clientResponse;
        }
    }
}