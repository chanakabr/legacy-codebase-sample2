using ApiObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

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
                            domainResponse = domain.AddDomain(username + "/Domain", username + "/Domain", userId, domainInfo.GroupId, domainInfo.DomainCoGuid);

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
        internal override UserResponseObject MidSignIn(Int32 siteGuid, string userName, string password, int maxFailCount, int lockMin, int groupId, string sessionId, string ip, string deviceId, bool preventDoubleLogin)
        {
            UserResponseObject Response = new UserResponseObject();
            Int32 oldSiteGuid = siteGuid;
            bool isGracePeriod = false;
            UserActivationState userStatus = GetUserStatus(ref userName, ref siteGuid, ref isGracePeriod);

            if (userStatus != UserActivationState.Activated)
            {
                Response.m_RespStatus = ResponseStatus.UserNotActivated;

                if (siteGuid <= 0)
                    Response.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                else
                {
                    switch (userStatus)
                    {
                        case UserActivationState.UserDoesNotExist:
                            Response.m_RespStatus = ResponseStatus.UserDoesNotExist;
                            break;

                        case UserActivationState.NotActivated:
                            Response.m_user = new User(groupId, siteGuid);
                            Response.m_RespStatus = ResponseStatus.UserNotActivated;
                            break;

                        case UserActivationState.NotActivatedByMaster:
                            Response.m_user = new User(groupId, siteGuid);
                            Response.m_RespStatus = ResponseStatus.UserNotMasterApproved;
                            break;

                        case UserActivationState.UserRemovedFromDomain:
                            Response.m_user = new User(groupId, siteGuid);
                            Response.m_RespStatus = ResponseStatus.UserNotIndDomain;
                            break;
                        case UserActivationState.UserWIthNoDomain:
                            Response.m_user = new User(groupId, siteGuid);
                            bool bValidDomainStat = MidAddDomain(ref Response, Response.m_user, userName, siteGuid, new DomainInfo(groupId));
                            if (!bValidDomainStat)
                                return Response;
                            break;
                        case UserActivationState.UserSuspended:
                            Response.m_user = new User(groupId, siteGuid);
                            Response.m_RespStatus = ResponseStatus.UserSuspended;
                            break;
                    }
                }
                if (userStatus != UserActivationState.UserWIthNoDomain)
                    return Response;
            }

            if (oldSiteGuid == 0)
            {
                // check username and password
                Response = User.SignIn(userName, password, maxFailCount, lockMin, groupId, sessionId, ip, deviceId, preventDoubleLogin);
                if (Response != null && Response.m_user != null)
                {
                    Response.m_user.IsActivationGracePeriod = isGracePeriod;
                }
                return Response;
            }
            else if (oldSiteGuid == siteGuid)
            {
                // validate siteguid received is legal
                Response = User.SignIn(siteGuid, maxFailCount, lockMin, groupId, sessionId, ip, deviceId, preventDoubleLogin);
                if (Response != null && Response.m_user != null)
                {
                    Response.m_user.IsActivationGracePeriod = isGracePeriod;
                }
                return Response;
            }
            
            Response.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;

            return Response;
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

            newUser.Initialize(basicData, dynamicData, GroupId, password);
            if (newUser.m_sSiteGUID != "")
            {
                userResponse.Initialize(ResponseStatus.UserExists, newUser);
                return userResponse;
            }
            else
            {
                if (newUser.m_oBasicData != basicData)
                {
                    userResponse.Initialize(ResponseStatus.UserExists, newUser);
                    return userResponse;
                }
            }

            // save user
            int nUserID = FlowManager.SaveUser(ref userResponse, this, ref basicData, newUser, GroupId, !IsActivationNeeded(basicData), keyValueList);

            // add domain
            if (newUser.m_domianID <= 0)
            {
                FlowManager.AddDomain(ref userResponse, this, newUser, basicData.m_sUserName, nUserID, domainInfo, keyValueList);
            }
            else
                userResponse.Initialize(ResponseStatus.OK, newUser);

            // add role to user
            if (userResponse.m_RespStatus == ResponseStatus.OK || userResponse.m_RespStatus == ResponseStatus.UserWithNoDomain)
            {
                long roleId;

                if (DAL.UsersDal.IsUserDomainMaster(GroupId, nUserID))
                {
                    long.TryParse(Utils.GetTcmConfigValue("master_role_id"), out roleId);
                }
                else
                {
                    long.TryParse(Utils.GetTcmConfigValue("user_role_id"), out roleId);
                }

                if (roleId != 0)
                {
                    DAL.UsersDal.Insert_UserRole(GroupId, nUserID.ToString(), roleId, true);
                }
                else
                {
                    userResponse.m_RespStatus = ResponseStatus.UserCreatedWithNoRole;
                    log.ErrorFormat("User created with no role. userId = {0}", nUserID);
                }
            }
            // create default rules
            FlowManager.CreateDefaultRules(ref userResponse, this, newUser, newUser.m_sSiteGUID, GroupId, keyValueList);

            // subscribe to news letter
            FlowManager.SubscribeToNewsLetter(ref userResponse, this, dynamicData, newUser, keyValueList);

            // send welcome mail
            FlowManager.SendWelcomeMailRequest(ref userResponse, this, newUser, password, keyValueList);

            return userResponse;
        }

        public override void PostAddNewUser(ref UserResponseObject response, ref List<KeyValuePair> keyValueList) { }

        public override void PreSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive, ref List<KeyValuePair> keyValueList) { }

        internal override int MidSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive)
        {
            return user.Save(GroupId, !IsActivationNeeded(basicData), true);
        }

        public override void PostSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 groupId, bool IsSetUserActive, int userId, ref List<KeyValuePair> keyValueList) { }

        internal override void InitSubscribeToNewsLetter(ref UserResponseObject response, ref UserDynamicData dynamicData, ref User user, ref bool shouldSubscribe)
        {
            string sNewsLetter = dynamicData.GetValByKey("newsletter");
            if (!string.IsNullOrEmpty(sNewsLetter) && sNewsLetter.ToLower().Equals("true"))
                shouldSubscribe = true;
            else
                shouldSubscribe = false;
        }

        public override void PreSubscribeToNewsLetter(ref UserResponseObject response, ref UserDynamicData dynamicData, ref User user, ref bool shouldSubscribe, ref List<KeyValuePair> keyValueList) { }

        internal override bool MidSubscribeToNewsLetter(ref UserResponseObject userResponse, UserDynamicData dynamicData, User user, ref bool shouldSubscribe)
        {
            bool passed = false;
            if (shouldSubscribe)
            {
                if (newsLetterImpl != null)
                {
                    if (!newsLetterImpl.IsUserSubscribed(user))
                        passed = newsLetterImpl.Subscribe(userResponse.m_user);
                }
            }
            return passed;
        }

        public override void PostSubscribeToNewsLetter(ref UserResponseObject userResponse, bool passed, ref UserDynamicData dynamicData, ref User user, ref List<KeyValuePair> keyValueList) { }

        internal override void InitSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string userName, string password, string email, string facebookId)
        {
            mailRequest.m_sToken = DAL.UsersDal.GetActivationToken(GroupId, userName);
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
            // return client.SetDefaultRules(wsUserName, wSPass, siteGuid);
            return true;
        }

        public override void PostDefaultRules(ref UserResponseObject userResponse, bool passed, string siteGuid, int groupId, ref User userBo, ref List<KeyValuePair> keyValueList) { }

        public UserActivationState GetUserStatus(ref string username, ref Int32 userId, ref bool isGracePeriod)
        {
            UserActivationState activStatus = (UserActivationState)DAL.UsersDal.GetUserActivationState(GroupId, activationMustHours, ref username, ref userId, ref isGracePeriod);

            return activStatus;
        }

        internal override DomainResponseObject AddNewDomain(string username, int userId, int groupId)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, groupId);
            DomainResponseObject dr = t.AddDomain(username + "/Domain", username + "/Domain", userId, groupId, "");

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

                if (newsLetterImpl != null)
                {
                    if (user.m_oDynamicData != null && user.m_oDynamicData.GetDynamicData() != null)
                    {
                        foreach (UserDynamicDataContainer udc in user.m_oDynamicData.GetDynamicData())
                        {
                            if (udc.m_sDataType.ToLower().Equals("newsletter"))
                            {
                                udc.m_sValue = newsLetterImpl.IsUserSubscribed(user).ToString().ToLower();
                            }
                        }
                    }
                }
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

            lock (ActivationMail)
            {

                // try get mail parameters from cache 
                KalturaUsers tUser = null;
                string key = string.Format("users_KalturaUsersInitialize_{0}", GroupId);
                bool bRes = UsersCache.GetItem<KalturaUsers>(key, out tUser);
                if (bRes)
                {
                    this.isActivationNeededProp = tUser.isActivationNeededProp;
                    this.mailImpl = tUser.mailImpl;
                    this.newsLetterImpl = tUser.newsLetterImpl;
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
                        //m_newsLetterImpl composition
                        object oNewLetterImplID = dvMailParameters["NewsLetter_Impl_ID"];
                        if (oNewLetterImplID != DBNull.Value && oNewLetterImplID != null && !string.IsNullOrEmpty(oNewLetterImplID.ToString()))
                        {
                            object oNewLetterApiKey = dvMailParameters["NewsLetter_API_Key"];
                            object oNewLetterListID = dvMailParameters["NewsLetter_List_ID"];

                            if (oNewLetterApiKey != DBNull.Value && oNewLetterApiKey != null && oNewLetterListID != DBNull.Value && oNewLetterListID != null)
                                newsLetterImpl = Utils.GetBaseImpl(oNewLetterApiKey.ToString(), oNewLetterListID.ToString(), int.Parse(oNewLetterImplID.ToString()));
                        }

                        int nMailImplID = ODBCWrapper.Utils.GetIntSafeVal(dvMailParameters, "Mail_Impl_ID");

                        if (nMailImplID > 0)
                            mailImpl = Utils.GetBaseImpl(GroupId, 0, nMailImplID);

                        // add to cache 
                        bRes = UsersCache.AddItem(key, this);
                    }
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

        public override UserResponseObject PreSignOut(ref int siteGuid, ref int groupId, ref string sessionId, ref string ip, ref  string deviceUdid, ref List<KeyValuePair> keyValueList) { return new UserResponseObject(); }

        internal override UserResponseObject MidSignOut(int siteGuid, int groupId, string sessionId, string ip, string deviceUdid)
        {

            return User.SignOut(siteGuid, groupId, sessionId, ip, deviceUdid);
        }

        public override void PostSignOut(ref UserResponseObject userResponse, int siteGuid, int groupId, string sessionId, string ip, string deviceUdid, ref List<KeyValuePair> keyValueList) { }

        public override UserResponseObject PreGetUserData(string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP) { return new UserResponseObject(); }

        internal override void MidGetUserData(ref UserResponseObject userResponse, string siteGuid, string userIP)
        {
            try
            {
                Int32 userId = int.Parse(siteGuid);
                User user = new User();

                user.Initialize(userId, GroupId);

                if (newsLetterImpl != null)
                {
                    if (user.m_oDynamicData != null && user.m_oDynamicData.GetDynamicData() != null)
                    {
                        foreach (UserDynamicDataContainer udc in user.m_oDynamicData.GetDynamicData())
                        {
                            if (udc.m_sDataType.ToLower().Equals("newsletter"))
                                udc.m_sValue = newsLetterImpl.IsUserSubscribed(user).ToString().ToLower();
                        }
                    }
                }

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

        public override void PostGetUserData(ref UserResponseObject userResponse, string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP) { }

        public override List<UserResponseObject> PreGetUsersData(List<string> sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP) { return new List<UserResponseObject>(); }

        internal override void MidGetUsersData(ref List<UserResponseObject> userResponses, List<string> siteGuids, ref List<KeyValuePair> keyValueList, string userIP)
        {
            try
            {
                for (int i = 0; i < siteGuids.Count; i++)
                {
                    try
                    {
                        UserResponseObject temp = FlowManager.GetUserData(this, siteGuids[i], keyValueList, userIP);
                        if (temp != null)
                            userResponses.Add(temp);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public override void PostGetUsersData(ref List<UserResponseObject> userResponse, List<string> sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP) { }

        public override ApiObjects.Response.Status PreDeleteUser(int siteGuid) 
        {
            return new ApiObjects.Response.Status();
        }
        internal override ApiObjects.Response.Status MidDeleteUser(int userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };

            try
            {
                UserResponseObject userResponse = GetUserData(userId.ToString());

                if (userResponse.m_RespStatus != ResponseStatus.OK)
                {
                    response = Utils.ConvertResponseStatusToResponseObject(userResponse.m_RespStatus);
                    return response;
                }

                // get User's domain 
                Core.Users.BaseDomain baseDomain = null;
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
                    response.Code = (int)eResponseStatus.OK;
                    response.Message = eResponseStatus.OK.ToString();

                    // remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(userId, GroupId);

                    // add invalidation key for user roles cache
                    string invalidationKey = CachingProvider.LayeredCache.LayeredCacheKeys.GetAddRoleInvalidationKey(userId.ToString());
                    if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on User.Save key = {0}", invalidationKey);
                    }
                    
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
    }
}
