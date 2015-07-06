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

        public WebAPI.Models.Users.User Login(int groupId, string userName, string password, string deviceId, Dictionary<string, string> keyValueList)
        {
            WebAPI.Models.Users.User user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    string ip = Utils.Utils.GetClientIP();
                    List<KeyValuePair> lKeyvalue = keyValueList.Select(p => new KeyValuePair { key = p.Key, value = p.Value }).ToList();
                    response = Users.LogIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, ip, deviceId, group.ShouldSupportSingleLogin, lKeyvalue.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. Username: {0}, PAssword: {1}, exception: {2}", userName, password, ex);
                ErrorUtils.HandleWSException(ex);
            }
           
            if (response == null || response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user);

            return user;
        }


        public Models.Users.User SignUp(int groupId, Models.Users.UserBasicData user_basic_data, Dictionary<string, string> user_dynamic_data, string password, string affiliateCode)
        {
            WebAPI.Models.Users.User user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(user_basic_data);
                    WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(user_dynamic_data);
                    response = Users.SignUp(group.UsersCredentials.Username, group.UsersCredentials.Password, userBasicData, userDynamicData, password, affiliateCode);                     
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SignUp.  Password: {0}, exception: {1}", password, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user);

            return user;
        }

        public bool SendNewPassword(int groupId, string userName)
        {
            WebAPI.Users.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);              
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.SendRenewalPasswordMail(group.UsersCredentials.Username, group.UsersCredentials.Password, userName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SendNewPassword. Username: {0}, exception: {1}", userName, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool RenewPassword(int groupId, string userName, string password)
        {
            WebAPI.Users.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.RenewPassword(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while RenewPassword. Username: {0}, password : {1}, exception: {2}", userName, password, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public bool ChangeUserPassword(int groupId, string userName, string oldPassword, string newPassword)
        {
            WebAPI.Users.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.ReplacePassword(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, oldPassword, newPassword);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ChangeUserPassword. Username: {0}, oldPassword : {1}, newPassword: {2}, exception : {3}", userName, oldPassword, newPassword, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        //public WebAPI.Models.Users.ClientUser SignIn(int groupId, string userName, string password)
        //{
        //    WebAPI.Models.Users.ClientUser user = null;
        //    Group group = GroupsManager.GetGroup(groupId);

        //    try
        //    {
        //        //TODO: add parameters
        //        UserResponseObject response;
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            response = Users.SignIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, string.Empty, string.Empty, false);
        //        }

        //        user = Mapper.Map<WebAPI.Models.Users.ClientUser>(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Error while signing in. Username: {0}, exception: {1}", userName, ex);
        //        throw new ClientException((int)StatusCode.InternalConnectionIssue);
        //    }
        //    return user;
        //}

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

            UserResponse response = null;
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

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user);

            return user;
        }

        public Models.Users.User CheckPasswordToken(int groupId, string token)
        {            
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);
            WebAPI.Models.Users.User user = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {   
                    response = Users.CheckPasswordToken(group.UsersCredentials.Username, group.UsersCredentials.Password, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while signing in.  token: {0}, exception: {1}", token, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user.m_user);

            return user;
        }

        public bool SetLoginPin(int groupId, string userId, string pin, string secret)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Users.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.SetLoginPIN(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, pin, secret);
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

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        public bool ClearLoginPIN(int groupId, string userId)
        {
            Group group = GroupsManager.GetGroup(groupId);
            
            WebAPI.Users.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.ClearLoginPIN(group.UsersCredentials.Username, group.UsersCredentials.Password, userId);
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

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        public List<Models.Users.User> GetUsersData(int groupId, List<int> usersIds)
        {
            List<WebAPI.Models.Users.User> users = null;
            UsersResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<string> siteGuids = usersIds.Select(x => x.ToString()).ToList();
                    response = Users.GetUsers(group.UsersCredentials.Username, group.UsersCredentials.Password, siteGuids.ToArray(), Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SignUp.  exception: {0}, ", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.users == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            users = Mapper.Map<List<WebAPI.Models.Users.User>>(response.users);

            return users;
        }

        public Models.Users.User SetUserData(int groupId, string siteGuid, Models.Users.UserBasicData user_basic_data, Dictionary<string, string> user_dynamic_data)
        {
            WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(user_basic_data);
            WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(user_dynamic_data);

            WebAPI.Models.Users.User user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.SetUser(group.UsersCredentials.Username, group.UsersCredentials.Password, siteGuid, userBasicData, userDynamicData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. siteGuid: {0}, exception: {1}", siteGuid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            user = Mapper.Map<WebAPI.Models.Users.User>(response.user);

            return user;
        }
    }
}