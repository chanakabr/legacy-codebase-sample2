using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using KLogMonitor;
using Core.Users;
using ApiObjects;

namespace WS_Users
{
    public partial class SSO : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected void Page_Load(object sender, EventArgs e)
        {
            string sSiteGUID = Request.QueryString["siteGuid"];

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                sSiteGUID = "0";

                string sGroupID = Request.QueryString["group_id"];
                string sContentProviderID = Request.QueryString["cp_id"];
                string sToolboxToken = Request.QueryString["toolbox_user_token"];

                int nGroupID;

                int.TryParse(sGroupID, out nGroupID);

                if (nGroupID != 0)
                {
                    try
                    {
                        BaseUsers t = new TurnerUsers(nGroupID);

                        UserResponseObject userResponseObject = signIn(t, sContentProviderID, sToolboxToken, 3, 3, nGroupID, string.Empty, string.Empty, string.Empty, false);

                        if (userResponseObject != null && userResponseObject.m_RespStatus == ResponseStatus.OK)
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(userResponseObject.m_user.m_sSiteGUID);
                            sSiteGUID = System.Convert.ToBase64String(bytes);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("SignInWithToken", ex);
                    }
                }

                string sRedirectURL = string.Format("{0}?siteGuid={1}&sessionId={2}", Request.Url.AbsolutePath, sSiteGUID, sToolboxToken);

                Response.Redirect(sRedirectURL);
            }
        }

        private TurnerUserDetails getUserDetails(string sContentProviderID, string sToken)
        {
            TurnerUserDetails res = null;

            string sURL = string.Format(TVinciShared.WS_Utils.GetTcmConfigValue("TurnerTokenAuth"), sContentProviderID, sToken);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(sURL);

            try
            {
                String sResponse = string.Empty;

                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        sResponse = reader.ReadToEnd();
                    }
                }

                res = new JavaScriptSerializer().Deserialize<TurnerUserDetails>(sResponse);
            }
            catch (Exception ex)
            {
                log.Error("GetUserDetails", ex);
            }

            return res;
        }

        private UserResponseObject signIn(BaseUsers t, string sContentProviderID, string sToolboxToken, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            TurnerUserDetails turnerUserDetails = getUserDetails(sContentProviderID, sToolboxToken);

            if (turnerUserDetails == null || string.IsNullOrEmpty(turnerUserDetails.identifier))
                return new UserResponseObject() { m_RespStatus = ResponseStatus.ErrorOnInitUser };

            if (!turnerUserDetails.authorized)
                return new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };

            string sUserName = string.Empty;
            string sPassword = string.Empty;

            UserResponseObject userResponseObject = t.GetUserByCoGuid(turnerUserDetails.identifier, -1);

            if (userResponseObject.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                UserBasicData userBasic = getUserBasicData(turnerUserDetails);
                UserDynamicData userDynamic = getUserDynamicData(turnerUserDetails);

                userResponseObject = t.AddNewUser(userBasic, userDynamic, userBasic.m_sPassword);

                if (userResponseObject.m_RespStatus != ResponseStatus.OK)
                {
                    return userResponseObject;
                }

                sUserName = userBasic.m_sUserName;
                sPassword = userBasic.m_sPassword;
            }
            else
            {
                sUserName = userResponseObject.m_user.m_oBasicData.m_sUserName;
                sPassword = userResponseObject.m_user.m_oBasicData.m_sPassword;
            }

            return t.SignIn(sUserName, sPassword, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins); ;
        }

        private UserBasicData getUserBasicData(TurnerUserDetails turnerUserDetails)
        {
            UserBasicData userBasicData = new UserBasicData();

            userBasicData.m_CoGuid = turnerUserDetails.identifier;

            string sUserName = turnerUserDetails.identifier.Substring(0, 10) + DateTime.Now.Ticks;

            userBasicData.m_sUserName = sUserName;
            userBasicData.m_sPassword = sUserName;
            userBasicData.m_sEmail = sUserName;

            userBasicData.m_Country = new Core.Users.Country();
            userBasicData.m_Country.InitializeByCode(turnerUserDetails.country);

            return userBasicData;
        }

        private UserDynamicData getUserDynamicData(TurnerUserDetails turnerUserDetails)
        {
            UserDynamicData userDynamicData = new UserDynamicData();

            userDynamicData.m_sUserData = new UserDynamicDataContainer[] { new UserDynamicDataContainer(){
                m_sDataType = "mso",
                m_sValue = turnerUserDetails.mso
            }};

            return userDynamicData;
        }

        private class TurnerUserDetails
        {
            public bool authorized { get; set; }
            public string mso { get; set; }
            public string identifier { get; set; }
            public string country { get; set; }
        }
    }

    class TurnerUsers : TvinciUsers
    {
        public TurnerUsers(int nGroupID)
            : base(nGroupID)
        {
            this.m_mailImpl = new TurnerMailImpl(nGroupID, 0);
        }
    }

    class TurnerMailImpl : BaseMailImpl
    {
        public TurnerMailImpl(int nGroupID, int nRuleID)
            : base(nGroupID, nRuleID)
        {
        }

        public override bool SendMail(User user)
        {
            return true;
        }
    }
}