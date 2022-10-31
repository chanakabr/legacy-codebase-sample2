using ApiLogic.Users;
using ApiLogic.Users.Security;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace Core.Users
{
    public class KalturaUsers : KalturaBaseUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string DEFAULT_USER_CANNOT_BE_DELETED = "Default user cannot be deleted";
        private const string EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED = "Exclusive master user cannot be deleted";
        private const string HOUSEHOLD_NOT_INITIALIZED = "Household not initialized";
        private const string USER_NOT_EXISTS_IN_DOMAIN = "User not exists in domain";

        public bool ShouldSubscribeNewsLetter { get; set; }
        public bool ShouldCreateDefaultRules { get; set; }
        public bool ShouldSendWelcomeMail { get; set; }

        public KalturaUsers(Int32 groupId)
            : base(groupId)
        {
            // default Tvinci users features
            this.ShouldSubscribeNewsLetter = false;
            this.ShouldCreateDefaultRules = false;
            this.ShouldSendWelcomeMail = false;
        }

        public override bool PreAddDomain(ref UserResponseObject userResponse, ref User user, ref string username, ref int userId, ref DomainInfo domainInfo, ref List<KeyValuePair> keyValueList) { return true; }

        internal override bool MidAddDomain(ref UserResponseObject userResponse, User user, string username, int userId, DomainInfo domainInfo)
        {
            bool succeded = false;
            Core.Users.DomainResponseObject domainResponse = null;
            bool isSus = DAL.DomainDal.IsSingleDomainEnvironment(GroupId);

            //check domain type 
            if (isSus)
            {
                // backward compatibility - domainInfo should not be null for new clients

                // SUS - add new domain
                domainResponse = AddNewDomain(username, userId, GroupId);
                if (domainResponse != null && domainResponse.m_oDomain != null)
                    user.m_domianID = domainResponse.m_oDomain.m_nDomainID;

                if (domainResponse.m_oDomainResponseStatus != DomainResponseStatus.OK)
                    userResponse.Initialize(ResponseStatus.UserWithNoDomain, user);
                else
                {
                    userResponse.Initialize(ResponseStatus.OK, user);
                    succeded = true;
                }
            }
            else
            {
                if (domainInfo == null)
                {
                    // MUS without domainInfo - creation of domain is not dealt at this flow.
                    userResponse.Initialize(ResponseStatus.UserWithNoDomain, user);
                    succeded = true;
                }
                else
                {
                    // MUS
                    TvinciDomain domain = new TvinciDomain(GroupId);
                    switch (domainInfo.AddDomainType)
                    {
                        case DomainInfo.eAddToDomainType.CreateNewDomain:

                            //add a new domain
                            domainResponse = domain.AddDomain(username + "/Domain", username + "/Domain", userId, domainInfo.GroupId, domainInfo.DomainCoGuid, null);

                            if (domainResponse == null || domainResponse.m_oDomainResponseStatus != DomainResponseStatus.OK)
                            {
                                // Error adding to domain
                                userResponse = new UserResponseObject();
                                userResponse.Initialize(ResponseStatus.UserWithNoDomain, user);
                            }
                            else
                                succeded = true;
                            break;

                        case DomainInfo.eAddToDomainType.AddToExistingDomain:
                            // add a user to existing domain

                            domainResponse = domain.AddUserToDomain(GroupId, domainInfo.DomainId, userId, domainInfo.DomainMasterId, false);

                            if (domainResponse == null || domainResponse.m_oDomainResponseStatus != DomainResponseStatus.OK)
                            {
                                // Error join to domain
                                userResponse = new UserResponseObject();
                                userResponse.Initialize(ResponseStatus.UserWithNoDomain, user);
                            }
                            else
                                succeded = true;
                            break;

                        case DomainInfo.eAddToDomainType.DontAddDomain:
                        default:

                            userResponse.Initialize(ResponseStatus.UserWithNoDomain, user);
                            break;
                    }
                }
            }


            return succeded;
        }

        public override void PostAddDomain(bool addDomainPassed, ref UserResponseObject userResponse, User user, string username, int userId, DomainInfo domainInfo, ref List<KeyValuePair> keyValueList) { }

        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            return new UserResponseObject();
        }

        /// <summary>
        ///  performs SignIn by siteguid or by username, password
        /// </summary>
        /// <param name="siteGuid"></param>
        /// <param name="maxFailCount"></param>
        /// <param name="lockMin"></param>
        /// <param name="groupId"></param>
        /// <param name="sessionId"></param>
        /// <param name="ip"></param>
        /// <param name="deviceId"></param>
        /// <param name="preventDoubleLogin"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal override UserResponseObject MidSignIn(int siteGuid, string userName, string password, int maxFailCount, int lockMin, int groupId, string sessionId, string ip, string deviceId, bool preventDoubleLogin)
        {
            var userResponseObject = new UserResponseObject();
            int oldSiteGuid = siteGuid;
            bool isGracePeriod = false;

            var userStatus = GetUserActivationState(ref userName, ref siteGuid, ref isGracePeriod);
            var responseStatus = Utils.MapToResponseStatus(userStatus);
            if (responseStatus != ResponseStatus.OK && responseStatus != ResponseStatus.UserSuspended)
            {
                userResponseObject.m_user = responseStatus == ResponseStatus.InternalError || responseStatus == ResponseStatus.UserDoesNotExist
                    ? null
                    : new User(groupId, siteGuid);
                userResponseObject.m_RespStatus = responseStatus;

                if (responseStatus == ResponseStatus.UserWithNoDomain)
                {
                    var success = MidAddDomain(ref userResponseObject, userResponseObject.m_user, userName, siteGuid, new DomainInfo(groupId));
                    if (!success) return userResponseObject;
                }
                else
                {
                    return userResponseObject;
                }
            }

            if (oldSiteGuid == 0)
            {
                // check username and password
                userResponseObject = User.SignIn(userName, password, maxFailCount, lockMin, groupId, sessionId, ip, deviceId, preventDoubleLogin);
                if (userResponseObject != null && userResponseObject.m_user != null)
                {
                    userResponseObject.m_user.IsActivationGracePeriod = isGracePeriod;
                }
                return userResponseObject;
            }
            else if (oldSiteGuid == siteGuid)
            {
                // validate siteguid received is legal
                userResponseObject = User.SignIn(siteGuid, maxFailCount, lockMin, groupId, sessionId, ip, deviceId, preventDoubleLogin);
                if (userResponseObject != null && userResponseObject.m_user != null)
                {
                    userResponseObject.m_user.IsActivationGracePeriod = isGracePeriod;
                }
                return userResponseObject;
            }

            userResponseObject.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;

            return userResponseObject;
        }

        public override void PostSignIn(ref UserResponseObject response, ref List<KeyValuePair> keyValueList) { }

        public override UserResponseObject PreAddNewUser(ref UserBasicData basicData, ref UserDynamicData dynamicData, ref string password, ref DomainInfo domainInfo, ref List<KeyValuePair> keyValueList)
        {
            return new UserResponseObject();
        }

        internal override UserResponseObject MidAddNewUser(UserBasicData basicData, UserDynamicData dynamicData, string password, ref List<KeyValuePair> keyValueList, DomainInfo domainInfo = null)
        {
            UserResponseObject userResponse = new UserResponseObject();
            User newUser = new User();
            if (!string.IsNullOrEmpty(basicData.m_sUserName) && basicData.m_sUserName.ToLower().Contains("anonymous"))
                basicData.m_sUserName = string.Format(basicData.m_sUserName + "_{0}", User.GetNextGUID());

            newUser.InitializeNewUser(basicData, dynamicData, GroupId, password);

            if (!string.IsNullOrEmpty(newUser.m_sSiteGUID) || newUser.m_oBasicData != basicData)
            {
                userResponse.Initialize(ResponseStatus.UserExists, newUser);
                return userResponse;
            }

            if (!string.IsNullOrEmpty(newUser.m_oBasicData.m_CoGuid) && UsersDal.GetUserIDByExternalId(GroupId, newUser.m_oBasicData.m_CoGuid) > 0)
            {
                userResponse.Initialize(ResponseStatus.ExternalIdAlreadyExists, newUser);
                return userResponse;
            }

            // save user
            int userId = FlowManager.SaveNewUser(ref userResponse, this, ref basicData, newUser, GroupId, !IsActivationNeeded(basicData), keyValueList);
            if (userId <= 0)
            {
                userResponse.Initialize(ResponseStatus.ErrorOnSaveUser, newUser);
                return userResponse;
            }

            // add domain
            if (newUser.m_domianID <= 0)
            {
                FlowManager.AddDomain(ref userResponse, this, newUser, basicData.m_sUserName, userId, domainInfo, keyValueList);
            }
            else
            {
                userResponse.Initialize(ResponseStatus.OK, newUser);
            }

            // add role to user + create default rules
            if (userResponse.m_RespStatus == ResponseStatus.OK || userResponse.m_RespStatus == ResponseStatus.UserWithNoDomain)
            {
                FlowManager.CreateDefaultRules(ref userResponse, this, newUser, userId.ToString(), GroupId, keyValueList);
            }

            // send welcome mail
            FlowManager.SendWelcomeMailRequest(ref userResponse, this, newUser, password, keyValueList);

            return userResponse;
        }

        public override void PostAddNewUser(ref UserResponseObject response, ref List<KeyValuePair> keyValueList) { }

        public override void PreSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive, ref List<KeyValuePair> keyValueList) { }

        internal override int MidSaveNewUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive)
        {
            return user.SaveForInsert(GroupId, !IsActivationNeeded(basicData));
        }

        public override void PostSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive, int userId, ref List<KeyValuePair> keyValueList) { }

        internal override void InitSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string userName, string password, string email, string facebookId)
        {
            mailRequest.m_sToken = UserStorage.Instance().GetActivationToken(GroupId, userName);
            mailRequest.m_sTemplateName = WelcomeMailTemplate;
            mailRequest.m_eMailType = eMailTemplateType.Welcome;
            mailRequest.m_sFirstName = firstName;
            mailRequest.m_sLastName = string.Empty;
            mailRequest.m_sSenderFrom = MailFromAdd;
            mailRequest.m_sSenderName = MailFromName;
            mailRequest.m_sSenderTo = email;
            mailRequest.m_sSubject = WelcomeMailSubject;
            mailRequest.m_sUsername = userName;
            mailRequest.m_sPassword = (string.IsNullOrEmpty(facebookId)) ? password : "Facebook Password";
        }

        public override void PreSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string userName, string password, string email, string facebookId, ref List<KeyValuePair> keyValueList) { }

        internal override bool MidSendWelcomeMail(ref UserResponseObject userResponse, WelcomeMailRequest mailRequest)
        {
            return Utils.SendMail(GroupId, mailRequest);
        }

        public override void PostSendWelcomeMail(ref UserResponseObject userResponse, bool mailSent, ref List<KeyValuePair> keyValueList) { }

        public override void PreDefaultRules(ref UserResponseObject userResponse, string siteGuid, int groupId, ref User userBo, ref List<KeyValuePair> keyValueList) { }

        internal override bool MidCreateDefaultRules(ref UserResponseObject userResponse, string siteGuid, int groupId, ref User userBo)
        {
            long roleId;
            int userId = int.Parse(siteGuid);

            if (DAL.UsersDal.IsUserDomainMaster(GroupId, userId))
            {
                roleId = ApplicationConfiguration.Current.RoleIdsConfiguration.MasterRoleId.Value;
            }
            else
            {
                roleId = ApplicationConfiguration.Current.RoleIdsConfiguration.UserRoleId.Value;
            }

            if (roleId > 0 && !userResponse.m_user.m_oBasicData.RoleIds.Contains(roleId))
            {
                userResponse.m_user.m_oBasicData.RoleIds.Add(roleId);
                if (UsersDal.UpsertUserRoleIds(GroupId, userId, userResponse.m_user.m_oBasicData.RoleIds))
                {
                    string invalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(groupId, siteGuid);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on MidCreateDefaultRules key = {0}", invalidationKey);
                    }
                }
            }

            if (userResponse.m_user.m_oBasicData.RoleIds.Count == 0)
            {
                userResponse.Initialize(ResponseStatus.UserCreatedWithNoRole, userResponse.m_user);
                log.ErrorFormat("User created with no role. userId = {0}", siteGuid);
                return false;
            }

            return true;
        }

        public override void PostDefaultRules(ref UserResponseObject userResponse, bool passed, string siteGuid, int groupId, ref User userBo, ref List<KeyValuePair> keyValueList) { }

        internal override DomainResponseObject AddNewDomain(string username, int userId, int groupId)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, groupId);
            DomainResponseObject dr = t.AddDomain(username + "/Domain", username + "/Domain", userId, groupId, "", null);

            if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
            {
                // Error adding to domain
                log.Error("Add New Domain Error - Domain = " + t.ToString());
            }
            return dr;
        }

        public override UserResponseObject GetUserByCoGuid(string coGuid, int operatorId)
        {
            UserResponseObject retVal = new UserResponseObject();
            retVal.m_RespStatus = ResponseStatus.UserDoesNotExist;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += " select id from users where is_active = 1 and status = 1 and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("coguid", "=", coGuid);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", GroupId);

            selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    string sID = selectQuery.Table("query").DefaultView[0].Row["id"].ToString();
                    retVal = GetUserData(sID);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        public override UserResponseObject GetUserData(string siteGuid, bool shouldSaveInCache = true)
        {
            try
            {
                Int32 userId = int.Parse(siteGuid);
                User user = new User();

                user.Initialize(userId, GroupId, shouldSaveInCache);

                UserResponseObject resp = new UserResponseObject();
                if (user.m_oBasicData.m_sUserName == "")
                    resp.Initialize(ResponseStatus.UserDoesNotExist, user);
                else
                    resp.Initialize(ResponseStatus.OK, user);
                return resp;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at GetUserData. Site Guid: ", siteGuid));
                sb.Append(String.Concat(" G ID: ", GroupId));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                throw;
            }
        }

        public override void Initialize()
        {
            if (ActivationMail == null)
                ActivationMail = "";

            // try get mail parameters from cache 
            KalturaUsers tUser = null;
            string key = string.Format("users_KalturaUsersInitialize_{0}", GroupId);
            bool bRes = UsersCache.GetItem<KalturaUsers>(key, out tUser);
            if (bRes)
            {
                this.isActivationNeededProp = tUser.isActivationNeededProp;
                this.mailImpl = tUser.mailImpl;
                this.GroupId = tUser.GroupId;
                this.ActivationMail = tUser.ActivationMail;
                this.ChangedPinMail = tUser.ChangedPinMail;
                this.ChangedPinMailSubject = tUser.ChangedPinMailSubject;
                this.ChangePassMailSubject = tUser.ChangePassMailSubject;
                this.ChangePasswordMail = tUser.ChangePasswordMail;
                this.ForgotPassMailSubject = tUser.ForgotPassMailSubject;
                this.ForgotPasswordMail = tUser.ForgotPasswordMail;
                this.MailFromAdd = tUser.MailFromAdd;
                this.MailFromName = tUser.MailFromName;
                this.mailPort = tUser.mailPort;
                this.MailServer = tUser.MailServer;
                this.MailServerPass = tUser.MailServerPass;
                this.MailServerUN = tUser.MailServerUN;
                this.mailSSL = tUser.mailSSL;
                this.SendPasswordMailSubject = tUser.SendPasswordMailSubject;
                this.SendPasswordMailTemplate = tUser.SendPasswordMailTemplate;
                this.WelcomeFacebookMailSubject = tUser.WelcomeFacebookMailSubject;
                this.WelcomeFacebookMailTemplate = tUser.WelcomeFacebookMailTemplate;
                this.WelcomeMailSubject = tUser.WelcomeMailSubject;
                this.WelcomeMailTemplate = tUser.WelcomeMailTemplate;
                this.activationMustHours = tUser.activationMustHours;
                this.tokenValidityHours = tUser.tokenValidityHours;
                this.changePinTokenValidityHours = tUser.changePinTokenValidityHours;
            }
            else
            {
                DataRowView dvMailParameters = DAL.UsersDal.GetGroupMailParameters(GroupId);
                if (dvMailParameters != null)
                {
                    // string members
                    WelcomeMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL");
                    WelcomeFacebookMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_FACEBOOK_MAIL");
                    MailFromAdd = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_ADD");
                    WelcomeMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_MAIL_SUBJECT");
                    WelcomeFacebookMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "WELCOME_FACEBOOK_MAIL_SUBJECT");
                    ForgotPasswordMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "FORGOT_PASSWORD_MAIL");
                    ChangedPinMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGED_PIN_MAIL");
                    ChangedPinMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGED_PIN_MAIL_SUBJECT");
                    ActivationMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "ACTIVATION_MAIL");
                    MailFromName = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_FROM_NAME");
                    MailServer = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_SERVER");
                    MailServerUN = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_USER_NAME");
                    MailServerPass = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "MAIL_PASSWORD");
                    ForgotPassMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "FORGOT_PASS_MAIL_SUBJECT");
                    SendPasswordMailTemplate = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "SEND_PASSWORD_MAIL");
                    SendPasswordMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "SEND_PASSWORD_MAIL_SUBJECT");
                    ChangePasswordMail = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGE_PASSWORD_MAIL");
                    ChangePassMailSubject = ODBCWrapper.Utils.GetSafeStr(dvMailParameters, "CHANGE_PASSWORD_MAIL_SUBJECT");
                    //int members
                    Int32 nActivationNeeded = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["IS_ACTIVATION_NEEDED"]);
                    activationMustHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["ACTIVATION_MUST_HOURS"]);
                    tokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["TOKEN_VALIDITY_HOURS"]);
                    changePinTokenValidityHours = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters["CHANGED_PIN_TOKEN_VALIDITY_HOURS"]);
                    mailSSL = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "MAIL_SSL");
                    mailPort = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "MAIL_PORT");
                    //bool member
                    isActivationNeededProp = (nActivationNeeded == 1);

                    int nMailImplID = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "Mail_Impl_ID");

                    if (nMailImplID > 0)
                        mailImpl = Utils.GetBaseImpl(GroupId, 0, nMailImplID);

                    // add to cache 
                    bRes = UsersCache.AddItem(key, this);
                }
            }
        }

        /// <summary>
        /// This method exists only for backward compatibly (implementing the ISSOProvider interface)
        /// </summary>
        /// <param name="sCoGuid"></param>
        /// <param name="sPass"></param>
        /// <param name="nOperatorID"></param>
        /// <param name="nMaxFailCount"></param>
        /// <param name="nLockMinutes"></param>
        /// <param name="sSessionID"></param>
        /// <param name="sIP"></param>
        /// <param name="sDeviceID"></param>
        /// <param name="bPreventDoubleLogins"></param>
        /// <returns></returns>
        public UserResponseObject SignIn(string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins) { return null; }

        public override UserResponseObject PreSignOut(ref int siteGuid, ref int groupId, ref string sessionId, ref string ip, ref string deviceUdid, ref List<KeyValuePair> keyValueList) { return new UserResponseObject(); }

        internal override UserResponseObject MidSignOut(int siteGuid, int groupId, string sessionId, string ip, string deviceUdid)
        {

            return User.SignOut(siteGuid, groupId, sessionId, ip, deviceUdid);
        }

        public override void PostSignOut(ref UserResponseObject userResponse, int siteGuid, int groupId, string sessionId, string ip, string deviceUdid, ref List<KeyValuePair> keyValueList) { }

        internal override void GetUserData(ref UserResponseObject userResponse, string siteGuid, string userIP)
        {
            try
            {
                Int32 userId = int.Parse(siteGuid);
                User user = new User();

                user.Initialize(userId, GroupId);

                if (user.m_oBasicData.m_sUserName == "")
                    userResponse.Initialize(ResponseStatus.UserDoesNotExist, user);
                else
                    userResponse.Initialize(ResponseStatus.OK, user);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at GetUserData. Site Guid: ", siteGuid));
                sb.Append(String.Concat(" G ID: ", GroupId));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                throw;
            }
        }

        internal override void GetUsersData(ref List<UserResponseObject> userResponses, List<string> siteGuids, ref List<KeyValuePair> keyValueList, string userIP)
        {
            if (siteGuids != null)
            {
                for (int i = 0; i < siteGuids.Count; i++)
                {
                    try
                    {
                        UserResponseObject temp = FlowManager.GetUserData(this, siteGuids[i], keyValueList, userIP);
                        if (temp != null)
                        {
                            userResponses.Add(temp);
                        }
                    }
                    catch { }
                }
            }
        }

        public override ApiObjects.Response.Status PreDeleteUser(int siteGuid)
        {
            return new ApiObjects.Response.Status();
        }
        internal override ApiObjects.Response.Status MidDeleteUser(int userId)
        {
            var response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            try
            {
                UserResponseObject userResponse = GetUserData(userId.ToString());
                if (userResponse.m_RespStatus != ResponseStatus.OK)
                {
                    response = Utils.ConvertResponseStatusToResponseObject(userResponse.m_RespStatus);
                    return response;
                }

                // get User's domain 
                BaseDomain baseDomain = null;
                Utils.GetBaseImpl(ref baseDomain, GroupId);

                if (baseDomain == null)
                {
                    response.Code = (int)eResponseStatus.DomainNotInitialized;
                    response.Message = HOUSEHOLD_NOT_INITIALIZED;
                    return response;
                }

                Domain userDomain = baseDomain.GetDomainInfo(userResponse.m_user.m_domianID, GroupId);
                if (userDomain == null)
                {
                    response.Code = (int)eResponseStatus.UserNotExistsInDomain;
                    response.Message = USER_NOT_EXISTS_IN_DOMAIN;
                    return response;
                }

                //Delete is not allowed if the user is in the DefaultUsersIDs list.
                if (userDomain.m_DefaultUsersIDs.Contains(userId))
                {
                    response.Code = (int)eResponseStatus.DefaultUserCannotBeDeleted;
                    response.Message = DEFAULT_USER_CANNOT_BE_DELETED;
                    return response;
                }

                //Delete is not allowed if the user is in the master in the domain and there is only 1 master.
                if (userDomain.m_masterGUIDs.Contains(userId) && userDomain.m_masterGUIDs.Count == 1)
                {
                    response.Code = (int)eResponseStatus.ExclusiveMasterUserCannotBeDeleted;
                    response.Message = EXCLUSIVE_MASTER_USER_CANNOT_BE_DELETED;
                    return response;
                }

                //Delete 
                // in case user in domain ( domain id > 0 ) remove user from domain 
                if (userResponse.m_user.m_domianID > 0)
                {
                    DomainResponseObject domainResponse = baseDomain.RemoveUserFromDomain(GroupId, userResponse.m_user.m_domianID, userId);
                    if (domainResponse.m_oDomainResponseStatus != DomainResponseStatus.OK)
                    {
                        response = Utils.ConvertDomainResponseStatusToResponseObject(domainResponse.m_oDomainResponseStatus);
                        return response;
                    }
                }

                // delete user 
                if (UsersDal.DeleteUser(GroupId, userId))
                {
                    // GDPR TTV
                    ApiObjects.Segmentation.UserSegment.Remove(userId.ToString());

                    response.Code = (int)eResponseStatus.OK;
                    response.Message = eResponseStatus.OK.ToString();

                    // remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(userId, GroupId);

                    return response;
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, ex.Message);
                log.Error("DeleteUser - " + string.Format("Failed ex={0}, siteGuid={1}, groupID ={2}, ", ex.Message, userId, GroupId), ex);
            }
            return response;
        }
        public override void PostDeleteUser(ref ApiObjects.Response.Status response) { }

        public override SSOAdapterProfileInvoke Invoke(int groupId, string intent, List<KeyValuePair> keyValueList)
        {
            return new SSOAdapterProfileInvoke() { AdapterData = new Dictionary<string, string>() };
        }
    }
}
