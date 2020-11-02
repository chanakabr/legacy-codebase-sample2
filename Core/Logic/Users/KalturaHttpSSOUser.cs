using ApiObjects;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ApiObjects.SSOAdapter;
using APILogic.SSOAdapaterService;
using CachingProvider.LayeredCache;
using SSOAdapaterUser = APILogic.SSOAdapaterService.User;
using SSOAdapterUserType = APILogic.SSOAdapaterService.UserType;
using System.Web;
using APILogic.Users;
using TVinciShared;
using KeyValuePair = ApiObjects.KeyValuePair;
using Newtonsoft.Json;
using ApiLogic.Users;
using ApiObjects.Response;

namespace Core.Users
{
    public class KalturaHttpSSOUser : KalturaUsers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static Dictionary<int, SSOImplementationsResponse> _SSOImplementationCache = new Dictionary<int, SSOImplementationsResponse>();
        private readonly int _GroupId;
        private readonly SSOAdapter _AdapterConfig;
        private readonly ServiceClient _AdapterClient;
        [Obsolete("Use _ImplementedMethodsExtend Instead")]
        private eSSOMethods[] _ImplementedMethods;
        private int[] _ImplementedMethodsExtend;
        private int _AdapterId;

        public KalturaHttpSSOUser(int groupId, SSOAdapter adapterConfig) : base(groupId)
        {
            _GroupId = groupId;
            _AdapterId = adapterConfig.Id.Value;
            _AdapterConfig = adapterConfig;
            _AdapterClient = SSOAdaptersManager.GetSSOAdapterServiceClient(_AdapterConfig.AdapterUrl);

            var implementationsResponse = GetSSOImplementations();

            if (implementationsResponse.ImplementedMethodsExtend == null || implementationsResponse.ImplementedMethodsExtend.Count() == 0)
            {
                _ImplementedMethodsExtend = implementationsResponse.ImplementedMethods.Select(val => (int)val).ToArray();
            }
            else
            {
                _ImplementedMethodsExtend = implementationsResponse.ImplementedMethodsExtend;
            }

            base.ShouldSendWelcomeMail = implementationsResponse.SendWelcomeEmail;
        }

        public override UserResponseObject PreSignIn(ref int siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PerSignIn))
            {
                return base.PreSignIn(ref siteGuid, ref userName, ref password, ref maxFailCount, ref lockMin, ref groupId, ref sessionId, ref ip, ref deviceId, ref preventDoubleLogin, ref keyValueList);
            }

            try
            {
                var preSignInModel = new PreSignInModel
                {
                    UserId = siteGuid,
                    UserName = userName,
                    Password = password,
                    MaxFailCount = maxFailCount,
                    LockMin = lockMin,
                    GroupId = groupId,
                    SessionId = sessionId,
                    IPAddress = ip,
                    DeviceId = deviceId,
                    PreventDoubleLogin = preventDoubleLogin,
                    CustomParams = keyValueList?.ToDictionary(k => k.key, v => v.value),
                };

                var customParamsStr = preSignInModel.CustomParams != null ? string.Concat(preSignInModel.CustomParams.Select(c => c.Key + c.Value)) : string.Empty;
                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, preSignInModel.UserId, preSignInModel.UserName, preSignInModel.Password, customParamsStr);
                _Logger.InfoFormat("Calling sso adapter PreSignIn [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PreSignInAsync(_AdapterId, preSignInModel, signature).ExecuteAndWait();
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PreSignInAsync(_AdapterId, preSignInModel, signature).ExecuteAndWait();
                }

                if (response.SSOResponseStatus.ResponseStatus != eSSOUserResponseStatus.OK)
                {
                    return CreateFromAdapterResponseStatus(response.SSOResponseStatus);
                }

                siteGuid = response.UserId;
                userName = response.Username;
                password = response.Password;

                if (response.Priviliges != null && response.Priviliges.Count > 0)
                {
                    if (HttpContext.Current.Items[Constants.PRIVILIGES] == null)
                    {
                        HttpContext.Current.Items.Add(Constants.PRIVILIGES, response.Priviliges);
                    }
                    else
                    {
                        HttpContext.Current.Items[Constants.PRIVILIGES] = response.Priviliges;
                    }
                }

                return new UserResponseObject { m_RespStatus = ResponseStatus.OK };
            }
            catch (Exception e)
            {
                _Logger.ErrorFormat("Unexpected error during PreSignIn for user:[{0}] group:[{1}], ex:{2}", userName, groupId, e);
                return new UserResponseObject { m_RespStatus = ResponseStatus.InternalError };
            }
        }

        public override void PostSignIn(ref UserResponseObject authenticatedUser, ref List<KeyValuePair> keyValueList)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PostSignIn))
            {
                base.PostSignIn(ref authenticatedUser, ref keyValueList);
                return;
            }

            try
            {
                keyValueList = keyValueList ?? new List<KeyValuePair>();

                var postSignInModel = new PostSignInModel
                {
                    AuthenticatedUser = ConvertUserToSSOUser(authenticatedUser?.m_user),
                    CustomParams = keyValueList.ToDictionary(k => k.key, v => v.value),
                };

                var customParamsStr = string.Concat(postSignInModel.CustomParams.Select(c => c.Key + c.Value));
                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, postSignInModel.AuthenticatedUser?.Id, postSignInModel.AuthenticatedUser?.Username, postSignInModel.AuthenticatedUser?.Email, customParamsStr);
                var response = _AdapterClient.PostSignInAsync(_AdapterId, postSignInModel, signature).ExecuteAndWait();

                _Logger.InfoFormat("Calling sso adapter PostSignIn [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PostSignInAsync(_AdapterId, postSignInModel, signature).ExecuteAndWait();
                }

                if (response.AdapterStatus != AdapterStatusCode.OK)
                {
                    authenticatedUser.ExternalCode = response.SSOResponseStatus.ExternalCode;
                    authenticatedUser.ExternalMessage = response.SSOResponseStatus.ExternalMessage;
                    authenticatedUser.m_RespStatus = (ResponseStatus)(int)response.SSOResponseStatus.ResponseStatus;
                }

                // map the response to the user data;
                ExtendUserWithSSOUser(response.User, ref authenticatedUser.m_user);
            }
            catch (Exception e)
            {
                _Logger.ErrorFormat("Unexpected error during PostSignIn for user:[{0}] group:[{1}], ex:{2}", authenticatedUser.m_user != null ? authenticatedUser.m_user.m_oBasicData.m_sUserName : "user is null", _GroupId, e);
            }

        }

        public override UserResponseObject PreGetUserData(string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PreGetUserData))
            {
                return base.PreGetUserData(sSiteGUID, ref keyValueList, userIP);
            }

            try
            {
                var userId = int.Parse(sSiteGUID);
                var customParams = keyValueList.ToDictionary(k => k.key, v => v.value);
                var customParamsSrt = string.Concat(keyValueList.Select(kv => kv.key + kv.value));
                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, userId, userIP, customParamsSrt);

                _Logger.InfoFormat("Calling sso adapter PreGetUserData [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PreGetUserDataAsync(_AdapterId, userId, userIP, customParams, signature).ExecuteAndWait();
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PreGetUserDataAsync(_AdapterId, userId, userIP, customParams, signature).ExecuteAndWait();
                }

                if (response.SSOResponseStatus.ResponseStatus != eSSOUserResponseStatus.OK)
                {
                    return CreateFromAdapterResponseStatus(response.SSOResponseStatus);
                }

                return new UserResponseObject
                {
                    m_RespStatus = ResponseStatus.OK,
                    m_user = ConvertSSOUserToUser(response.User),
                };
            }
            catch (Exception e)
            {
                _Logger.ErrorFormat("Unexpected error during PostSignIn for userId:[{0}] userIP:[{1}], ex:{2}", sSiteGUID, userIP, e);
                return new UserResponseObject { m_RespStatus = ResponseStatus.InternalError };
            }

        }

        public override void PostGetUserData(ref UserResponseObject userResponse, string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PostGetUserData))
            {
                base.PostGetUserData(ref userResponse, sSiteGUID, ref keyValueList, userIP);
                return;
            }

            try
            {
                var userData = ConvertUserToSSOUser(userResponse.m_user);
                var customParams = keyValueList.ToDictionary(k => k.key, v => v.value);

                var customParamsStr = string.Concat(customParams.Select(c => c.Key + c.Value));
                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, userData?.Id, userData?.Username, userData?.Email, customParamsStr);

                _Logger.InfoFormat("Calling sso adapter PostGetUserData [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PostGetUserDataAsync(_AdapterId, userData, customParams, signature).ExecuteAndWait();
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PostGetUserDataAsync(_AdapterId, userData, customParams, signature).ExecuteAndWait();
                }

                if (response.AdapterStatus != AdapterStatusCode.OK)
                {
                    userResponse.ExternalCode = response.SSOResponseStatus.ExternalCode;
                    userResponse.ExternalMessage = response.SSOResponseStatus.ExternalMessage;
                    userResponse.m_RespStatus = (ResponseStatus)(int)response.SSOResponseStatus.ResponseStatus;
                }

                // if all okay return converted user
                userResponse.m_RespStatus = (ResponseStatus)(int)response.SSOResponseStatus.ResponseStatus;
                userResponse.m_user = ConvertSSOUserToUser(response.User);
            }
            catch (Exception e)
            {
                if (userResponse != null && userResponse.m_user != null && userResponse.m_user.m_oBasicData != null)
                {
                    _Logger.ErrorFormat("Unexpected error during PostSignIn for user:[{0}] group:[{1}], ex:[{2}]", userResponse.m_user.m_oBasicData.m_sUserName, userResponse.m_user.GroupId, e);
                }
                else
                {
                    _Logger.ErrorFormat("Unexpected error during PostSignIn for group:[{0}], ex:{1}", _GroupId, e);
                }
                // TODO: should we throw an error here ? or return the normal user data ? 
            }

        }

        private SSOImplementationsResponse GetSSOImplementations()
        {
            SSOImplementationsResponse implementationsResponse = null;
            var key = LayeredCacheKeys.GetSSOAdapaterImplementationsKey(_AdapterId);
            var cacheResult = LayeredCache.Instance.Get(
                key,
                ref implementationsResponse,
                GetImplementationsFromAdapater,
                new Dictionary<string, object>() { { "adapterId", _AdapterId } },
                _GroupId,
                LayeredCacheConfigNames.GET_SSO_ADAPATER_BY_GROUP_ID_CACHE_CONFIG_NAME,
                new List<string>() { LayeredCacheKeys.GetSSOAdapaterImplementationsInvalidationKey(_GroupId, _AdapterId) });

            if (!cacheResult || implementationsResponse == null)
            {
                _Logger.ErrorFormat("Error getting GetImplementationsFromAdapater from http sso adapter id:[{0}], groupId:[{1}]. setting default implementation settings as fallback", _AdapterId, _GroupId);
                throw new Exception("Error getting GetImplementationsFromAdapater from http sso adapter");
            }

            return implementationsResponse;
        }

        private Tuple<SSOImplementationsResponse, bool> GetImplementationsFromAdapater(Dictionary<string, object> arg)
        {
            try
            {
                var adapterId = (int)arg["adapterId"];
                var implementationsResponse = _AdapterClient.GetConfigurationAsync(adapterId).ExecuteAndWait();
                if (!ValidateConfigurationIsSet(implementationsResponse.Status))
                {
                    implementationsResponse = _AdapterClient.GetConfigurationAsync(adapterId).ExecuteAndWait();
                }

                return new Tuple<SSOImplementationsResponse, bool>(implementationsResponse, true);
            }
            catch (Exception e)
            {
                _Logger.Error("Error getting GetImplementationsFromAdapater from http sso adapter", e);
                return new Tuple<SSOImplementationsResponse, bool>(null, false);
            }

        }

        private static SSOAdapaterUser ConvertUserToSSOUser(User userData)
        {
            if (userData == null) { return null; }
            var user = new SSOAdapaterUser
            {
                Id = int.Parse(userData.m_sSiteGUID),
                ExternalId = userData.m_oBasicData.m_CoGuid,
                HouseholdID = userData.m_domianID,
                IsHouseholdMaster = userData.m_isDomainMaster,
                Username = userData.m_oBasicData.m_sUserName,
                FirstName = userData.m_oBasicData.m_sFirstName,
                LastName = userData.m_oBasicData.m_sLastName,
                Email = userData.m_oBasicData.m_sEmail,
                City = userData.m_oBasicData.m_sCity,
                CountryId = userData.m_oBasicData.m_Country != null ? userData.m_oBasicData.m_Country.m_nObjecrtID : (int?)null,
                Zip = userData.m_oBasicData.m_sZip,
                Phone = userData.m_oBasicData.m_sPhone,
                Address = userData.m_oBasicData.m_sAddress,
                UserState = (eUserState)(int)userData.m_eUserState,
                SuspensionState = (eHouseholdSuspensionState)(int)userData.m_eSuspendState,
                UserType = new SSOAdapterUserType
                {
                    Id = userData.m_oBasicData.m_UserType.ID ?? 0,
                    Description = userData.m_oBasicData.m_UserType.Description
                }
            };

            if (userData.m_oDynamicData?.m_sUserData != null)
            {
                user.DynamicData = userData.m_oDynamicData.m_sUserData.ToDictionary(k => k.m_sDataType, k => k.m_sValue);
            }


            return user;
        }

        private static User ConvertSSOUserToUser(SSOAdapaterUser userData)
        {
            var user = new User();
            ExtendUserWithSSOUser(userData, ref user);
            return user;
        }

        private static void ExtendUserWithSSOUser(SSOAdapaterUser userData, ref User ioUser)
        {
            ioUser = ioUser ?? new User();

            UserDynamicDataContainer[] dynamicData = null;
            if (userData.DynamicData != null)
            {
                dynamicData = userData.DynamicData?.Select(kv => new UserDynamicDataContainer { m_sDataType = kv.Key, m_sValue = kv.Value }).ToArray();
            }
            else
            {
                dynamicData = new UserDynamicDataContainer[] { };
            }

            ioUser.Id = userData.Id;
            ioUser.m_domianID = userData.HouseholdID ?? 0;
            ioUser.m_isDomainMaster = userData.IsHouseholdMaster ?? false;
            ioUser.m_eUserState = (UserState)(int)userData.UserState;
            ioUser.m_eSuspendState = (DomainSuspentionStatus)(int)userData.SuspensionState;

            //m_oBasicData
            ioUser.m_oBasicData = ioUser.m_oBasicData ?? new UserBasicData();//only if null
            ioUser.m_oBasicData.m_CoGuid = ioUser.m_oBasicData.m_CoGuid ?? userData.ExternalId;
            ioUser.m_oBasicData.m_sUserName = ioUser.m_oBasicData.m_sUserName ?? userData.Username;
            ioUser.m_oBasicData.m_sFirstName = ioUser.m_oBasicData.m_sFirstName ?? userData.FirstName;
            ioUser.m_oBasicData.m_sLastName = ioUser.m_oBasicData.m_sLastName ?? userData.LastName;
            ioUser.m_oBasicData.m_sEmail = ioUser.m_oBasicData.m_sEmail ?? userData.Email;
            ioUser.m_oBasicData.m_sCity = ioUser.m_oBasicData.m_sCity ?? userData.City;
            if (ioUser.m_oBasicData.m_Country == null)
            {
                ioUser.m_oBasicData.m_Country = userData.CountryId.HasValue && userData.CountryId.Value > 0 ? new Country { m_nObjecrtID = userData.CountryId.Value } : null;   //BEO-7091
            }
            ioUser.m_oBasicData.m_sZip = ioUser.m_oBasicData.m_sZip ?? userData.Zip;
            ioUser.m_oBasicData.m_sPhone = ioUser.m_oBasicData.m_sPhone ?? userData.Phone;
            ioUser.m_oBasicData.m_sAddress = ioUser.m_oBasicData.m_sAddress ?? userData.Address;
            ioUser.m_oBasicData.m_UserType = new ApiObjects.UserType
            {
                ID = userData.UserType.Id > 0 ? userData.UserType.Id : (int?)null,  //BEO-7091
                Description = userData.UserType.Description
            };

            //m_oDynamicData
            ioUser.m_oDynamicData = ioUser.m_oDynamicData ?? new UserDynamicData();
            ioUser.m_oDynamicData.m_sUserData = ioUser.m_oDynamicData.m_sUserData ?? dynamicData;
            ioUser.m_oDynamicData.UserId = ioUser.m_oDynamicData.UserId == default ? userData.Id : ioUser.m_oDynamicData.UserId;
        }

        private bool ValidateConfigurationIsSet(AdapterStatusCode responseStatus)
        {
            if (responseStatus == AdapterStatusCode.NoConfigurationFound)
            {
                SetAdapaterConfiguration(_AdapterClient, _AdapterConfig);
                return false;
            }
            return true;
        }

        public static void SetAdapaterConfiguration(ServiceClient client, SSOAdapter adapter)
        {
            var configDict = adapter.Settings.ToDictionary(k => k.Key, v => v.Value);
            var settingsString = string.Concat(configDict.Select(kv => kv.Key + kv.Value));
            var signature = GenerateSignature(adapter.SharedSecret, adapter.Id, adapter.GroupId, settingsString);

            _Logger.DebugFormat("SSO Adapater [{0}] returned with no configuration. sending configuration: [{1}]", adapter.Name, string.Concat(configDict.Select(kv => string.Format("[{0}|{1}], ", kv.Key, kv.Value))));
            client.SetConfigurationAsync(adapter.Id.Value, adapter.GroupId, configDict, signature).ExecuteAndWait();
        }

        private static string GenerateSignature(string secret, params object[] values)
        {
            var signatureStr = string.Concat(values);
            var signatureSHA1 = TVinciShared.EncryptUtils.HashSHA1(signatureStr);
            var signatureAES = TVinciShared.EncryptUtils.AesEncrypt(secret, signatureSHA1);
            var signature = Convert.ToBase64String(signatureAES);
            return signature;
        }

        private static UserResponseObject CreateFromAdapterResponseStatus(SSOResponseStatus ssoResponseStatus)
        {
            return new UserResponseObject
            {
                m_RespStatus = (ResponseStatus)(int)ssoResponseStatus.ResponseStatus,
                ExternalCode = ssoResponseStatus.ExternalCode,
                ExternalMessage = ssoResponseStatus.ExternalMessage,
            };
        }


        public override SSOAdapterProfileInvoke Invoke(int groupId, string intent, List<KeyValuePair> keyValueList)
        {
            try
            {
                _Logger.Info($"Calling SSO adapter Invoke [{_AdapterConfig.Name}], group:[{_GroupId}]");

                var ssoAdapterProfileInvokeModel = new SSOAdapterProfileInvokeModel()
                {
                    Intent = intent,
                    AdapterData = keyValueList?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray()
                };

                var adapterResponse = _AdapterClient.InvokeAsync(_AdapterId, ssoAdapterProfileInvokeModel).ExecuteAndWait();

                if (!ValidateConfigurationIsSet(adapterResponse.AdapterStatus))
                {
                    adapterResponse = _AdapterClient.InvokeAsync(_AdapterId, ssoAdapterProfileInvokeModel).ExecuteAndWait();
                }

                var status = CreateFromAdapterResponseStatus(adapterResponse.SSOResponseStatus);

                if (adapterResponse.SSOResponseStatus.ResponseStatus != eSSOUserResponseStatus.OK)
                {
                    return new SSOAdapterProfileInvoke
                    {
                        Status = new ApiObjects.Response.Status(status.ExternalCode),
                        Code = adapterResponse.Code,
                        Message = adapterResponse.Message
                    };
                }

                var _adapterResponse = new SSOAdapterProfileInvoke
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, status.ExternalMessage),
                    Code = adapterResponse?.Code,
                    Message = adapterResponse?.Message,
                    AdapterData = new Dictionary<string, string>()
                };

                if (adapterResponse.AdapterData != null && adapterResponse.AdapterData.Length > 0)
                {
                    _adapterResponse.AdapterData.TryAddRange(adapterResponse.AdapterData.ToDictionary(x => x.Key, x => x.Value));
                }

                return _adapterResponse;
            }
            catch (Exception ex)
            {
                _Logger.Error($"Unexpected error during Invoke for group:[{groupId}], ex:{ex}");
                return new SSOAdapterProfileInvoke()
                {
                    Status = new ApiObjects.Response.Status(eResponseStatus.Error)
                };
            }
        }

        public override UserResponseObject PreSignOut(ref int siteGuid, ref int groupId, ref string sessionId, ref string ip, ref string deviceUdid, ref List<KeyValuePair> keyValueList)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PreSignOut))
            {
                return base.PreSignOut(ref siteGuid, ref groupId, ref sessionId, ref ip, ref deviceUdid, ref keyValueList);
            }

            try
            {
                var domainResponse = Domains.Module.GetDomainByUser(groupId, siteGuid.ToString());
                var domainId = domainResponse.Status.IsOkStatusCode() && domainResponse.Domain != null ? domainResponse.Domain.m_nDomainID : 0;
                var preSignOutModel = new PreSignOutModel
                {
                    UserId = siteGuid,
                    HouseholdId = domainId,
                    DeviceUdid = deviceUdid,
                    AdapterData = keyValueList?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray()
                };

                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, preSignOutModel.UserId);
                _Logger.Info($"Calling SSO adapter PreSignOut [{_AdapterConfig.Name}], group:[{_GroupId}]");

                _Logger.Debug($"[PreSignOut] Adapter model object: {JsonConvert.SerializeObject(preSignOutModel)}, Signature: {signature}");

                var response = _AdapterClient.PreSignOutAsync(_AdapterId, preSignOutModel, signature).ExecuteAndWait();
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PreSignOutAsync(_AdapterId, preSignOutModel, signature).ExecuteAndWait();
                }

                if (response.SSOResponseStatus.ResponseStatus != eSSOUserResponseStatus.OK)
                {
                    return CreateFromAdapterResponseStatus(response.SSOResponseStatus);
                }

                return new UserResponseObject { m_RespStatus = ResponseStatus.OK };
            }
            catch (Exception ex)
            {
                _Logger.Error($"Unexpected error during PreSignOut for user:[{siteGuid}] group:[{groupId}], ex:{ex}");
                return new UserResponseObject { m_RespStatus = ResponseStatus.InternalError };
            }
        }

        public override void PostSignOut(ref UserResponseObject userResponse, int siteGuid, int groupId, string sessionId, string ip, string deviceUdid, ref List<KeyValuePair> keyValueList)
        {
            if (!_ImplementedMethodsExtend.Contains((int)Api.eSSOMethodsExtend.PostSignOut))
            {
                base.PostSignOut(ref userResponse, siteGuid, groupId, sessionId, ip, deviceUdid, ref keyValueList);
                return;
            }

            try
            {
                var domainResponse = Core.Domains.Module.GetDomainByUser(groupId, siteGuid.ToString());
                var domainId = domainResponse.Status.IsOkStatusCode() && domainResponse.Domain != null ? domainResponse.Domain.m_nDomainID : 0;

                var postSignOutModel = new PostSignOutModel
                {
                    UserId = siteGuid,
                    DeviceUdid = deviceUdid,
                    HouseholdId = domainId,
                    AuthenticatedUser = new SSOAdapaterUser() { HouseholdID = domainId, Id = siteGuid },
                    AdapterData = keyValueList?.Select(x => new KeyValue { Key = x.key, Value = x.value }).ToArray()
                };

                var signature = GenerateSignature(_AdapterConfig.SharedSecret, _AdapterId, postSignOutModel.UserId);
                _Logger.Debug($"[PostSignOut] Adapter model object: {JsonConvert.SerializeObject(postSignOutModel)}, Signature: {signature}");

                var response = _AdapterClient.PostSignOutAsync(_AdapterId, postSignOutModel, signature).ExecuteAndWait();

                _Logger.Info($"Calling SSO adapter PostSignOut [{_AdapterConfig.Name}], group:[{_GroupId}]");

                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PostSignOutAsync(_AdapterId, postSignOutModel, signature).ExecuteAndWait();
                }

                if (response.AdapterStatus != AdapterStatusCode.OK)
                {
                    _Logger.Error($"Failed to PostSignOut, Status: {response.SSOResponseStatus} for user: [{siteGuid}] group: [{groupId}]");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"Unexpected error during PostSignOut for user:[{siteGuid}] group:[{groupId}], ex:{ex}");
            }
        }
    }
}