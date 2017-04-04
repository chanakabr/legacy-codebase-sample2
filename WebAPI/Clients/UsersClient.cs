using ApiObjects;
using ApiObjects.Response;
using AutoMapper;
using Core.Users;
using KLogMonitor;
using ObjectsConvertor.Mapping;
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
using WebAPI.Utils;


namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public UsersClient()
        {
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
                    response = Core.Users.Module.LogIn(groupId, userName, password, string.Empty, Utils.Utils.GetClientIP(), deviceId,
                        group.ShouldSupportSingleLogin, keyValueList);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    UserBasicData userBasicData = Mapper.Map<UserBasicData>(userData);
                    UserDynamicData userDynamicData = Mapper.Map<UserDynamicData>(userData.DynamicData);

                    if (userDynamicData == null)
                        userDynamicData = new UserDynamicData();

                    response = Core.Users.Module.SignUp(groupId, userBasicData, userDynamicData, password, userData.AffiliateCode);
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
            ApiObjects.Response.Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.SendRenewalPasswordMail(groupId, userName);
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
            ApiObjects.Response.Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.RenewPassword(groupId, userName, password);
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
            ApiObjects.Response.Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ReplacePassword(groupId, userName, oldPassword, newPassword);
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
        //    

        //    try
        //    {
        //        //TODO: add parameters
        //        UserResponseObject response;
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            response = Core.Users.Module.SignIn(groupId, userName, password, string.Empty, string.Empty, string.Empty, false);
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
            

            PinCodeResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GenerateLoginPIN(groupId, userId, secret);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            UserResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.LoginWithPIN(groupId, pin, string.Empty, Utils.Utils.GetClientIP(), deviceId, false, null, secret);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service", true, ex);

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
            
            WebAPI.Models.Users.KalturaOTTUser user = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.CheckPasswordToken(groupId, token);
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
            

            PinCodeResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.SetLoginPIN(groupId, userId, pin, secret);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            ApiObjects.Response.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ClearLoginPIN(groupId, userId, pinCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUsers(groupId, usersIds.ToArray(), Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUsersData. exception: {0}, ", ex);
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
            UserBasicData userBasicData = Mapper.Map<UserBasicData>(user);
            UserDynamicData userDynamicData = Mapper.Map<UserDynamicData>(user.DynamicData);

            WebAPI.Models.Users.KalturaOTTUser responseUser = null;
            UserResponse response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.SetUser(groupId, siteGuid, userBasicData, userDynamicData);
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
            

            ApiObjects.Response.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.AddUserFavorit(groupId, userId, domainID, deviceUDID, mediaType, mediaId, extraData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            

            ApiObjects.Response.Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.RemoveUserFavorit(groupId, userId, mediaIDs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            
            FavoriteOrderBy wsOrderBy = UsersMappings.ConvertFavoriteOrderBy(orderBy);

            FavoriteResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserFavorites(groupId, userId, domainID, udid, mediaType, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            UsersItemsListsResponse response = null;
            ListType wsListType = UsersMappings.ConvertUserAssetsListType(listType);
            ListItemType wsAssetType = UsersMappings.ConvertUserAssetsListItemType(assetType);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetItemsFromUsersLists(groupId, userIds, wsListType, wsAssetType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            
            FavoriteOrderBy wsOrderBy = UsersMappings.ConvertFavoriteOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.FilterFavoriteMediaIds(groupId, userId, mediaIds, udid, mediaType, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserRoleIds(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.AddRoleToUser(groupId, userId, roleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.DeleteUser(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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
                    response = Core.Users.Module.SignOut(groupId, userId.ToString(), string.Empty, ip, deviceId, group.ShouldSupportSingleLogin);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ActivateAccount(groupId, username, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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
            ApiObjects.Response.Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ResendActivationToken(groupId, username);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

            

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.AddItemToUsersList(groupId, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

        internal bool DeleteItemFromUsersList(int groupId, string userId, string assetId, KalturaUserAssetsListType listType)
        {
            ApiObjects.Response.Status response = null;

            

            try
            {
                Item item = new Item()
                {
                    ItemType = ListItemType.Media,
                    ItemId = int.Parse(assetId),
                    UserId = userId,
                    ListType = UsersMappings.ConvertUserAssetsListType(listType)
                };

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.DeleteItemFromUsersList(groupId, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

        [Obsolete]
        internal bool DeleteItemFromUsersList(int groupId, string userId, KalturaUserAssetsListItem userAssetsListItem)
        {
            ApiObjects.Response.Status response = null;

            

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.DeleteItemFromUsersList(groupId, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

        internal KalturaUserAssetsListItem GetItemFromUsersList(int groupId, string userId, string assetId, KalturaUserAssetsListType listType, KalturaUserAssetsListItemType itemType)
        {
            KalturaUserAssetsListItem listItem = null;
            UsersListItemResponse response = null;

            

            try
            {
                Item item = new Item() { 
                    ItemType = UsersMappings.ConvertUserAssetsListItemType(itemType),
                    ItemId = int.Parse(assetId),
                    UserId = userId,
                    ListType = UsersMappings.ConvertUserAssetsListType(listType)
                };

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetItemFromUsersList(groupId, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

        [Obsolete]
        internal KalturaUserAssetsListItem GetItemFromUsersList(int groupId, string userId, KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem listItem = null;
            UsersListItemResponse response = null;

            

            try
            {
                Item item = Mapper.Map<Item>(userAssetsListItem);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetItemFromUsersList(groupId, userId, item);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while ActivateAccount. Username: {0}, Password: {1}, exception: {2}", groupId, ex);
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

        internal KalturaOTTUserListResponse GetUserByExternalID(int groupId, string externalID)
        {
            KalturaOTTUserListResponse listUser = null;
            UserResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserByExternalID(groupId, externalID, -1);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserDataByCoGuid. Username: {0}, Password: {1}, externalID: {2}", groupId, externalID);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message.ToString());
            }

            if (response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            KalturaOTTUser User;
            User = Mapper.Map<KalturaOTTUser>(response.user);
            listUser = new KalturaOTTUserListResponse()
            {
                Users = new List<KalturaOTTUser>() { User },
                TotalCount = 1,
            };

            return listUser;
        }

        internal KalturaOTTUserListResponse GetUserByName(int groupId, string userName)
        {
            KalturaOTTUserListResponse listUser = null;
            UserResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserByName(groupId, userName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserDataByCoGuid. Username: {0}, Password: {1}, userNameFilter: {2}", groupId, userName);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message.ToString());
            }

            if (response.user == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            KalturaOTTUser User;
            User = Mapper.Map<KalturaOTTUser>(response.user);
            listUser = new KalturaOTTUserListResponse()
            {
                Users = new List<KalturaOTTUser>() { User },
                TotalCount = 1,                
            };

            return listUser;
        }

        internal void UpdateUserPassword(int groupId, int userId, string password)
        {
            ApiObjects.Response.Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.UpdateUserPassword(groupId, userId, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateUserPassword. userId: {0}, password : {1}, exception: {2}", userId, password, ex);
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
        }

        internal bool IsUserActivated(int groupId, string userId)
        {
            ApiObjects.Response.Status response = null;

            int userIdntifier = 0;
            int.TryParse(userId, out userIdntifier);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.IsUserActivated(groupId, userIdntifier);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {1}", ex);
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

    }
}
