using ApiObjects;
using Core.Users;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCorp
{
    public class User : KalturaUsers
    {
        public User(int groupId)
            : base(groupId)
        {
            // activate/deactivate user features
            this.ShouldSubscribeNewsLetter = true;
            this.ShouldCreateDefaultRules = true;
            this.ShouldSendWelcomeMail = true;
        }

        public override bool IsActivationNeeded(UserBasicData oBasicData)
        {
            if (oBasicData == null)
                return base.IsActivationNeeded(null);
            return string.IsNullOrEmpty(oBasicData.m_sFacebookToken);
        }

        public override void PreSendWelcomeMail(ref UserResponseObject userResponse, ref WelcomeMailRequest mailRequest, string firstName, string username, string password, string email, string facebookId, ref List<ApiObjects.KeyValuePair> keyValueList)
        {
            string sActivation = UsersDal.GetActivationToken(GroupId, username);

            if (string.IsNullOrEmpty(facebookId))
            {
                //not facebook registration, user needs to activate his account.
                mailRequest.m_sPassword = password;
            }
            else
            {
                if (sActivation.Length > 0)
                {
                    /*
                     the user did the following flow:
                 1. regular registration
                 2. did NOT activate his account
                 3. merged his account with facebook
                 4. tried to login after non activation period expires
                 5. asked to resend activation token.
                     */
                    mailRequest.m_sPassword = password;
                    mailRequest.m_sTemplateName = WelcomeMailTemplate;
                    mailRequest.m_sSubject = WelcomeMailSubject;
                }
                else
                {
                    // facebook registration. no need for user to activate his account.
                    mailRequest.m_sPassword = "Facebook Password";
                    mailRequest.m_sTemplateName = WelcomeFacebookMailTemplate;
                    mailRequest.m_sSubject = WelcomeFacebookMailSubject;
                }
            }
            mailRequest.m_sToken = sActivation;
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
    }
}
