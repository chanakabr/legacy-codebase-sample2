using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using ApiObjects;
using KLogMonitor;
using System.Reflection;

namespace Users
{
    class YesUsers : TvinciUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public YesUsers(int nGroupID)
            : base(nGroupID)
        {

        }

        private Dictionary<string, string> GetYesUserDetails(string sURL, string sPostData)
        {
            string ret = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(sPostData);
            request.ContentLength = data.Length;
            request.CookieContainer = new CookieContainer();
            request.MaximumAutomaticRedirections = 10;
            request.AllowAutoRedirect = true;

            Dictionary<string, string> sData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

                var jss = new JavaScriptSerializer();
                sData = jss.Deserialize<Dictionary<string, string>>(sResponse);

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
            string sPostData = string.Format("HOD={0}", sToken);//yesURL    /
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

        public UserResponseObject SignInYes(Dictionary<string, string> yesUserData, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject o = null;

            string sUN = string.Empty;

            // we are assuming the user name is in the yesUserData      
            GetUserContentInfo(ref sUN, "userName", yesUserData);
            log.Debug("SignInYes - sUN = " + sUN);

            if (string.IsNullOrEmpty(sUN))
            {
                o = new UserResponseObject();
                o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                return o;
            }

            // Get user permissions
            yesUserData["USER_PERMISSIONS"] = GetUserPermission(yesUserData);

            if (((!yesUserData.ContainsKey("AccountUuid")) ||
                 (yesUserData.ContainsKey("AccountUuid") && string.IsNullOrEmpty(yesUserData["AccountUuid"]))) &&
                ((!yesUserData.ContainsKey("userUuid")) ||
                 (yesUserData.ContainsKey("userUuid") && string.IsNullOrEmpty(yesUserData["userUuid"]))))
            {
                UserBasicData userBasic = GetUserBasicData(yesUserData, sUN.ToLower(), sUN);
                UserDynamicData userDynamic = GetUserDynamicData(yesUserData);

                o = new UserResponseObject();
                o.m_user = new User();
                o.m_user.Initialize(userBasic, userDynamic, m_nGroupID, sUN.ToLower());

                o.m_RespStatus = ResponseStatus.ErrorOnInitUser;
                return o;
            }

            // Check the user CoGuid stuff
            string sUserCoGuid = string.Empty;
            GetUserContentInfo(ref sUserCoGuid, "userUuid", yesUserData);

            string domainCoGuid = string.Empty;
            GetUserContentInfo(ref domainCoGuid, "AccountUuid", yesUserData);

            // Check recommendation flag, if 'Y' - do ORCA SignIn
            char recommendflag;
            if ((yesUserData.ContainsKey("recommendflag_rac")) &&
                (!string.IsNullOrEmpty(yesUserData["recommendflag_rac"])) &&
                (char.TryParse(yesUserData["recommendflag_rac"], out recommendflag)) &&
                (recommendflag == 'Y'))
            {
                try
                {
                    string orcaProxyURL = TVinciShared.WS_Utils.GetTcmConfigValue("ORCA_PROXY_URL");

                    if (!string.IsNullOrEmpty(orcaProxyURL))
                    {
                        OrcaProxy.Service op = new OrcaProxy.Service();
                        op.Url = orcaProxyURL;

                        if ((yesUserData.ContainsKey("accNum")) && (!string.IsNullOrEmpty(yesUserData["accNum"])) &&
                            (yesUserData.ContainsKey("CrmUserId")) && (!string.IsNullOrEmpty(yesUserData["CrmUserId"])))
                        {
                            string orcaToken = op.OrcaLogin(yesUserData["accNum"], yesUserData["CrmUserId"]);

                            if (!string.IsNullOrEmpty(orcaToken))
                            {
                                yesUserData["orcaToken"] = orcaToken;
                            }
                            else
                            {
                                yesUserData["orcaToken"] = string.Empty;
                            }
                        }
                    }
                }
                catch
                {
                    yesUserData["orcaToken"] = "OrcaLoginError";
                }
            }


            // Check if user exists in the DB
            UserResponseObject userInfo = base.GetUserByCoGuid(sUserCoGuid, -1);
            if (userInfo.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                // Create Tvinci user - set password = username
                UserBasicData userBasic = GetUserBasicData(yesUserData, sUN.ToLower(), sUN);
                UserDynamicData userDynamic = GetUserDynamicData(yesUserData);

                userInfo = base.AddNewUser(userBasic, userDynamic, sUN.ToLower());
                if (userInfo.m_RespStatus != ResponseStatus.OK)
                {
                    log.Error("Creating User Error - sUN = " + sUN + " Response = " + userInfo.m_RespStatus.ToString());
                    o = new UserResponseObject();
                    o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                    return o;
                }
            }
            else
            {
                //get the dynamic data from yes and update it in DB
                UserDynamicData userDynamic = GetUserDynamicData(yesUserData);
                List<KeyValuePair> lKeyValue = new List<KeyValuePair>();
                foreach (UserDynamicDataContainer container in userDynamic.m_sUserData)
                {
                    KeyValuePair kvp = new KeyValuePair(container.m_sDataType, container.m_sValue);
                    lKeyValue.Add(kvp);
                }

                SetUserDynamicData(userInfo.m_user.m_sSiteGUID, lKeyValue, userInfo);
            }


            // Check if the Domain exist
            int userID = int.Parse(userInfo.m_user.m_sSiteGUID);
            Users.BaseDomain t = null;

            string ip = "1.1.1.1";
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "AddDomain", "domains", ip, ref sWSUserName, ref sWSPass);
            Utils.GetGroupID(sWSUserName, sWSPass, "AddDomain", ref t);

            if (t == null)
            {
                log.Error("Creating Domain_WS Error - Domain = " + t.ToString());
                o = new UserResponseObject();
                o.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                return o;
            }


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

        //in case of Yes - the new domain is added seperatly, this is a dummy function
        public override DomainResponseObject AddNewDomain(string sUN, int nUserID, int nGroupID)
        {
            DomainResponseObject dr = new DomainResponseObject();
            dr.m_oDomainResponseStatus = DomainResponseStatus.OK;
            return dr;
        }

        private void GetUserContentInfo(ref string subject, string value, Dictionary<string, string> userInfo)
        {
            if (userInfo.ContainsKey(value))
            {
                subject = userInfo[value];
            }
            else if (userInfo.ContainsKey(value.ToLower()))
            {
                subject = userInfo[value.ToLower()];
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

        private List<string> _userParams = new List<string>()
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

            // Version 4
            "userUuid",
            "userlimit_rac",
            "devicelimit_rac",
            "accountstatus_rac",
            "tvestatus_rac",
            "tvestatusdesc_rac",
            "satstatus_rac",
            "indicationflag_rac",
            "permissionid_rac",
            "recommendflag_rac",
            "eulaFlag_rac",
            "offerid_rac",
            "USER_PERMISSIONS",
            "orcaToken"
        };

        private UserDynamicData GetUserDynamicData(Dictionary<string, string> yesUserData)
        {
            UserDynamicData udd = new UserDynamicData();
            udd.m_sUserData = new UserDynamicDataContainer[_userParams.Count];

            for (int i = 0; i < _userParams.Count; ++i)
            {
                UserDynamicDataContainer dynamicData = new UserDynamicDataContainer();

                if (yesUserData.ContainsKey(_userParams[i]))
                {
                    dynamicData.m_sDataType = _userParams[i];
                    dynamicData.m_sValue = yesUserData[_userParams[i]];
                    udd.m_sUserData[i] = dynamicData;
                }
            }

            udd.m_sUserData = udd.m_sUserData.Where(d => d != null).ToArray();

            return udd;
        }

        // WARNING: Crazy code below
        public string GetUserPermission(Dictionary<string, string> yesUserData)
        {
            // OK || LOGIN_MSG_{Excel line number}
            string sRet = "OK";

            try
            {
                ushort tveStatus = 0;
                ushort.TryParse(yesUserData["tvestatus_rac"], out tveStatus);

                ushort satstatus = 0;
                ushort.TryParse(yesUserData["satstatus_rac"], out satstatus);

                if ((satstatus == 3) &&
                    ((!yesUserData.ContainsKey("AccountUuid")) ||
                     (yesUserData.ContainsKey("AccountUuid") && string.IsNullOrEmpty(yesUserData["AccountUuid"]))))
                {
                    sRet = "LOGIN_MSG_115";
                    return sRet;
                }

                if (((!yesUserData.ContainsKey("AccountUuid")) ||
                     (yesUserData.ContainsKey("AccountUuid") && string.IsNullOrEmpty(yesUserData["AccountUuid"]))) &&
                    ((!yesUserData.ContainsKey("userUuid")) ||
                     (yesUserData.ContainsKey("userUuid") && string.IsNullOrEmpty(yesUserData["userUuid"]))))
                {
                    if (tveStatus == 3)
                    {
                        sRet = "LOGIN_MSG_1151";
                        return sRet;
                    }

                    //if (satstatus == 3)
                    //{
                    //    sRet = "LOGIN_MSG_115";
                    //    return sRet;
                    //}

                    if (tveStatus != 1)
                    {
                        sRet = "LOGIN_MSG_109";
                        return sRet;
                    }
                }


                ushort indicationflag = 0;
                ushort.TryParse(yesUserData["indicationflag_rac"], out indicationflag);

                string sType = yesUserData["Type"];

                if ((yesUserData.ContainsKey("AccountUuid") && !string.IsNullOrEmpty(yesUserData["AccountUuid"])) &&
                    ((string.Compare(sType, "slv", StringComparison.OrdinalIgnoreCase) == 0)))
                {
                    switch (indicationflag)
                    {
                        case 1:
                            sRet = "LOGIN_MSG_1031";
                            return sRet;

                        case 2:
                            sRet = "LOGIN_MSG_1041";
                            return sRet;

                        case 3:
                            sRet = "LOGIN_MSG_1043";
                            return sRet;
                    }
                }


                // Check accNum, AccountUuid, CrmUserId, userUuid are not NULL 
                if (((yesUserData.ContainsKey("accNum") && !string.IsNullOrEmpty(yesUserData["accNum"])) &&
                    (yesUserData.ContainsKey("AccountUuid") && !string.IsNullOrEmpty(yesUserData["AccountUuid"])) &&
                    (yesUserData.ContainsKey("CrmUserId") && !string.IsNullOrEmpty(yesUserData["CrmUserId"])) &&
                    (yesUserData.ContainsKey("userUuid") && !string.IsNullOrEmpty(yesUserData["userUuid"]))) == false)
                {
                    //if (tveStatus == 3)
                    //{
                    //    sRet = "LOGIN_MSG_1151";
                    //    return sRet;
                    //}

                    if (tveStatus == 1)
                    {
                        sRet = "LOGIN_MSG_108";
                        return sRet;
                    }

                    sRet = "LOGIN_MSG_109";
                    return sRet;
                }

                switch (tveStatus)
                {
                    case 1:
                        sRet = "LOGIN_MSG_108";
                        return sRet;

                    case 4:
                        sRet = "LOGIN_MSG_105";
                        return sRet;

                    case 5:
                        sRet = "LOGIN_MSG_106";
                        return sRet;

                    case 2:
                    case 9:
                        //ushort satstatus = 0;
                        //ushort.TryParse(yesUserData["satstatus_rac"], out satstatus);

                        if (satstatus == 3)
                        {
                            sRet = "LOGIN_MSG_115";
                            return sRet;
                        }
                        else if (satstatus == 1 || satstatus == 2)
                        {
                            //string sType = yesUserData["Type"];
                            bool isMaster = (string.Compare(sType, "mas", StringComparison.OrdinalIgnoreCase) == 0);
                            bool isSlave = (string.Compare(sType, "slv", StringComparison.OrdinalIgnoreCase) == 0);
                            //bool isTypeReg = (string.Compare(sType, "reg", StringComparison.OrdinalIgnoreCase) == 0);

                            switch (indicationflag)
                            {
                                case 1:
                                    if (isMaster)
                                    {
                                        sRet = "LOGIN_MSG_103";
                                    }
                                    else if (isSlave)
                                    {
                                        sRet = "LOGIN_MSG_1031";
                                    }
                                    return sRet;

                                case 2:
                                    if (isMaster)
                                    {
                                        sRet = "LOGIN_MSG_104";
                                    }
                                    else if (isSlave)
                                    {
                                        sRet = "LOGIN_MSG_1041";
                                    }
                                    return sRet;

                                case 3:
                                    if (isMaster)
                                    {
                                        sRet = "LOGIN_MSG_1042";
                                        return sRet;
                                    }

                                    if (isSlave)
                                    {
                                        sRet = "LOGIN_MSG_1043";
                                        return sRet;
                                    }
                                    break;

                                case 0:
                                    ushort eulaFlag = 0;
                                    ushort.TryParse(yesUserData["eulaFlag_rac"], out eulaFlag);

                                    if (eulaFlag == 0)
                                    {
                                        sRet = "LOGIN_MSG_1111";    // New EULA message
                                        return sRet;
                                    }

                                    if (eulaFlag == 1)
                                    {
                                        string sStatus = yesUserData["Status"];
                                        if (isSlave &&
                                            string.Compare(sStatus, "TDS", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            sRet = "LOGIN_MSG_107";
                                            return sRet;
                                        }

                                        // else
                                        if (isSlave &&
                                            string.Compare(sStatus, "PN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            sRet = "LOGIN_MSG_108";
                                            return sRet;
                                        }

                                        sRet = "OK";
                                        return sRet;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            sRet = "TVESTATUS_ERROR";
                            return sRet;
                        }

                        break;
                }


            }
            catch (Exception)
            {
                sRet = "ERROR";
                return sRet;
            }

            #region Commented
            //"AccountUuid",
            //"accNum",
            //"Type",
            //"company",
            //"Permission",
            //"parentalControl",
            //"Status",
            //"siteTransfer",
            //"TVcookie",
            //"CrmUserId",

            //// Version 4
            //"userUuid",
            //"userlimit_rac",
            //"devicelimit_rac",
            //"accountstatus_rac",
            //"tvestatus_rac",
            //"tvestatusdesc_rac",
            //"satstatus_rac",
            //"indicationflag_rac",
            //"permissionid_rac",
            //"recommendflag _rac",
            //"eulaflag_rac",
            //"offerId_rac"


            //try
            //{
            //    using (YesServices.UserPermission.WServiceTVE service = new YesServices.UserPermission.WServiceTVE())
            //    {
            //        string xml = service.GetAccontPermissions(sUserName);

            //        string indicationflag = string.Empty;
            //        if (GetTagValue(ref xml, "indicationflag", out indicationflag))
            //        {
            //            switch (indicationflag)
            //            {
            //                case "1":
            //                    sRet = "LOGIN_MSG_103";
            //                    goto Exit;
            //                case "2":
            //                    sRet = "LOGIN_MSG_104";
            //                    goto Exit;
            //                default:
            //                    break;
            //            }
            //        }


            //        string accountuuid = string.Empty, tvestatus = string.Empty;
            //        if (GetTagValue(ref xml, "accountuuid", out accountuuid))
            //        {
            //            if (string.IsNullOrEmpty(accountuuid))
            //            {
            //                sRet = "LOGIN_MSG_116";
            //                goto Exit;
            //            }
            //        }

            //        if (GetTagValue(ref xml, "tvestatus", out tvestatus))
            //        {
            //            switch (tvestatus)
            //            {
            //                case "1":
            //                    sRet = "LOGIN_MSG_108";
            //                    goto Exit;
            //                case "3":
            //                    sRet = "LOGIN_MSG_109";
            //                    goto Exit;
            //                case "4":
            //                    sRet = "LOGIN_MSG_105";
            //                    goto Exit;
            //                case "5":
            //                    sRet = "LOGIN_MSG_106";
            //                    goto Exit;
            //                //case "9":
            //                //    sRet = "LOGIN_MSG_112";
            //                //    goto Exit;
            //                default:
            //                    break;
            //            }
            //        }
            //        string accountstatus = string.Empty;
            //        if (GetTagValue(ref xml, "accountstatus", out accountstatus))
            //        {
            //            if (sUserType.ToLower() != "mas")
            //            {
            //                switch (accountstatus)
            //                {
            //                    case "PN":
            //                        sRet = "LOGIN_MSG_115";
            //                        goto Exit;
            //                    case "FA":
            //                        sRet = "LOGIN_MSG_114";
            //                        goto Exit;
            //                    case "IN":
            //                        sRet = "LOGIN_MSG_107";
            //                        goto Exit;
            //                    default:
            //                        break;
            //                }
            //            }
            //        }

            //    }

            ////Exit:
            ////    logger.DebugFormat("GetUserPermission:: Finish request with : {0}", sRet);
            //}
            //catch (Exception ex)
            //{
            //    //logger.DebugFormat("GetUserPermission::GetAccontPermissions(YesServices)-> Error occured, Exception:{0}", ex.ToString());
            //}
            #endregion

            return sRet;
        }
    }
}