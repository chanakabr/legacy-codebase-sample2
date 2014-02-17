using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Web.Script.Serialization;
using System.Configuration;

namespace Users
{
    class YesUsers : TvinciUsers
    {
        public YesUsers(int nGroupID)
            : base(nGroupID)
        {

        }

        private Dictionary<string, string> GetYesUserDetails(string sURL, string sPostData) 
        {  
            string ret = string.Empty;                         
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            request.Method          = "POST";
            request.ContentType     = "application/x-www-form-urlencoded";         
            ASCIIEncoding encoding  = new ASCIIEncoding();
            byte[] data             = encoding.GetBytes(sPostData);
            request.ContentLength                = data.Length;
            request.CookieContainer              = new CookieContainer();
            request.MaximumAutomaticRedirections = 10;
            request.AllowAutoRedirect            = true;

            Dictionary<string, string> sData = new Dictionary<string,string>();

            try
            {
                using (Stream writer = request.GetRequestStream())
                {
                    writer.Write(data, 0, data.Length);
                }

                String sResponse = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sResponse = reader.ReadToEnd();
                    }
                }

                //Logger.Logger.Log("WEB, http://192.116.126.212/fullauth response", sResponse.ToString(), "WebResp");

                var jss = new JavaScriptSerializer();
                sData   = jss.Deserialize<Dictionary<string, string>>(sResponse);

            }
            catch
            {
            }

            return sData;
        }


        public override UserResponseObject SignInWithToken(string sToken, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            // Get Yes User Details
            string yesURL = TVinciShared.WS_Utils.GetTcmConfigValue("YesTokenAuth");
            string sPostData = string.Format("HOD={0}", sToken);//yesURL
            Dictionary<string, string> yesUserData = GetYesUserDetails(yesURL, sPostData);           
            return SignInYes(yesUserData, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }

        public override UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            // Get Yes User Details
            string yesURL = TVinciShared.WS_Utils.GetTcmConfigValue("YESFullAuth");
            string sPostData = string.Format("username={0}&password={1}", sUN, sPass);
            Dictionary<string, string> yesUserData = GetYesUserDetails(yesURL, sPostData);   
            return SignInYes(yesUserData, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }

        public UserResponseObject SignInYes( Dictionary<string, string> yesUserData , int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject o = null;         
            string sUserCoGuid = string.Empty;
            GetUserContentInfo(ref sUserCoGuid, "userUuid", yesUserData);  

            // If not valid return user not exist
            if (string.IsNullOrEmpty(sUserCoGuid))
            {
                o = new UserResponseObject();
                o.m_RespStatus = ResponseStatus.UserDoesNotExist;
                return o;
            }

            string sUN = string.Empty;
            // we are assuming the user name is in the yesUserData     
            GetUserContentInfo(ref sUN, "userName", yesUserData);

            // Check if user on Tvinci
            UserResponseObject userInfo = base.GetUserByCoGuid(sUserCoGuid, -1);
            if (userInfo.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                // Create Tvinci user - set password = username
                UserBasicData userBasic = GetUserBasicData(yesUserData, sUN.ToLower(), sUN);
                UserDynamicData userDynamic = GetUserDynamicData(yesUserData);

                userInfo = base.AddNewUser(userBasic, userDynamic, sUN.ToLower());
                if (userInfo.m_RespStatus != ResponseStatus.OK)
                {
                    o = new UserResponseObject();
                    o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                    return o;
                }
            }

           
            
            // Check if Domain exist
            string domainCoGuid = string.Empty;
            int userID = int.Parse(userInfo.m_user.m_sSiteGUID);
            Users.BaseDomain t = null;

            string ip = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "AddDomain", "domains", ip, ref sWSUserName, ref sWSPass);
            Utils.GetGroupID(sWSUserName, sWSPass, "AddDomain", ref t);

            if (t == null)
            {
                //Logger.Logger.Log("Creating Domain_WS Error", "Domain = " + t.ToString(), "Domains");
                o = new UserResponseObject();
                o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                return o;
            }

            GetUserContentInfo(ref domainCoGuid, "AccountUuid", yesUserData);

            // get TVinci domain ID, if 0 domain is not exist yet
            int domainID = t.GetDomainIDByCoGuid(domainCoGuid);
            // get TVinci associated domain id from user info
            int userTVinciDomainID = userInfo.m_user.m_domianID;

            Users.DomainResponseObject dr = null;

            // Create new TVDomain if domain CoGuid is not empty, TVinci user associated domain has no id and there is no domain related to that coGuid
            if (!string.IsNullOrEmpty(domainCoGuid) && userTVinciDomainID == 0 && domainID == 0)
            {
                dr = t.AddDomain(sUN + "/Domain", sUN + "/Domain", userID, nGroupID, domainCoGuid);

                if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
                {
                    // Error adding to domain
                    //Logger.Logger.Log("Add Domain Error", "Domain = " + t.ToString(), "Domains");
                    o = new UserResponseObject();
                    o.m_RespStatus = ResponseStatus.UserDoesNotExist;
                    return o;
                }
            }
            // join user to an active domain if Vinci user associated domain has no id, domain CoGuid is not empty and there is a domain related to that coGuid
            else if (!string.IsNullOrEmpty(domainCoGuid) && userTVinciDomainID == 0 && domainID != 0)
            {
                Domain oDomain = t.GetDomainInfo(domainID, nGroupID);

                // add user to domain
                dr = t.AddUserToDomain(nGroupID, domainID, userID, oDomain.m_masterGUIDs[0], false); 

                if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
                {
                    // Error join to domain
                    //Logger.Logger.Log("Join Domain Error", "Domain = " + t.ToString(), "Domains");
                    o = new UserResponseObject();
                    o.m_RespStatus = ResponseStatus.UserDoesNotExist;
                    return o;
                }

            }
            // if there is existing TVinci domain that associated with the current user return an error
            else if (domainID != 0 && domainID != userTVinciDomainID)
            {
                o = new UserResponseObject();
                o.m_RespStatus = ResponseStatus.UserDoesNotExist;
                return o;
            }

            // Tvinci signIn. password = username 
            return base.SignIn(sUN, sUN.ToLower(), nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        }


        #region OldSignIn
        //public override UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        //{
        //    UserResponseObject o = null;
        //    // Get Yes User Details
        //    Dictionary<string, string> yesUserData = GetYesUserDetails(sUN, sPass, false); //
        //    string sUserCoGuid = string.Empty;
        //    GetUserContentInfo(ref sUserCoGuid, "userUuid", yesUserData);

        //    // If not valid return user not exist
        //    if (string.IsNullOrEmpty(sUserCoGuid))
        //    {
        //        o = new UserResponseObject();
        //        o.m_RespStatus = ResponseStatus.UserDoesNotExist;
        //        return o;
        //    }

        //    // Check if user on Tvinci
        //    UserResponseObject userInfo = base.GetUserByCoGuid(sUserCoGuid, -1);
        //    if (userInfo.m_RespStatus == ResponseStatus.UserDoesNotExist)
        //    {
        //        // Create Tvinci user - set password = username
        //        UserBasicData userBasic = GetUserBasicData(yesUserData, sUN.ToLower(), sUN);
        //        UserDynamicData userDynamic = GetUserDynamicData(yesUserData);

        //        userInfo = base.AddNewUser(userBasic, userDynamic, sUN.ToLower());
        //        if (userInfo.m_RespStatus != ResponseStatus.OK)
        //        {
        //            o = new UserResponseObject();
        //            o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
        //            return o;
        //        }
        //    }

        //    // Check if Domain exist
        //    string domainCoGuid = string.Empty;
        //    int userID = int.Parse(userInfo.m_user.m_sSiteGUID);
        //    Users.BaseDomain t = null;

        //    string ip = "1.1.1.1";
        //    string sWSUserName = string.Empty;
        //    string sWSPass = string.Empty;
        //    TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "AddDomain", "domains", ip, ref sWSUserName, ref sWSPass);
        //    Utils.GetGroupID(sWSUserName, sWSPass, "AddDomain", ref t);

        //    if (t == null)
        //    {
        //        //Logger.Logger.Log("Creating Domain_WS Error", "Domain = " + t.ToString(), "Domains");
        //        o = new UserResponseObject();
        //        o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
        //        return o;
        //    }

        //    GetUserContentInfo(ref domainCoGuid, "AccountUuid", yesUserData);

        //    // get TVinci domain ID, if 0 domain is not exist yet
        //    int domainID = t.GetDomainIDByCoGuid(domainCoGuid);
        //    // get TVinci associated domain id from user info
        //    int userTVinciDomainID = userInfo.m_user.m_domianID;

        //    Users.DomainResponseObject dr = null;

        //    // Create new TVDomain if domain CoGuid is not empty, TVinci user associated domain has no id and there is no domain related to that coGuid
        //    if (!string.IsNullOrEmpty(domainCoGuid) && userTVinciDomainID == 0 && domainID == 0)
        //    {
        //        dr = t.AddDomain(sUN + "/Domain", sUN + "/Domain", userID, nGroupID, domainCoGuid);

        //        if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
        //        {
        //            // Error adding to domain
        //            //Logger.Logger.Log("Add Domain Error", "Domain = " + t.ToString(), "Domains");
        //            o = new UserResponseObject();
        //            o.m_RespStatus = ResponseStatus.UserDoesNotExist;
        //            return o;
        //        }
        //    }
        //    // join user to an active domain if Vinci user associated domain has no id, domain CoGuid is not empty and there is a domain related to that coGuid
        //    else if (!string.IsNullOrEmpty(domainCoGuid) && userTVinciDomainID == 0 && domainID != 0)
        //    {
        //        Domain oDomain = t.GetDomainInfo(domainID, nGroupID);

        //        // add user to domain
        //        dr = t.AddUserToDomain(nGroupID, domainID, userID, oDomain.m_masterGUIDs[0], false);

        //        if (dr == null || dr.m_oDomainResponseStatus != DomainResponseStatus.OK)
        //        {
        //            // Error join to domain
        //            //Logger.Logger.Log("Join Domain Error", "Domain = " + t.ToString(), "Domains");
        //            o = new UserResponseObject();
        //            o.m_RespStatus = ResponseStatus.UserDoesNotExist;
        //            return o;
        //        }

        //    }
        //    // if there is existing TVinci domain that associated with the current user return an error
        //    else if (domainID != 0 && domainID != userTVinciDomainID)
        //    {
        //        o = new UserResponseObject();
        //        o.m_RespStatus = ResponseStatus.UserDoesNotExist;
        //        return o;
        //    }

        //    // Tvinci signIn. password = username 
        //    return base.SignIn(sUN, sUN.ToLower(), nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
        //} 
        #endregion

        private void GetUserContentInfo(ref string subject, string value, Dictionary<string, string> userInfo)
        {
            if (userInfo.ContainsKey(value) == true)
            {
                subject = userInfo[value];
            }
        }

        private UserBasicData GetUserBasicData(Dictionary<string, string> yesUserData, string sPass, string sUN)
        {
            UserBasicData ubd = new UserBasicData();

            GetUserContentInfo(ref ubd.m_CoGuid, "userUuid", yesUserData);
            GetUserContentInfo(ref ubd.m_sEmail, "eMail", yesUserData);
            GetUserContentInfo(ref ubd.m_sFirstName, "FirstName", yesUserData);
            GetUserContentInfo(ref ubd.m_sLastName, "LastName", yesUserData);

            ubd.m_sPassword = sPass;
            ubd.m_sUserName = sUN;

            return ubd;
        }

        private List<string> userParams = new List<string>()
        {
            "AccountUuid",
            "accNum",
            "Type",
            "company",
            "Permission",
            "parentalControl",
            "Status",
            "siteTransfer",
            "TVcookie",
            "CrmUserId",
        };

        private UserDynamicData GetUserDynamicData(Dictionary<string, string> yesUserData)
        {
            UserDynamicData udd = new UserDynamicData();
            udd.m_sUserData     = new UserDynamicDataContainer[userParams.Count];

            for(int i = 0 ; i < userParams.Count ; ++i)
            {
                UserDynamicDataContainer dynamicData = new UserDynamicDataContainer();
                if (yesUserData.ContainsKey(userParams[i]) == true)
                {
                    dynamicData.m_sDataType = userParams[i];
                    dynamicData.m_sValue    = yesUserData[userParams[i]];
                    udd.m_sUserData[i]      = dynamicData;
                }
            }
            return udd;
        }
    }
}