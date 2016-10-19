using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using DAL;

namespace Users
{
    public abstract class KalturaBaseUsers
    {
        protected Int32 GroupId { get; set; }
        protected Int32 activationMustHours;
        protected Int32 tokenValidityHours;
        protected Int32 changePinTokenValidityHours;
        protected int mailSSL = 0;
        protected int mailPort = 0;
        protected BaseNewsLetterImpl newsLetterImpl;
        protected BaseMailImpl mailImpl;
        protected bool? isActivationNeededProp;

        // mail properties
        protected string WelcomeMailTemplate { get; set; }
        protected string WelcomeFacebookMailTemplate { get; set; }
        protected string ForgotPasswordMail { get; set; }
        protected string ChangePasswordMail { get; set; }
        protected string ChangedPinMail { get; set; }
        protected string ActivationMail { get; set; }
        protected string MailFromName { get; set; }
        protected string MailFromAdd { get; set; }
        protected string MailServer { get; set; }
        protected string MailServerUN { get; set; }
        protected string MailServerPass { get; set; }
        protected string WelcomeMailSubject { get; set; }
        protected string WelcomeFacebookMailSubject { get; set; }
        protected string ForgotPassMailSubject { get; set; }
        protected string ChangePassMailSubject { get; set; }
        protected string ChangedPinMailSubject { get; set; }
        protected string SendPasswordMailTemplate { get; set; }
        protected string SendPasswordMailSubject { get; set; }


        protected KalturaBaseUsers(Int32 groupId)
        {
            this.GroupId = groupId;
            Initialize();
        }

        public abstract UserResponseObject GetUserByCoGuid(string coGuid, int operatorId);
        public abstract UserResponseObject GetUserData(string sSiteGUID, bool sholudSaveInCache = true);
        public abstract void Initialize();

        ////Domain
        internal abstract DomainResponseObject AddNewDomain(string username, int userId, int groupId);
        public abstract bool PreAddDomain(ref UserResponseObject userResponse, ref User user, ref string username, ref int userId, ref DomainInfo domainInfo, ref List<KeyValuePair> keyValueList);
        internal abstract bool MidAddDomain(ref UserResponseObject userResponse, User user, string username, int userId, DomainInfo domainInfo);
        public abstract void PostAddDomain(bool addDomainPassed, ref UserResponseObject userResponse, User user, string username, int userId, DomainInfo domainInfo, ref List<KeyValuePair> keyValueList);

        // SignIn                                    
        public abstract UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList);
        internal abstract UserResponseObject MidSignIn(Int32 siteGuid, string userName, string password, int maxFailCount, int lockMin, int groupId, string sessionId, string ip, string deviceId, bool preventDoubleLogin);
        public abstract void PostSignIn(ref UserResponseObject userResponse, ref List<KeyValuePair> keyValueList);

        // SignOut
        public abstract UserResponseObject PreSignOut(ref int siteGuid, ref int groupId, ref string sessionId, ref string ip, ref  string deviceUdid, ref List<KeyValuePair> keyValueList);
        internal abstract UserResponseObject MidSignOut(int siteGuid, int groupId, string sessionId, string ip, string deviceUdid);
        public abstract void PostSignOut(ref UserResponseObject userResponse, int siteGuid, int groupId, string sessionId, string ip, string deviceUdid, ref List<KeyValuePair> keyValueList);

        // SignUp (AddNewUser)
        public abstract UserResponseObject PreAddNewUser(ref UserBasicData basicData, ref UserDynamicData dynamicData, ref string password, ref DomainInfo domainInfo, ref List<KeyValuePair> keyValueList);
        internal abstract UserResponseObject MidAddNewUser(UserBasicData basicData, UserDynamicData dynamicData, string password, ref List<KeyValuePair> keyValueList, DomainInfo domainInfo = null);
        public abstract void PostAddNewUser(ref UserResponseObject userResponse, ref List<KeyValuePair> keyValueList);

        // Welcome Mail
        internal abstract void InitSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string username, string password, string email, string facebookId);
        public abstract void PreSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string username, string password, string email, string facebookId, ref List<KeyValuePair> keyValueList);
        internal abstract bool MidSendWelcomeMail(ref UserResponseObject userResponse, WelcomeMailRequest mailRequest);
        public abstract void PostSendWelcomeMail(ref UserResponseObject userResponse, bool mailSent, ref List<KeyValuePair> keyValueList);

        // Default Rules
        public abstract void PreDefaultRules(ref UserResponseObject userResponse, string siteGuid, int groupId, ref User userBo, ref List<KeyValuePair> keyValueList);
        internal abstract bool MidCreateDefaultRules(ref UserResponseObject userResponse, string siteGuid, int groupId, ref User userBo);
        public abstract void PostDefaultRules(ref UserResponseObject userResponse, bool passed, string siteGuid, int groupId, ref User userBo, ref List<KeyValuePair> keyValueList);

        // Newsletter
        internal abstract void InitSubscribeToNewsLetter(ref UserResponseObject userResponse, ref UserDynamicData dynamicData, ref User user, ref bool shouldSubscribe);
        public abstract void PreSubscribeToNewsLetter(ref UserResponseObject userResponse, ref UserDynamicData dynamicData, ref User user, ref bool shouldSubscribe, ref List<KeyValuePair> keyValueList);
        internal abstract bool MidSubscribeToNewsLetter(ref UserResponseObject userResponse, UserDynamicData dynamicData, User user, ref bool shouldSubscribe);
        public abstract void PostSubscribeToNewsLetter(ref UserResponseObject userResponse, bool passed, ref UserDynamicData dynamicData, ref User user, ref List<KeyValuePair> keyValueList);

        // save user
        public abstract void PreSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 nGroupID, bool IsSetUserActive, ref List<KeyValuePair> keyValueList);
        internal abstract int MidSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 nGroupID, bool IsSetUserActive);
        public abstract void PostSaveUser(ref UserResponseObject userResponse, ref UserBasicData basicData, User user, Int32 nGroupID, bool IsSetUserActive, int userId, ref List<KeyValuePair> keyValueList);

        // get user data
        public abstract UserResponseObject PreGetUserData(string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP);
        internal abstract void MidGetUserData(ref UserResponseObject userResponse, string sSiteGUID, string userIP);
        public abstract void PostGetUserData(ref UserResponseObject userResponse, string sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP);

        // get users data
        public abstract List<UserResponseObject> PreGetUsersData(List<string> sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP);
        internal abstract void MidGetUsersData(ref List<UserResponseObject> userResponse, List<string> sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP);
        public abstract void PostGetUsersData(ref List<UserResponseObject> userResponse, List<string> sSiteGUID, ref List<KeyValuePair> keyValueList, string userIP);

        // delete user
        public abstract ApiObjects.Response.Status PreDeleteUser(int siteGuid);
        internal abstract ApiObjects.Response.Status MidDeleteUser(int siteGuid);
        public abstract void PostDeleteUser(ref ApiObjects.Response.Status response);

        public virtual bool SetUserDynamicData(string sSiteGUID, List<KeyValuePair> lKeyValue, UserResponseObject uro)
        {

            if (uro == null)
                uro = GetUserData(sSiteGUID, false);

            if (uro.m_RespStatus != ResponseStatus.OK || uro.m_user == null || uro.m_user.m_oDynamicData == null)
                return false;

            if (uro.m_user.m_oDynamicData.m_sUserData == null)
                uro.m_user.m_oDynamicData.m_sUserData = new UserDynamicDataContainer[0];

            bool hasChanged = false; //indicates if there is a need to update the dynamic data           
            List<UserDynamicDataContainer> newPairs = new List<UserDynamicDataContainer>();

            foreach (KeyValuePair pair in lKeyValue)
            {
                bool exists = false;//indicates if the pair exists inside the current dynamic data or not
                for (int i = 0; i < uro.m_user.m_oDynamicData.m_sUserData.Length && !exists; i++)
                {
                    if (uro.m_user.m_oDynamicData.m_sUserData[i].m_sDataType == pair.key)
                    {
                        exists = true;
                        if (uro.m_user.m_oDynamicData.m_sUserData[i].m_sValue != pair.value) //change the value only if it has changed
                        {
                            uro.m_user.m_oDynamicData.m_sUserData[i].m_sValue = pair.value;
                            hasChanged = true;
                        }
                    }
                }
                if (!exists)
                {
                    UserDynamicDataContainer ud = new UserDynamicDataContainer();
                    ud.m_sDataType = pair.key;
                    ud.m_sValue = pair.value;
                    newPairs.Add(ud);
                }
            }

            if (hasChanged && newPairs.Count == 0)
            {
                uro.m_user.UpdateDynamicData(uro.m_user.m_oDynamicData, GroupId);
            }
            else if (newPairs.Count > 0)
            {
                UserDynamicData newUdd = new UserDynamicData();
                newUdd.m_sUserData = new UserDynamicDataContainer[uro.m_user.m_oDynamicData.m_sUserData.Length + newPairs.Count];

                int preLength = uro.m_user.m_oDynamicData.m_sUserData.Length;
                for (int i = 0; i < preLength; i++)//copy all elements that are not new
                    newUdd.m_sUserData[i] = uro.m_user.m_oDynamicData.m_sUserData[i];

                for (int j = 0; j < newPairs.Count; j++)//add the new pairs
                    newUdd.m_sUserData[j + preLength] = newPairs[j];

                uro.m_user.UpdateDynamicData(newUdd, GroupId);
            }

            return true;
        }

        public virtual bool IsActivationNeeded(UserBasicData basicData)
        {
            if (!this.isActivationNeededProp.HasValue)
                this.isActivationNeededProp = UsersDal.GetIsActivationNeeded(GroupId);

            return (this.isActivationNeededProp.HasValue ? isActivationNeededProp.Value : true);
        }
    }
}
