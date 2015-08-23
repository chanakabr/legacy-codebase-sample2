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
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.ObjectsConvertor.Mapping;

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

        public WebAPI.Models.Users.KalturaOTTUser Login(int groupId, string userName, string password, string deviceId,
            Dictionary<string, KalturaStringValue> extraParams)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<KeyValuePair> keyValueList = new List<KeyValuePair>();
                    if (extraParams != null)
                    {
                        keyValueList = extraParams.Select(p => new KeyValuePair { key = p.Key, value = p.Value.value }).ToList();
                    }
                    response = Users.LogIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, Utils.Utils.GetClientIP(), deviceId,
                        group.ShouldSupportSingleLogin, keyValueList.ToArray());
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

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }


        public Models.Users.KalturaOTTUser SignUp(int groupId, Models.Users.KalturaUserBasicData user_basic_data, 
            Dictionary<string, KalturaStringValue> user_dynamic_data, string password, string affiliateCode)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(user_basic_data);
                    WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(user_dynamic_data);

                    if (userDynamicData == null)
                        userDynamicData = new UserDynamicData();

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

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

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

        public KalturaLoginPin GenerateLoginPin(int groupId, string userId, string secret)
        {
            KalturaLoginPin pinCode = null;
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

            pinCode = Mapper.Map<KalturaLoginPin>(response);

            return pinCode;
        }

        public WebAPI.Models.Users.KalturaOTTUser LoginWithPin(int groupId, string deviceId, string pin, string secret)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
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

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }

        public Models.Users.KalturaOTTUser CheckPasswordToken(int groupId, string token)
        {
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);
            WebAPI.Models.Users.KalturaOTTUser user = null;
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

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user.m_user);

            return user;
        }

        public KalturaLoginPin SetLoginPin(int groupId, string userId, string pin, string secret)
        {
            KalturaLoginPin pinCode = null;
            Group group = GroupsManager.GetGroup(groupId);

            PinCodeResponse response = null;
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

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp.Code, response.resp.Message);
            }

            return pinCode;
        }

        public bool ClearLoginPIN(int groupId, string userId, string pinCode)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Users.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.ClearLoginPIN(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, pinCode);
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

        public List<Models.Users.KalturaOTTUser> GetUsersData(int groupId, List<int> usersIds)
        {
            List<WebAPI.Models.Users.KalturaOTTUser> users = null;
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

            users = Mapper.Map<List<WebAPI.Models.Users.KalturaOTTUser>>(response.users);

            return users;
        }

        public Models.Users.KalturaOTTUser SetUserData(int groupId, string siteGuid, Models.Users.KalturaUserBasicData user_basic_data, 
            Dictionary<string, KalturaStringValue> user_dynamic_data)
        {
            WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(user_basic_data);
            WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(user_dynamic_data);

            WebAPI.Models.Users.KalturaOTTUser user = null;
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

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }

        public bool AddUserFavorite(int groupId, string userId, int domainID, string deviceUDID, string mediaType, string mediaId, string extraData)
        {
            bool res = false;
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Users.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.AddUserFavorit(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, domainID, deviceUDID, mediaType, mediaId, extraData);
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
            else
                res = true;

            return res;
        }

        public void RemoveUserFavorite(int groupId, string userId, int domainID, int[] mediaIDs)
        {
            Group group = GroupsManager.GetGroup(groupId);

            WebAPI.Users.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.RemoveUserFavorit(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, mediaIDs);
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
        }

        public List<Models.Users.KalturaFavorite> GetUserFavorites(int groupId, string userId, int domainID, string udid, string mediaType)
        {
            List<WebAPI.Models.Users.KalturaFavorite> favorites = null;

            Group group = GroupsManager.GetGroup(groupId);

            FavoriteResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GetUserFavorites(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, domainID, udid, mediaType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null | response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            favorites = Mapper.Map<List<WebAPI.Models.Users.KalturaFavorite>>(response.Favorites);

            return favorites;
        }

        public List<KalturaUserAssetsList> GetItemFromList(int groupId, List<string> userIds, KalturaUserAssetsListType listType, KalturaUserAssetsListItemType assetType)
        {
            List<KalturaUserAssetsList> userAssetsList = null;

            Group group = GroupsManager.GetGroup(groupId);

            UsersItemsListsResponse response = null;
            ListType wsListType = UsersMappings.ConvertUserAssetsListType(listType);
            ItemType wsAssetType = UsersMappings.ConvertUserAssetsListItemType(assetType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GetItemsFromUsersLists(group.UsersCredentials.Username, group.UsersCredentials.Password, userIds.ToArray(), wsListType, wsAssetType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null | response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            userAssetsList = Mapper.Map<List<WebAPI.Models.Users.KalturaUserAssetsList>>(response.UsersItemsLists);

            return userAssetsList;
        }

        internal List<KalturaFavorite> FilterFavoriteMedias(int groupId, string userId, List<int> mediaIds)
        {
            FavoriteResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.FilterFavoriteMediaIds(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, mediaIds.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null | response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            return Mapper.Map<List<WebAPI.Models.Users.KalturaFavorite>>(response.Favorites);
        }
    }
}