using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using ApiLogic.Users;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SSOAdapter;
using AutoMapper;
using Core.Users;
using Phx.Lib.Log;
using ObjectsConvertor.Mapping;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public UsersClient()
        {
        }

        public KalturaOTTUser Login(int groupId, string userName, string password, string deviceId, Dictionary<string, string> extraParams, bool shouldSupportSingleLogin)
        {
            GenericResponse<UserResponseObject> userResponse = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<KeyValuePair> keyValueList = new List<KeyValuePair>();
                    if (extraParams != null)
                    {
                        keyValueList = extraParams.Select(p => new KeyValuePair { key = p.Key, value = p.Value }).ToList();
                    }

                    userResponse = Core.Users.Module.LogIn(groupId,
                        userName,
                        password,
                        string.Empty,
                        Utils.Utils.GetClientIP(),
                        deviceId,
                        shouldSupportSingleLogin,
                        keyValueList);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. Username: {0}, exception: {1}", userName, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (userResponse == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!userResponse.IsOkStatusCode())
            {
                if (userResponse.Status.Code == (int)eResponseStatus.UserExternalError)
                {
                    throw new ClientExternalException(BadRequestException.EXTERNAL_ERROR, userResponse.Status.Code, userResponse.Status.Message, userResponse.Object.ExternalCode, userResponse.Object.ExternalMessage);
                }
                else
                {
                    throw new ClientException(userResponse.Status);
                }
            }

            if (userResponse.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaOTTUser user = Mapper.Map<KalturaOTTUser>(userResponse.Object);
            return user;
        }

        internal void ValidatePinEnhancements(int? pinUsages, long? pinDuration)
        {
            if (pinUsages.HasValue && pinUsages.Value < 0)
            {
                throw new ClientException((int)eResponseStatus.InvalidParameters, "Invalid Parameter value: [pinUsages]");
            }
            if (pinDuration.HasValue && pinDuration.Value < 0)
            {
                throw new ClientException((int)eResponseStatus.InvalidParameters, "Invalid Parameter value: [pinDuration]");
            }
        }

        public KalturaOTTUser SignUp(int groupId, KalturaOTTUser userData, string password)
        {
            GenericResponse<UserResponseObject> SignUpUser()
            {
                var userBasicData = Mapper.Map<UserBasicData>(userData);
                var userDynamicData = Mapper.Map<UserDynamicData>(userData.DynamicData) ?? new UserDynamicData();

                return Core.Users.Module.SignUp(groupId, userBasicData, userDynamicData, password, userData.AffiliateCode);
            }

            var signUpResponse = ClientUtils.GetResponseFromWs(SignUpUser);
            if (!signUpResponse.IsOkStatusCode())
            {
                if (signUpResponse.Status.Code == (int)eResponseStatus.UserExternalError)
                {
                    throw new ClientExternalException(
                        BadRequestException.EXTERNAL_ERROR,
                        signUpResponse.Status.Code,
                        signUpResponse.Status.Message,
                        signUpResponse.Object.ExternalCode,
                        signUpResponse.Object.ExternalMessage);
                }

                throw new ClientException(signUpResponse.Status);
            }

            if (signUpResponse.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            return Mapper.Map<KalturaOTTUser>(signUpResponse.Object);
        }

        public bool SendNewPassword(int groupId, string userName, string templateName)
        {
            Func<ApiObjects.Response.Status> sendRenewalPasswordMailFunc = () => Core.Users.Module.SendRenewalPasswordMail(groupId, userName, templateName);
            return ClientUtils.GetResponseStatusFromWS(sendRenewalPasswordMailFunc);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        public KalturaOTTUser RenewPasswordWithToken(int groupId, string token, string password)
        {
            GenericResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.RenewPasswordWithToken(groupId, token, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while RenewPasswordWithToken. token: {0}, exception: {2}", token, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            KalturaOTTUser user = Mapper.Map<KalturaOTTUser>(response.Object.m_user);
            return user;
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
                log.ErrorFormat("Error while ChangeUserPassword. Username: {0}, exception : {1}", userName, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        public bool SwitchUsers(int groupId, string oldUserId, string newUserId, string udid)
        {
            ApiObjects.Response.Status response = null;

            Group group = GroupsManager.Instance.GetGroup(groupId);

            if (!group.IsSwitchingUsersAllowed)
            {
                throw new ForbiddenException(ForbiddenException.SWITCH_USER_NOT_ALLOWED_FOR_PARTNER);
            }

            if (string.IsNullOrEmpty(newUserId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userIdToSwitch");
            }

            if (!Managers.AuthorizationManager.IsUserInHousehold(newUserId, groupId))
            {
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "OTT-User", newUserId);
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ChangeUsers(groupId, oldUserId, newUserId, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SwitchUsets. groupId: {0}, oldUserId : {1}, newUserId: {2}, UDID: {3}, exception : {4}", groupId, oldUserId, newUserId, udid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        public KalturaUserLoginPin GenerateLoginPin(int groupId, string userId, string secret, int? pinUsages, long? pinDuration)
        {
            KalturaUserLoginPin pinCode = null;

            PinCodeResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GenerateLoginPIN(groupId, userId, secret, pinUsages, pinDuration);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp);
            }

            pinCode = Mapper.Map<KalturaUserLoginPin>(response);

            return pinCode;
        }

        public KalturaOTTUser LoginWithPin(int groupId, string deviceId, string pin, string secret)
        {
            KalturaOTTUser user = null;
            GenericResponse<UserResponseObject> response = null;
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

            if (response == null || response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            user = Mapper.Map<KalturaOTTUser>(response.Object);

            return user;
        }

        public KalturaOTTUser CheckPasswordToken(int groupId, string token)
        {
            GenericResponse<UserResponseObject> response = null;

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

            if (response == null || response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            KalturaOTTUser user = Mapper.Map<KalturaOTTUser>(response.Object.m_user);
            return user;
        }

        public KalturaUserLoginPin SetLoginPin(int groupId, string userId, string pin, string secret, int? pinUsages, long? pinDuration)
        {
            KalturaUserLoginPin pinCode = null;
            PinCodeResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.SetLoginPIN(groupId, userId, pin, secret, pinUsages, pinDuration);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        public List<Models.Users.KalturaOTTUser> GetUsersData(int groupId, List<string> usersIds, HashSet<long> roleIds = null)
        {
            List<WebAPI.Models.Users.KalturaOTTUser> users = null;
            UsersResponse response = null;

            if (roleIds != null && roleIds.Count > 0)
            {
                if (usersIds == null || usersIds.Count == 0)
                {
                    usersIds = Core.Users.Module.GetUserIdsByRoleIds(groupId, roleIds);
                }
                else
                {
                    List<string> filteredUsersIds = new List<string>();
                    for (int i = 0; i < usersIds.Count; i++)
                    {
                        List<long> userRoleIds = GetUserRoleIds(groupId, usersIds[i]);
                        if (userRoleIds != null && userRoleIds.Count > 0)
                        {
                            if (roleIds.Any(userRoleIds.Contains))
                            {
                                filteredUsersIds.Add(usersIds[i]);
                            }
                        }
                    }

                    usersIds = filteredUsersIds;
                }
            }

            if (usersIds != null && usersIds.Count > 0)
            {
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
            }

            if (response == null || response.users == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.resp);
            }

            users = Mapper.Map<List<WebAPI.Models.Users.KalturaOTTUser>>(response.users);

            return users;
        }

        public KalturaOTTUser UpdateOTTUser(int groupId, string siteGuid, KalturaOTTUser user)
        {
            GenericResponse<UserResponseObject> response = null;
            UserBasicData userBasicData = Mapper.Map<UserBasicData>(user);
            UserDynamicData userDynamicData = Mapper.Map<UserDynamicData>(user.DynamicData);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.UpdateUser(groupId, siteGuid, userBasicData, userDynamicData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Login. siteGuid: {0}, exception: {1}", siteGuid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            KalturaOTTUser responseUser = Mapper.Map<KalturaOTTUser>(response.Object);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
                res = true;

            return res;
        }

        internal KalturaFavorite InsertUserFavorite(int groupId, string userId, int domainID, string deviceUDID, string mediaType, string mediaId, string extraData)
        {
            Func<GenericResponse<FavoritObject>> insertUserFavoriteFunc = () =>
             Core.Users.Module.InsertUserFavorite(groupId, userId, domainID, deviceUDID, mediaType, mediaId, extraData);

            KalturaFavorite result =
                ClientUtils.GetResponseFromWS<KalturaFavorite, FavoritObject>(insertUserFavoriteFunc);

            return result; ;
        }

        public bool RemoveUserFavorite(int groupId, string userId, int domainID, long[] mediaIDs)
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        public bool SignOut(int groupId, int userId, string ip, string deviceId, KS ks, SerializableDictionary<string, KalturaStringValue> adapterData)
        {
            UserResponseObject response = null;
            Group group = GroupsManager.Instance.GetGroup(groupId);

            try
            {
                var keyValueList = new List<KeyValuePair>();

                if (adapterData != null)
                {
                    keyValueList = adapterData.Select(p => new KeyValuePair { key = p.Key, value = p.Value.value }).ToList();
                }
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.SignOut(groupId, userId.ToString(), string.Empty, ip, deviceId, group.ShouldSupportSingleLogin, keyValueList);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SignOut. UserId: {0}, exception: {1}", userId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.m_RespStatus != ResponseStatus.SessionLoggedOut)
            {
                throw new ClientException((int)response.m_RespStatus, StatusCode.Error.ToString());
            }

            Managers.AuthorizationManager.LogOut(ks);

            return true;
        }

        public WebAPI.Models.Users.KalturaOTTUser ActivateAccount(int groupId, string username, string token)
        {
            GenericResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.ActivateAccount(groupId, username, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaOTTUser user = Mapper.Map<KalturaOTTUser>(response.Object);

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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Item == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        internal KalturaUserAssetsListItem GetItemFromUsersList(int groupId, string userId, string assetId, KalturaUserAssetsListType listType, KalturaUserAssetsListItemType itemType)
        {
            KalturaUserAssetsListItem listItem = null;
            UsersListItemResponse response = null;

            try
            {
                Item item = new Item()
                {
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Item == null)
            {
                throw new ClientException(StatusCode.Error);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Item == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            listItem = Mapper.Map<KalturaUserAssetsListItem>(response.Item);

            return listItem;
        }

        internal KalturaOTTUserListResponse GetUserByExternalID(int groupId, string externalID)
        {
            KalturaOTTUserListResponse listUser = null;
            GenericResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserByExternalID(groupId, externalID, -1);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserDataByCoGuid. Username: {0}, externalID: {1}", groupId, externalID);
                ErrorUtils.HandleWSException(ex);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaOTTUser User;
            User = Mapper.Map<KalturaOTTUser>(response.Object);
            listUser = new KalturaOTTUserListResponse()
            {
                Users = new List<KalturaOTTUser>() { User },
                TotalCount = 1,
            };

            return listUser;
        }

        internal KalturaOTTUserListResponse GetUsersByEmail(int groupId, string email)
        {
            GenericListResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUsersByEmail(groupId, email, -1);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while GetUsersByEmail. group id: {groupId}, email: {email}, ex: {ex}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Objects == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            var listUser = new KalturaOTTUserListResponse()
            {
                Users = Mapper.Map<List<KalturaOTTUser>>(response.Objects),
                TotalCount = response.Objects.Count,
            };

            return listUser;
        }

        internal KalturaOTTUserListResponse GetUserByName(int groupId, string userName)
        {
            GenericResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserByName(groupId, userName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserDataByCoGuid. Username: {0}, userNameFilter: {1}", groupId, userName);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaOTTUser User = Mapper.Map<KalturaOTTUser>(response.Object);
            var listUser = new KalturaOTTUserListResponse()
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                log.ErrorFormat("Exception received while calling users service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        internal List<KalturaUserInterest> GetUserInterests(int groupId, string user)
        {
            List<KalturaUserInterest> list = null;
            UserInterestResponseList response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetUserInterests(groupId, int.Parse(user));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserInterests. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            list = Mapper.Map<List<KalturaUserInterest>>(response.UserInterests);

            return list;
        }

        internal KalturaOTTUser LoginWithDevicePin(int groupId, string udid, string pin)
        {
            GenericResponse<UserResponseObject> response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Domains.Module.LoginWithDevicePIN(groupId, pin, string.Empty, Utils.Utils.GetClientIP(), udid, false, null);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling users service", true, ex);

                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            if (response.Object == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            KalturaOTTUser user = Mapper.Map<KalturaOTTUser>(response.Object);

            return user;
        }

        internal KalturaOTTUserDynamicData SetUserDynamicData(int groupId, string userId, string key, KalturaStringValue value)
        {
            KalturaOTTUserDynamicData response = null;
            bool success = false;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    success = Core.Users.Module.SetUserDynamicData(groupId, userId, key, value.value);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetUserDynamicData. siteGuid: {0}, exception: {1}", userId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (!success)
            {
                throw new ClientException(StatusCode.Error);
            }


            response = UsersMappings.ConvertOTTUserDynamicData(userId, key, value);

            return response;
        }

        internal bool DeleteUserDynamicData(int groupId, long userId, string key)
        {
            Func<ApiObjects.Response.Status> deleteDataFunc = () => Core.Users.Module.DeleteUserDynamicData(groupId, userId, key);
            var response = ClientUtils.GetResponseStatusFromWS(deleteDataFunc);

            return response;
        }

        internal List<KalturaSSOAdapterProfile> GetSSOAdapters(int groupId)
        {
            var response = new SSOAdaptersResponse();
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.GetSSOAdapters(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetSSOAdapters. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.RespStatus.Code != (int)StatusCode.OK)
            {
                log.Error($"Error while GetSSOAdapters. groupID: {groupId}, message: {response.RespStatus.Message}");
                throw new ClientException(response.RespStatus);
            }

            return Mapper.Map<List<KalturaSSOAdapterProfile>>(response.SSOAdapters);
        }

        internal KalturaSSOAdapterProfile InsertSSOAdapter(int groupId, KalturaSSOAdapterProfile kalturaSsoAdapater, int updaterId)
        {
            var response = new SSOAdapterResponse();
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var adapterDetails = Mapper.Map<SSOAdapter>(kalturaSsoAdapater);
                    adapterDetails.GroupId = groupId;
                    response = Core.Users.Module.InsertSSOAdapter(adapterDetails, updaterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertSSOAdapter. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.RespStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Error while InsertSSOAdapter. groupID: {0} message: {1}", groupId, response.RespStatus.Message);
                throw new ClientException(response.RespStatus);
            }

            return Mapper.Map<KalturaSSOAdapterProfile>(response.SSOAdapter);

        }

        internal KalturaSSOAdapterProfile SetSSOAdapter(int groupId, int ssoAdapterId, KalturaSSOAdapterProfile ssoAdapater, int updaterId)
        {
            var response = new SSOAdapterResponse();
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var adapterDetails = Mapper.Map<SSOAdapter>(ssoAdapater);
                    adapterDetails.GroupId = groupId;
                    adapterDetails.Id = ssoAdapterId;
                    response = Core.Users.Module.UpdateSSOAdapter(adapterDetails, updaterId);
                    response.SSOAdapter = adapterDetails;
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateSSOAdapter. groupID: {0}, adapterId:{1}, exception: {2}", groupId, ssoAdapterId, ex);
                ErrorUtils.HandleWSException(ex);
                return null;
            }

            if (response.RespStatus.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Error while UpdateSSOAdapter. groupID: {0} adapterId:{1}", groupId, ssoAdapterId);
                throw new ClientException(response.RespStatus);
            }

            return Mapper.Map<KalturaSSOAdapterProfile>(response.SSOAdapter);
        }

        internal ApiObjects.Response.Status DeleteSSOAdapater(int groupId, int ssoAdapterId, int updaterId)
        {
            var response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Could not delete SSO adapter");
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.DeleteSSOAdapter(groupId, ssoAdapterId, updaterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteSSOAdapater. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                log.ErrorFormat("Error while DeleteSSOAdapater. groupID: {0}, message: {1}", groupId, response.Message);
                throw new ClientException(response);
            }

            return response;
        }

        internal KalturaSSOAdapterProfileInvoke Invoke(int groupId, string intent, List<KalturaKeyValue> extraParams)
        {
            SSOAdapterProfileInvoke response = null;

            try
            {
                var keyValuePairs = Mapper.Map<List<KeyValuePair>>(extraParams);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Users.Module.Invoke(groupId, intent, keyValuePairs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Invoke. groupID: {0}, exception: {1}", groupId, ex);
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

            var profileInvoke = Mapper.Map<KalturaSSOAdapterProfileInvoke>(response);

            return profileInvoke;
        }

        internal KalturaSSOAdapterProfile GenerateSSOAdapaterSharedSecret(int groupId, int ssoAdapterId, int updaterId)
        {
            var response = new SSOAdapterResponse();
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                    response = Core.Users.Module.SetSSOAdapterSharedSecret(groupId, ssoAdapterId, sharedSecret, updaterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertSSOAdapter. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.RespStatus.Code != (int)StatusCode.OK)
            {
                log.ErrorFormat("Error while InsertSSOAdapter. groupID: {0}, message: {1}", groupId, response.RespStatus.Message);
                throw new ClientException(response.RespStatus);
            }

            return Mapper.Map<KalturaSSOAdapterProfile>(response.SSOAdapter);
        }

        internal bool UpdateLastLoginDate(int groupId, string userId)
        {
            var response = false;

            var initializedUser = new User();
            int.TryParse(userId, out int _userId);

            var isUserInitialized = initializedUser.Initialize(_userId, groupId);

            if (isUserInitialized && initializedUser?.m_oBasicData != null)
            {
                response = User.UpdateLoginViaStartSession(groupId, 0, _userId, initializedUser, true);
            }

            if (!response)
            {
                log.Error($"Failed to update last login date for user: {userId}, user init status: {isUserInitialized }, Method: [UpdateLastLoginDate]");
            }

            return response;
        }

        internal ResponseStatus GetUserActivationState(int groupId, long userId)
        {
            return Core.Users.Module.GetUserActivationState(groupId, (int)userId);
        }
    }
}
