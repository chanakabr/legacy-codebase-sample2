using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerceptiveMCAPI;
using PerceptiveMCAPI.Types;
using PerceptiveMCAPI.Methods;

namespace Core.Users
{
    public class MCNewsLetterImpl : BaseNewsLetterImpl
    {

        public MCNewsLetterImpl(string apiKey, string listID)
            : base(apiKey, listID)
        {
        }

        public MCNewsLetterImpl()
            : base()
        {
        }

        public override bool IsUserSubscribed(User user)
        {
            bool retVal = false;
            if (user != null && user.m_oBasicData != null)
            {
                string email = user.m_oBasicData.m_sEmail;
                List<string> emails = new List<string>();

                emails.Add(email);
                listMemberInfoInput lml2 = new listMemberInfoInput(m_apiKey, m_listID, emails);
                listMemberInfo lmi3 = new listMemberInfo(lml2);
                listMemberInfoOutput lmio2 = lmi3.Execute();
                if (lmio2 != null && lmio2.result != null)
                {
                    if (lmio2.result.data != null && lmio2.result.data.Count > 0)
                    {
                        listMemberInfoResults.MemberInfo mi = lmio2.result.data[0];
                        if (mi.status == EnumValues.listMembers_status.subscribed)
                        {
                            retVal = true;
                        }
                    }
                }
            }
            return retVal;
        }

        public override bool Subscribe(User user)
        {
            bool retVal = false;
            if (user != null && user.m_oBasicData != null)
            {
                Dictionary<string, object> mergeVars = new Dictionary<string, object>();
                mergeVars.Add("EMAIL", user.m_oBasicData.m_sEmail);
                mergeVars.Add("FNAME", user.m_oBasicData.m_sFirstName);
                mergeVars.Add("LNAME", user.m_oBasicData.m_sLastName);
                listSubscribeInput lsi = new listSubscribeInput(m_apiKey, m_listID, user.m_oBasicData.m_sEmail, mergeVars, EnumValues.emailType.html, false, true, false, true);
                listSubscribe cmd3 = new listSubscribe(lsi);
                listSubscribeOutput lso = cmd3.Execute();
                retVal = lso.result;
            }
            return retVal;
        }

        public override bool UnSubscribe(User user)
        {
            bool retVal = false;
            if (user != null && user.m_oBasicData != null)
            {
                listUnsubscribeInput lusi = new listUnsubscribeInput(m_apiKey, m_listID, user.m_oBasicData.m_sEmail, false, true, true);
                listUnsubscribe cmd3 = new listUnsubscribe(lusi);
                listUnsubscribeOutput lso = cmd3.Execute();
                retVal = lso.result;
            }
            return retVal;
        }
    }
}
