using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Users;
using WebAPI.Utils;
using System.Net;
using System.Web;
using System.ServiceModel;


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

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            if (response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }

        internal KalturaOTTUser Login(int partnerId, string userName, string password, string udid, SerializableDictionary<string, KalturaStringValue> extraParams, NameValueCollection nameValueCollection)
        {
            return Login(partnerId, userName, password, udid, GetMergedExtraParams(extraParams, nameValueCollection));
        }

        private Dictionary<string, KalturaStringValue> GetMergedExtraParams(Dictionary<string, KalturaStringValue> extraParams, NameValueCollection nameValueCollection)
        {
            Dictionary<string, KalturaStringValue> result = new Dictionary<string, KalturaStringValue>();

            if (extraParams != null && extraParams.Count > 0)
            {
                result = extraParams;
            }

            if (nameValueCollection != null && nameValueCollection.Count > 0)
            {
                foreach (var key in nameValueCollection.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key))
                    {
                        result.Add(key, new KalturaStringValue() { value = nameValueCollection[key] });
                    }
                }
            }
            return result;
        }

        public Models.Users.KalturaOTTUser SignUp(int groupId, KalturaOTTUser userData, string password)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
            UserResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(userData);
                    WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(userData.DynamicData);

                    if (userDynamicData == null)
                        userDynamicData = new UserDynamicData();

                    response = Users.SignUp(group.UsersCredentials.Username, group.UsersCredentials.Password, userBasicData, userDynamicData, password, userData.AffiliateCode);
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

        public KalturaUserLoginPin GenerateLoginPin(int groupId, string userId, string secret)
        {
            KalturaUserLoginPin pinCode = null;
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

            pinCode = Mapper.Map<KalturaUserLoginPin>(response);

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

        public KalturaUserLoginPin SetLoginPin(int groupId, string userId, string pin, string secret)
        {
            KalturaUserLoginPin pinCode = null;
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

            pinCode = Mapper.Map<WebAPI.Models.Users.KalturaUserLoginPin>(response);

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

        public List<Models.Users.KalturaOTTUser> GetUsersData(int groupId, List<string> usersIds)
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

        public Models.Users.KalturaOTTUser SetUserData(int groupId, string siteGuid, KalturaOTTUser user)
        {
            WebAPI.Users.UserBasicData userBasicData = Mapper.Map<WebAPI.Users.UserBasicData>(user);
            WebAPI.Users.UserDynamicData userDynamicData = Mapper.Map<WebAPI.Users.UserDynamicData>(user.DynamicData);

            WebAPI.Models.Users.KalturaOTTUser responseUser = null;
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

            responseUser = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return responseUser;
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

        public bool RemoveUserFavorite(int groupId, string userId, int domainID, int[] mediaIDs)
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

            return response.Code == (int)StatusCode.OK;
        }

        public List<Models.Users.KalturaFavorite> GetUserFavorites(int groupId, string userId, int domainID, string udid, string mediaType, KalturaFavoriteOrderBy orderBy)
        {
            List<WebAPI.Models.Users.KalturaFavorite> favorites = null;

            Group group = GroupsManager.GetGroup(groupId);
            FavoriteOrderBy wsOrderBy = UsersMappings.ConvertFavoriteOrderBy(orderBy);

            FavoriteResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GetUserFavorites(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, domainID, udid, mediaType, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
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

            if (response == null || response.Status == null)
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

        internal List<KalturaFavorite> FilterFavoriteMedias(int groupId, string userId, List<int> mediaIds, string udid, string mediaType, KalturaFavoriteOrderBy orderBy)
        {
            FavoriteResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);
            FavoriteOrderBy wsOrderBy = UsersMappings.ConvertFavoriteOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.FilterFavoriteMediaIds(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, mediaIds.ToArray(), udid, mediaType, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            return Mapper.Map<List<WebAPI.Models.Users.KalturaFavorite>>(response.Favorites);
        }

        internal List<long> GetUserRoleIds(int groupId, string userId)
        {
            List<long> roleIds = null;
            LongIdsResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GetUserRoleIds(group.UsersCredentials.Username, group.UsersCredentials.Password, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. ws address: {0}, exception: {1}", Users.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Ids != null)
            {
                roleIds = response.Ids.ToList();
            }

            return roleIds;
        }

        internal bool AddRoleToUser(int groupId, string userId, long roleId)
        {
            Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.AddRoleToUser(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, roleId);
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

        internal bool DeleteUser(int groupId, int userId)
        {
            Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.DeleteUser(group.UsersCredentials.Username, group.UsersCredentials.Password, userId);
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

        public bool SignOut(int groupId, int userId, string ip, string deviceId)
        {
            UserResponseObject response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.SignOut(group.UsersCredentials.Username, group.UsersCredentials.Password, userId.ToString(), string.Empty, ip, deviceId, group.ShouldSupportSingleLogin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.m_RespStatus != ResponseStatus.SessionLoggedOut)
            {
                throw new ClientException((int)response.m_RespStatus, StatusCode.Error.ToString());
            }

            return true;
        }

        public WebAPI.Models.Users.KalturaOTTUser ActivateAccount(int groupId, string username, string token)
        {
            WebAPI.Models.Users.KalturaOTTUser user = null;
            UserResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.ActivateAccount(group.UsersCredentials.Username, group.UsersCredentials.Password, username, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            if (response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            user = Mapper.Map<WebAPI.Models.Users.KalturaOTTUser>(response.user);

            return user;
        }

        public bool ResendActivationToken(int groupId, string username)
        {
            Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.ResendActivationToken(group.UsersCredentials.Username, group.UsersCredentials.Password, username);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
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

        internal KalturaUserAssetsListItem AddItemToUsersList(int groupId, string userId, KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem listItem = null;
            UsersListItemResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.AddItemToUsersList(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
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

            if (response.Item == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            listItem = Mapper.Map<KalturaUserAssetsListItem>(response.Item);

            return listItem;
        }

        internal bool DeleteItemFromUsersList(int groupId, string userId, KalturaUserAssetsListItem userAssetsListItem)
        {
            Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.DeleteItemFromUsersList(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
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

        internal KalturaUserAssetsListItem GetItemFromUsersList(int groupId, string userId, KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem listItem = null;
            UsersListItemResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Users.GetItemFromUsersList(group.UsersCredentials.Username, group.UsersCredentials.Password, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", group.UsersCredentials.Username, group.UsersCredentials.Password, ex);
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

            if (response.Item == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            listItem = Mapper.Map<KalturaUserAssetsListItem>(response.Item);

            return listItem;
        }
    }
}

namespace WebAPI.Users
{
    // adding request ID to header
    public partial class UsersService
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}