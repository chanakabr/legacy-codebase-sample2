using System.Collections.Generic;
using DAL;
using System;
using Newtonsoft.Json;
using ApiObjects;

namespace Users
{
    public class MediaCorpUsers : SSOUsers, ISSOProvider
    {
        public MediaCorpUsers(int nGroupID, int operatorId)
            : base(nGroupID, operatorId)
        {

        }


        public override bool IsActivationNeeded(UserBasicData oBasicData)
        {
            if (oBasicData == null)
                return base.IsActivationNeeded(null);
            return string.IsNullOrEmpty(oBasicData.m_sFacebookToken);
        }

        protected override WelcomeMailRequest GetWelcomeMailRequest(string sFirstName, string sUserName, string sPassword, string sEmail, string sFacekookID)
        {
            WelcomeMailRequest retVal = new WelcomeMailRequest();
            string sMailData = string.Empty;
            string sActivation = string.Empty;
            retVal.m_eMailType = eMailTemplateType.Welcome;
            retVal.m_sFirstName = sFirstName;
            retVal.m_sLastName = string.Empty;
            retVal.m_sSenderFrom = m_sMailFromAdd;
            retVal.m_sSenderName = m_sMailFromName;
            retVal.m_sSenderTo = sEmail;
            retVal.m_sUsername = sUserName;
            sActivation = UsersDal.GetActivationToken(m_nGroupID, sUserName);
            
            if (string.IsNullOrEmpty(sFacekookID))
            {
                //not facebook registeration, user needs to activate his account.

                retVal.m_sPassword = sPassword;
                retVal.m_sTemplateName = m_sWelcomeMailTemplate;
                retVal.m_sSubject = m_sWelcomeMailSubject;
            }
            else
            {

                if (sActivation.Length > 0)
                {
                    /*
                     the user did the following flow:
                 1. regular registeration
                 2. did NOT activate his account
                 3. merged his account with facebook
                 4. tried to login after non activation period expires
                 5. asked to resend activation token.
                     */
                    retVal.m_sPassword = sPassword;
                    retVal.m_sTemplateName = m_sWelcomeMailTemplate;
                    retVal.m_sSubject = m_sWelcomeMailSubject;

                }
                else
                {
                    // facebook registeration. no need for user to activate his account.
                    retVal.m_sPassword = "Facebook Password";
                    retVal.m_sTemplateName = m_sWelcomeFacebookMailTemplate;
                    retVal.m_sSubject = m_sWelcomeFacebookMailSubject;

                }
            }
            retVal.m_sToken = sActivation;

            return retVal;

        }

        private string GenerateUniqueNumberWithChecksum(string sSiteGuid)
        {
            int guidLength = sSiteGuid.Length;
            int extraDigitCount = 8 - guidLength;

            Random r = new Random();
            for (int i = 0; i < extraDigitCount; i++)
            {
                sSiteGuid = "0" + sSiteGuid;
            }
            sSiteGuid += GenerateChecksum(sSiteGuid);
            return sSiteGuid;
        }

        private int GenerateChecksum(string s)
        {
            char[] chars = s.ToCharArray();
            int sum = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                sum += int.Parse(chars[i].ToString());
            }
            return (sum % 6);
        }

        #region ISSOProviderImplementation

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            User u = new User();
            UserResponseObject uRepsObj = new UserResponseObject();
            int nSiteGuid = u.InitializeByUsername(sUserName, m_nGroupID);

            if (nSiteGuid == 0 || u.m_sSiteGUID.Length == 0)
            {
                uRepsObj.Initialize(ResponseStatus.UserDoesNotExist, u);
            }
            else
            {
                uRepsObj.Initialize(ResponseStatus.OK, u);
            }

            return uRepsObj;
        }

        #endregion
    }
}
