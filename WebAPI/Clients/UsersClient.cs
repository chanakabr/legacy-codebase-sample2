using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Users;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using KLogMonitor;
using System.Reflection;
using WebAPI.Models.Users;
using System.ServiceModel;
using System.Net;

namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public UsersClient()
        {
        }

        protected WebAPI.Users.UsersService Users
        {
            get
            {
                return (Module as WebAPI.Users.UsersService);
            }
        }

        public WebAPI.Models.Users.ClientUser SignIn(int groupId, string userName, string password)
        {
            WebAPI.Models.Users.ClientUser user = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                //TODO: add parameters
                UserResponseObject response;
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.SignIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, string.Empty, string.Empty, false);
                }

                user = Mapper.Map<WebAPI.Models.Users.ClientUser>(response);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while signing in. Username: {0}, exception: {1}", userName, ex);
                throw new ClientException((int)StatusCode.InternalConnectionIssue);
            }
            return user;
        }

        public LoginPin GenerateLoginPin(int groupId, string userId, string secret)
        {
            LoginPin pinCode = null;
            Group group = GroupsManager.GetGroup(groupId);

            PinCodeResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GenerateLoginPIN(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, secret);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp.Code, response.resp.Message);
            }

            pinCode = Mapper.Map<LoginPin>(response);

            return pinCode;
        }

        public WebAPI.Models.Users.User LoginWithPin(int groupId, string deviceId, string pin, string secret)
        {
            WebAPI.Models.Users.User user = null;
            Group group = GroupsManager.GetGroup(groupId);

            LoginResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.LoginWithPIN(group.UsersCredentials.Username, group.UsersCredentials.Password, pin, string.Empty, Utils.Utils.GetClientIP(), deviceId, false, null, secret);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}", true, ex,
                        Users.Url                          // 0
                        );

                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp.Code, response.resp.Message);
            }

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user.m_user);

            return user;
        }
    }
}