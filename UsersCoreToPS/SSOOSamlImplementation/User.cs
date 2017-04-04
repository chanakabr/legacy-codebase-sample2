using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using Core.Users;
using Core.Users.Saml;

namespace SSOOSamlImplementation
{
    public class User : KalturaSSOUsers, ISSOProvider
    {
        OSamlUserDetails userDetails = null;
        SamlProviderObject prov = null;

        public User(int nGroupID, int operatorId)
            : base(nGroupID, operatorId) { }

        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            // get operation ID from key-value pair list
            int operatorId;
            var keyValueOperatorId = keyValueList.FirstOrDefault(x => x.key == "operator");
            if (keyValueOperatorId != null)
                operatorId = Convert.ToInt32(keyValueOperatorId.value);
            else
                return new UserResponseObject() { m_RespStatus = ResponseStatus.InternalError };

            prov = OSamlUtils.Get_ProviderDetails(operatorId);
            if (prov != null)
            {
                DataTable dt = DAL.SSODal.IsUserExsits(userName, operatorId);

                if (dt != null && dt.DefaultView.Count > 0)
                {
                    userDetails = new OSamlUserDetails()
                    {
                        SiteGuid = dt.Rows[0]["user_site_guid"].ToString(),
                        CoGuid = dt.Rows[0]["CO_GUID"].ToString(),
                    };

                    siteGuid = int.Parse(userDetails.SiteGuid);
                    return new UserResponseObject();
                }
                else
                    return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
            }
            else
                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
        }

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            //return this.MidSignIn(sUserName, string.Empty, nOperatorID, 0, 0, string.Empty, string.Empty, string.Empty, false);
            // TODO: talk to Michael Mars about this!
            return null;
        }

        private class OSamlUserDetails
        {
            public string CoGuid { get; set; }
            public string SiteGuid { get; set; }
        }
    }
}
