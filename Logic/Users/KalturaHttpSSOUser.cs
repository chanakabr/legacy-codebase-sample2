using ApiObjects;
using ApiObjects.Response;
using ConfigurationManager;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.SSOAdapter;
using APILogic.SSOAdapaterService;
using SSOAdapaterUser = APILogic.SSOAdapaterService.User;
using SSOAdapterUserType = APILogic.SSOAdapaterService.UserType;

namespace Core.Users
{
    public class KalturaHttpSSOUser : KalturaUsers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly int _GroupId;
        private readonly SSOAdapter _AdapterConfig;
        private readonly ServiceClient _AdapterClient;
        private eSSOMethods[] _ImplementedMethods;
        private int _AdapterId;

        public KalturaHttpSSOUser(int groupId, SSOAdapter adapterConfig) : base(groupId)
        {
            _GroupId = groupId;
            _AdapterId = adapterConfig.Id.Value;
            _AdapterConfig = adapterConfig;
            _AdapterClient = new ServiceClient(string.Empty, _AdapterConfig.AdapterUrl);
            if (!string.IsNullOrEmpty(_AdapterConfig.AdapterUrl))
            {
                _AdapterClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(_AdapterConfig.AdapterUrl);
            }

            var implementationsResponse = _AdapterClient.GetConfiguration(_AdapterId);
            if (!ValidateConfigurationIsSet(implementationsResponse.Status))
            {
                implementationsResponse = _AdapterClient.GetConfiguration(_AdapterId);
            }

            _ImplementedMethods = implementationsResponse.ImplementedMethods;
            base.ShouldSendWelcomeMail = implementationsResponse.SendWelcomeEmail;
        }



        // TODO: where is the base operatorID CTOR ?
        //public KalturaHttpSSOUser(int groupId, int operatorID) : base(groupId, operatorID) { }

        public override UserResponseObject PreSignIn(ref int siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            if (!_ImplementedMethods.Contains(eSSOMethods.PerSignIn))
                return base.PreSignIn(ref siteGuid, ref userName, ref password, ref maxFailCount, ref lockMin, ref groupId, ref sessionId, ref ip, ref deviceId, ref preventDoubleLogin, ref keyValueList);

            try
            {
                var preSignInModel = new PreSignInModel
                {
                    UserId = siteGuid,
                    UserName = userName,
                    Password = password,
                    // TODO: Remove from pre sign in 
                    MaxFailCount = maxFailCount,
                    // TODO: Remove from pre sign in 
                    LockMin = lockMin,
                    GroupId = groupId,
                    SessionId = sessionId,
                    IPAddress = ip,
                    DeviceId = deviceId,
                    // TODO: Remove from pre sign in 
                    PreventDoubleLogin = preventDoubleLogin,
                    CustomParams = keyValueList.ToDictionary(k => k.key, v => v.value),
                };

                // TODO: signature from object
                var customParamsStr = string.Concat(preSignInModel.CustomParams.Select(c => c.Key + c.Value));
                var signature = GenerateSignature(_AdapterId, preSignInModel.UserId, preSignInModel.UserName, preSignInModel.Password, customParamsStr);
                _Logger.InfoFormat("Calling sso adapter PreSignIn [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PreSignIn(_AdapterId, preSignInModel, signature);
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PreSignIn(_AdapterId, preSignInModel, signature);
                }

                if (response.SSOResponseStatus.ResponseStatus != eSSOUserResponseStatus.OK)
                {
                    return CreateFromAdapterResponseStatus(response.SSOResponseStatus);
                }

                siteGuid = response.UserId;
                userName = response.Username;
                password = response.Password;
                // TODO: ask ira what are the other ref params used for...
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
            if (!_ImplementedMethods.Contains(eSSOMethods.PostSignIn))
                base.PostSignIn(ref authenticatedUser, ref keyValueList);


            try
            {
                var postSignInModel = new PostSignInModel
                {
                    AuthenticatedUser = ConvertUserToSSOUser(authenticatedUser.m_user),
                    CustomParams = keyValueList.ToDictionary(k => k.key, v => v.value),
                };

                // TODO: signature from object
                var customParamsStr = string.Concat(postSignInModel.CustomParams.Select(c => c.Key + c.Value));
                var signature = GenerateSignature(_AdapterId, postSignInModel.AuthenticatedUser.Id, postSignInModel.AuthenticatedUser.Username, postSignInModel.AuthenticatedUser.Email, customParamsStr);
                var response = _AdapterClient.PostSignIn(_AdapterId, postSignInModel, signature);

                _Logger.InfoFormat("Calling sso adapter PostSignIn [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PostSignIn(_AdapterId, postSignInModel, signature);
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
                _Logger.ErrorFormat("Unexpected error during PostSignIn for user:[{0}] group:[{1}], ex:{2}", authenticatedUser?.m_user?.m_oBasicData.m_sUserName, authenticatedUser?.m_user?.GroupId, e);
                // TODO: should we throw an error here ? or return the normal user data ? 
            }

        }

        public override UserResponseObject PreGetUserData(string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP)
        {
            if (!_ImplementedMethods.Contains(eSSOMethods.PreGetUserData))
                return base.PreGetUserData(sSiteGUID, ref keyValueList, userIP);

            try
            {
                var userId = int.Parse(sSiteGUID);
                var customParams = keyValueList.ToDictionary(k => k.key, v => v.value);
                var customParamsSrt = string.Concat(keyValueList.Select(kv => kv.key + kv.value));
                var signature = GenerateSignature(_AdapterId, userId, userIP, customParamsSrt);

                _Logger.InfoFormat("Calling sso adapter PreGetUserData [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PreGetUserData(_AdapterId, userId, userIP, customParams, signature);
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PreGetUserData(_AdapterId, userId, userIP, customParams, signature);
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
            if (!_ImplementedMethods.Contains(eSSOMethods.PostGetUserData))
                base.PostGetUserData(ref userResponse, sSiteGUID, ref keyValueList, userIP);

            try
            {
                var userData = ConvertUserToSSOUser(userResponse.m_user);
                var customParams = keyValueList.ToDictionary(k => k.key, v => v.value);

                //TODO: signature from object
                var customParamsStr = string.Concat(customParams.Select(c => c.Key + c.Value));
                var signature = GenerateSignature(_AdapterId, userData.Id, userData.Username, userData.Email, customParamsStr);

                _Logger.InfoFormat("Calling sso adapter PostGetUserData [{0}], group:[{1}]", _AdapterConfig.Name, _GroupId);
                var response = _AdapterClient.PostGetUserData(_AdapterId, userData, customParams, signature);
                if (!ValidateConfigurationIsSet(response.AdapterStatus))
                {
                    response = _AdapterClient.PostGetUserData(_AdapterId, userData, customParams, signature);
                }

                if (response.AdapterStatus != AdapterStatusCode.OK)
                {
                    userResponse.ExternalCode = response.SSOResponseStatus.ExternalCode;
                    userResponse.ExternalMessage = response.SSOResponseStatus.ExternalMessage;
                    userResponse.m_RespStatus = (ResponseStatus)(int)response.SSOResponseStatus.ResponseStatus;
                }

                // map the response to the user data;
                userResponse.m_user = null; //TODO: ask Ira\TanTan why automapper is not used here ? should this code be elsewhere ?  
            }
            catch (Exception e)
            {
                _Logger.ErrorFormat("Unexpected error during PostSignIn for user:[{0}] group:[{1}], ex:[{2}]", userResponse?.m_user?.m_oBasicData.m_sUserName, userResponse?.m_user?.GroupId, e);
                // TODO: should we throw an error here ? or return the normal user data ? 
            }

        }


        private static SSOAdapaterUser ConvertUserToSSOUser(User userData)
        {
            var user = new SSOAdapaterUser();
            user.Id = (int)userData.Id;
            user.ExternalId = userData.m_oBasicData.m_CoGuid;
            user.HouseholdID = userData.m_domianID;
            user.IsHouseholdMaster = userData.m_isDomainMaster;
            user.Username = userData.m_oBasicData.m_sUserName;
            user.FirstName = userData.m_oBasicData.m_sFirstName;
            user.LastName = userData.m_oBasicData.m_sLastName;
            user.Email = userData.m_oBasicData.m_sEmail;
            user.City = userData.m_oBasicData.m_sCity;
            user.CountryId = userData.m_oBasicData.m_Country?.m_nObjecrtID;
            user.Zip = userData.m_oBasicData.m_sZip;
            user.Phone = userData.m_oBasicData.m_sPhone;
            user.Address = userData.m_oBasicData.m_sAddress;
            user.UserState = (eUserState)(int)userData.m_eUserState;
            user.DynamicData = userData.m_oDynamicData.m_sUserData.ToDictionary(k => k.m_sDataType, k => k.m_sValue);
            user.SuspensionState = (eHouseholdSuspensionState)(int)userData.m_eSuspendState;
            user.UserType = new SSOAdapterUserType();
            user.UserType.Id = userData.m_oBasicData.m_UserType.ID ?? 0;
            user.UserType.Description = userData.m_oBasicData.m_UserType.Description;
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
            var dynamicData = userData.DynamicData.Select(kv => new UserDynamicDataContainer { m_sDataType = kv.Key, m_sValue = kv.Value }).ToArray();

            ioUser.Id = userData.Id;
            ioUser.m_oBasicData = new UserBasicData
            {
                m_CoGuid = userData.ExternalId,
                m_sUserName = userData.Username,
                m_sFirstName = userData.FirstName,
                m_sLastName = userData.LastName,
                m_sEmail = userData.Email,
                m_sCity = userData.City,
                m_Country = new Country { m_nObjecrtID = userData.CountryId ?? 0 },
                m_sZip = userData.Zip,
                m_sPhone = userData.Phone,
                m_sAddress = userData.Address,
                m_UserType = new ApiObjects.UserType
                {
                    ID = userData.UserType.Id,
                    Description = userData.UserType.Description,
                },
            };
            ioUser.m_domianID = userData.HouseholdID ?? 0;
            ioUser.m_isDomainMaster = userData.IsHouseholdMaster ?? false;
            ioUser.m_eUserState = (UserState)(int)userData.UserState;
            ioUser.m_oDynamicData = new UserDynamicData
            {
                m_sUserData = dynamicData,
                UserId = userData.Id,
                // TODO: ask ira if this is important to set
                //GroupId = 0,
                //Id = 0, 

            };
            ioUser.m_eSuspendState = (DomainSuspentionStatus)(int)userData.SuspensionState;
        }

        private bool ValidateConfigurationIsSet(AdapterStatusCode responseStatus)
        {
            if (responseStatus == AdapterStatusCode.NoConfigurationFound)
            {

                var configDict = _AdapterConfig.Settings.ToDictionary(k => k.Key, v => v.Value);
                var settingsString = string.Concat(configDict.Select(kv => kv.Key + kv.Value));
                var signature = GenerateSignature(_AdapterId, _GroupId, settingsString);


                _Logger.DebugFormat("SSO Adapater [{0}] returned with no configuration. sending configuration: [{1}]", _AdapterConfig.Name, string.Concat(configDict.Select(kv => string.Format("[{0}|{1}], ", kv.Key, kv.Value))));
                _AdapterClient.SetConfiguration(_AdapterId, _GroupId, configDict, signature);
                return false;
            }
            return true;
        }

        private string GenerateSignature(params object[] values)
        {
            var signatureStr = string.Concat(values);
            var signatureSHA1 = TVinciShared.EncryptUtils.HashSHA1(signatureStr);
            var signatureAES = TVinciShared.EncryptUtils.AesEncrypt(_AdapterConfig.SharedSecret, signatureSHA1);
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

    }
}
