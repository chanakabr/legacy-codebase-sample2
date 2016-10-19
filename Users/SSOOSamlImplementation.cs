using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TVinciShared;
using Users.Saml;

namespace Users
{
    public class SSOOSamlImplementation : SSOUsers, ISSOProvider
    {
        OSamlUserDetails userDetails = null;
        SamlProviderObject prov = null;

        public SSOOSamlImplementation(int nGroupID, int operatorId)
            : base(nGroupID, operatorId)
        {
        }

        public override UserResponseObject SignIn(string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            prov = OSamlUtils.Get_ProviderDetails(nOperatorID);
            if (prov != null)
            {
                DataTable dt = DAL.SSODal.IsUserExsits(sCoGuid, nOperatorID);

                if (dt != null && dt.DefaultView.Count > 0)
                {
                    userDetails = new OSamlUserDetails()
                    {
                        SiteGuid = dt.Rows[0]["user_site_guid"].ToString(),
                        CoGuid = dt.Rows[0]["CO_GUID"].ToString(),
                    };
                    return User.SignIn(int.Parse(userDetails.SiteGuid), nMaxFailCount, nLockMinutes, m_nGroupID, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                }
                else
                {
                    return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
                }
            }
            else
                return new UserResponseObject() { m_RespStatus = ResponseStatus.UserDoesNotExist };
        }

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            return this.SignIn(sUserName, string.Empty, nOperatorID, 0, 0, string.Empty, string.Empty, string.Empty, false);
        }

        private class OSamlUserDetails
        {
            public string CoGuid { get; set; }
            public string SiteGuid { get; set; }        
        }
    }
}
