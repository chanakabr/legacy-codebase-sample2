using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVinciShared;
using Core.Social;
using System.Web.Script.Serialization;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Specialized;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using Core.Billing;
using ApiObjects.Social;
using ApiObjects.Billing;

namespace WS_Social
{
    public partial class facebook_api : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected string GetFBURL()
        {
            int nGroupID = (!string.IsNullOrEmpty(GetQueryValue("groupId"))) ? int.Parse(GetQueryValue("groupId")) : 0;
            string platform = (!string.IsNullOrEmpty(GetQueryValue("platform"))) ? GetQueryValue("platform") : "0";

            string display = GetDisplay(platform);

            FacebookManager fbManager = FacebookManager.GetInstance;

            FacebookConfig fbc = null;

            if (fbManager != null)
            {
                fbc = fbManager.GetFacebookConfigInstance(nGroupID);
            }

            string url = string.Empty;

            if (Session["logout"] != null)
            {
                string token = Session["logout"].ToString();
                Session["logout"] = null;

                url = string.Format("https://www.facebook.com/logout.php?next={0}&access_token={1}",
                    HttpUtility.UrlEncode(fbc.sFBCallback + "&logout=1"), token);

            }
            else
            {
                url = string.Format("https://www.facebook.com/dialog/oauth/?display={0}&client_id={1}&redirect_uri={2}&scope={3}",
                    display,
                    fbc.sFBKey,
                    HttpUtility.UrlEncode(fbc.sFBCallback),
                    fbc.sFBPermissions);
            }

            return url;
        }

        protected string IsInitFB()
        {
            bool flag = ((Session["error"] == null) && (string.IsNullOrEmpty(Request["code"]) || Session["logout"] != null));

            return flag.ToString().ToLower();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Session["error"] = null;
                string error = Request.QueryString.Get("error");
                string code = Request.QueryString.Get("code");

                if (!string.IsNullOrEmpty(error))
                {
                    Session["error"] = error;

                    throw new FacebookException(FacebookResponseStatus.ERROR, "Facebook callback error");
                }

                FacebookManager fbManager = FacebookManager.GetInstance;


                //Callback from facebook - dialog
                if (!string.IsNullOrEmpty(code))
                {
                    FacebookResponseObject fro = new FacebookResponseObject();
                    string jsonString = string.Empty;

                    string platform = GetQueryValue("platform");
                    string cbURL = GetQueryValue("callbackURL");

                    if (string.IsNullOrEmpty(cbURL) && string.IsNullOrEmpty(platform))
                    {
                        throw new FacebookException(FacebookResponseStatus.ERROR, "missing callback url");
                    }

                    int nGroupID = (!string.IsNullOrEmpty(GetQueryValue("groupId"))) ? int.Parse(GetQueryValue("groupId")) : 0;

                    FacebookConfig fbc = null;

                    if (fbManager != null)
                    {
                        fbc = fbManager.GetFacebookConfigInstance(nGroupID);
                    }

                    //facebook handshake 
                    string url = string.Format("{0}/oauth/access_token?client_id={1}&redirect_uri={2}&client_secret={3}&code={4}",
                        Core.Social.Utils.GetValFromConfig("FB_GRAPH_URI"),
                        fbc.sFBKey,
                        HttpUtility.UrlEncode(fbc.sFBCallback),
                        fbc.sFBSecret,
                        code);

                    int nResStatus = 0;
                    int counter = 0;
                    string tokenResp = string.Empty;
                    string sAccessToken = string.Empty;

                    while (true)
                    {
                        tokenResp = Core.Social.Utils.SendXMLHttpReq(url, string.Empty, string.Empty, ref nResStatus);

                        if (nResStatus != 200)
                        {
                            log.Error("SendXMLHttpReq error - try : " + counter.ToString() + ", statuscode : " + nResStatus.ToString() + ", error : " + tokenResp);
                            if (counter == 3)
                            {
                                throw new FacebookException(FacebookResponseStatus.ERROR, tokenResp); //Exception(tokenResp);
                            }
                        }
                        else
                        {
                            break;
                        }
                        counter++;
                    }


                    if (!string.IsNullOrEmpty(tokenResp) && tokenResp.Contains("access_token"))
                    {

                        string[] seperator = { "&" };
                        string[] splited = tokenResp.Split(seperator, StringSplitOptions.None);
                        if (splited.Length > 0)
                        {
                            sAccessToken = splited[0].Remove(0, 13);
                        }

                    }
                    else
                    {
                        throw new FacebookException(FacebookResponseStatus.ACCESSDENIED, "tokenResp : " + tokenResp);
                    }

                    //Get facebook user
                    string sRetVal = string.Empty;
                    int status = FBUtils.GetGraphApiAction("me?fields=id,name,first_name,last_name,email,gender,birthday,location,interests.limit(500)", string.Empty, sAccessToken, ref sRetVal);

                    if (status != 200)
                    {
                        //Error with facebook response
                        throw new FacebookException(FacebookResponseStatus.ERROR, "graph.facebook.com/me");
                    }

                    //Create FBUser
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    FBUser fbUser = serializer.Deserialize<FBUser>(sRetVal);

                    if (string.IsNullOrEmpty(fbUser.email))
                    {
                        throw new FacebookException(FacebookResponseStatus.ERROR, "Missing user email");
                    }

                    string action = (Session["action"] != null) ? Session["action"].ToString() : string.Empty;
                    switch (action)
                    {
                        case "getdata":
                            {
                                jsonString = GetData(fbUser, nGroupID, sAccessToken).ToJSON();
                                break;
                            }
                        case "register":
                            {
                                //int domain = (!string.IsNullOrEmpty(GetQueryValue("domain"))) ? int.Parse(GetQueryValue("domain")) : 0;
                                jsonString = Register(GetQueryValue("siteGuid"), nGroupID, fbUser, sAccessToken).ToJSON();
                                break;
                            }
                        case "logout":
                            {
                                Session["logout"] = sAccessToken;
                                Session["action"] = "getdata";

                                break;
                            }
                        case "share":
                            {
                                string sShareURL = GetQueryValue("shareURL"); //Session["shareURL"] != null ? (string)Session["shareURL"] : string.Empty;
                                string shareUrl = string.Format("http://www.facebook.com/sharer/sharer.php?u={0}", sShareURL);
                                Response.Redirect(shareUrl);
                                break;
                            }
                        default:
                            {
                                fro.status = FacebookResponseStatus.NOACTION.ToString();
                                jsonString = fro.ToJSON();
                                break;
                            }
                    }

                    if (Session["logout"] == null)
                    {
                        if (!string.IsNullOrEmpty(platform) && platform != "10")
                        {
                            Response.Write("<div id='fbResponse' style='display:none'>" + jsonString + "</div>");
                        }
                        else
                        {
                            if (platform == "10")
                            {
                                Response.Redirect(string.Concat(HttpUtility.UrlDecode(cbURL), "?res=") + HttpUtility.UrlEncode(HttpUtility.UrlEncode(jsonString)));
                            }

                            innerIframe.Visible = true;
                            iframe.Attributes.Add("src", string.Concat(HttpUtility.UrlDecode(cbURL), "?res=") + HttpUtility.UrlEncode(HttpUtility.UrlEncode(jsonString)));

                        }
                    }
                }
                else if (string.IsNullOrEmpty(Request.QueryString.Get("logout")))
                {
                    Session["QueryString"] = Request.QueryString;
                    Session["action"] = Request["action"];

                    if (Session["action"] == null)
                    {
                        int nGroupID = int.Parse(Request.QueryString.Get("groupId"));

                        FacebookConfig fbc = null;

                        if (fbManager != null)
                        {
                            fbc = fbManager.GetFacebookConfigInstance(nGroupID);
                        }

                        Response.Redirect(fbc.sFBRedirect);
                    }
                }
            }
            catch (FacebookException ex)
            {
                Session["error"] = "ERROR";

                log.Error("FacebookException - " + ex.status.ToString() + ", " + ex.Message, ex);

                FacebookResponseObject fro = new FacebookResponseObject();
                fro.status = ex.status.ToString();
                fro.data = ex.Message;

                string jsonString = fro.ToJSON();

                string platform = GetQueryValue("platform");
                string cbURL = GetQueryValue("callbackURL");

                if (!string.IsNullOrEmpty(cbURL) || !string.IsNullOrEmpty(platform))
                {
                    if (!string.IsNullOrEmpty(platform) && platform != "10")
                    {
                        Response.Write("<div id='fbResponse' style='display:none'>" + jsonString + "</div>");
                    }
                    else
                    {
                        if (platform == "10")
                        {
                            Response.Redirect(string.Concat(HttpUtility.UrlDecode(cbURL), "?res=") + HttpUtility.UrlEncode(HttpUtility.UrlEncode(jsonString)));
                            return;
                        }

                        innerIframe.Visible = true;
                        iframe.Attributes.Add("src", string.Concat(HttpUtility.UrlDecode(cbURL), "?res=") + HttpUtility.UrlEncode(jsonString));
                    }
                }
                else
                {
                    Response.Write("<div id='fbResponse' style='display:none'>" + jsonString + "</div>");
                    Response.Write("Error occurred");
                }

                Lbl1.Text = "Please Close and try again";
                Image1.Visible = false;
            }
            catch (Exception ex)
            {
                log.Error("Facebook error - " + GetStatusToLog() + "; error:" + ex.Message);

                FacebookResponseObject fro = new FacebookResponseObject();
                fro.status = FacebookResponseStatus.ERROR.ToString();
                fro.data = ex.Message;

                string jsonString = fro.ToJSON();

                Lbl1.Text = "Please close and try again";
                Image1.Visible = false;
                Response.Write("<div id='fbResponse' style='display:none'>" + jsonString + "</div>");
                Response.Write("Error occurred");
            }
        }

        private FacebookResponseObject GetData(FBUser fbUser, Int32 nGroupID, string accessToken)
        {
            FacebookResponseObject fro = new FacebookResponseObject();

            FacebookManager fbManager = FacebookManager.GetInstance;

            FacebookConfig fbc = null;

            if (fbManager != null)
            {
                fbc = fbManager.GetFacebookConfigInstance(nGroupID);
            }
            //Search user with facebook id 
            UserResponseObject uObj = Core.Social.Utils.GetUserDataByFacebookID(fbUser.id, nGroupID);

            string key = Core.Social.Utils.GetValFromConfig("FB_TOKEN_KEY");
            string sEncryptToken = Core.Social.Utils.Encrypt(accessToken, key);

            fro.fbUser = fbUser;
            fro.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbUser.id);
            fro.facebookName = fbUser.name;

            //User Exists
            if (uObj.m_RespStatus == ResponseStatus.OK)
            {
                string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;
                fro.status = FacebookResponseStatus.OK.ToString();

                //Update user FBToken
                if (sFBToken != sEncryptToken)
                {
                    uObj.m_user.m_oBasicData.m_sFacebookToken = sEncryptToken;

                    uObj = Core.Social.Utils.SetUserData(nGroupID, uObj.m_user.m_sSiteGUID, uObj.m_user.m_oBasicData, uObj.m_user.m_oDynamicData);
                    fro.status = uObj.m_RespStatus.ToString().ToUpper();
                }

                fro.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;
                fro.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;

                fro.data = HttpUtility.UrlEncode(Core.Social.Utils.GetEncryptPass(fro.siteGuid));
            }
            // User Does Not Exists
            if (uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                //serach user by facebook email as username 
                uObj = Core.Social.Utils.GetUserByUsername(fbUser.email, nGroupID);
                if (uObj.m_RespStatus == ResponseStatus.UserDoesNotExist)
                {
                    fro.status = FacebookResponseStatus.NOTEXIST.ToString();
                    fro.siteGuid = string.Empty;
                    fro.facebookName = fbUser.name;
                    fro.token = HttpUtility.UrlEncode(sEncryptToken);

                    List<FBUser> lFreindsList;

                    FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);

                    Int32 nNumOfFriends;

                    bool bFriendsList = fbWrapper.GetFriendsList("me", accessToken, out nNumOfFriends, out lFreindsList);

                    if (nNumOfFriends < fbc.nFBMinFriends)
                    {
                        fro.status = FacebookResponseStatus.MINFRIENDS.ToString();
                        fro.data = nNumOfFriends.ToString();
                        fro.minFriends = fbc.nFBMinFriends.ToString();
                        fro.facebookName = fbUser.name;
                        fro.token = string.Empty;
                    }
                }
                else
                {
                    string sFacebookID = uObj.m_user.m_oBasicData.m_sFacebookID;
                    if (!string.IsNullOrEmpty(sFacebookID))
                    {
                        fro.status = FacebookResponseStatus.CONFLICT.ToString();
                        fro.tvinciName = uObj.m_user.m_oBasicData.m_sUserName;
                    }
                    else
                    {
                        fro.status = FacebookResponseStatus.MERGE.ToString();
                        fro.siteGuid = uObj.m_user.m_sSiteGUID;
                        fro.tvinciName = uObj.m_user.m_oBasicData.m_sUserName;
                    }
                }
            }

            return fro;
        }

        private FacebookResponseObject Register(string sSiteGUID, Int32 nGroupID, FBUser fbUser, string accessToken)
        {
            UserResponseObject uObj = new UserResponseObject();
            FacebookResponseObject fro = new FacebookResponseObject();

            string key = Core.Social.Utils.GetValFromConfig("FB_TOKEN_KEY");
            string sEncryptToken = Core.Social.Utils.Encrypt(accessToken, key);

            FacebookManager fbManager = FacebookManager.GetInstance;

            FacebookConfig fbc = null;

            if (fbManager != null)
            {
                fbc = fbManager.GetFacebookConfigInstance(nGroupID);
            }

            FacebookWrapper fbWrapper = new FacebookWrapper(nGroupID);

            fro.fbUser = fbUser;
            fro.pic = string.Format("http://graph.facebook.com/{0}/picture?type=normal", fbUser.id);
            fro.facebookName = fbUser.name;

            if (!string.IsNullOrEmpty(sSiteGUID))
            {
                uObj = Core.Social.Utils.GetUserDataByID(sSiteGUID, nGroupID);

                if (uObj.m_RespStatus == ResponseStatus.OK)
                {
                    string sFacebookID = uObj.m_user.m_oBasicData.m_sFacebookID;
                    string sFBToken = uObj.m_user.m_oBasicData.m_sFacebookToken;
                    string sFacebookImage = uObj.m_user.m_oBasicData.m_sFacebookImage;

                    if (sFBToken != sEncryptToken || sFacebookID != fbUser.id || sFacebookImage != fro.pic)
                    {
                        uObj.m_user.m_oBasicData.m_sFacebookID = fbUser.id;
                        uObj.m_user.m_oBasicData.m_sFacebookToken = sEncryptToken;
                        uObj.m_user.m_oBasicData.m_sFacebookImage = fro.pic;
                        //dynData.Where<
                        uObj = Core.Social.Utils.SetUserData(nGroupID, sSiteGUID, uObj.m_user.m_oBasicData, uObj.m_user.m_oDynamicData);
                    }
                }
                fro.status = (uObj.m_RespStatus == ResponseStatus.OK) ? FacebookResponseStatus.MERGEOK.ToString() : FacebookResponseStatus.ERROR.ToString();
                fro.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;

                fro.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;
                fro.data = HttpUtility.UrlEncode(Core.Social.Utils.GetEncryptPass(fro.siteGuid));
            }
            else
            {


                List<FBUser> lFriendsList;
                Int32 nNumOfFriends;

                bool bFriendsList = fbWrapper.GetFriendsList("me", accessToken, out nNumOfFriends, out lFriendsList);

                if (nNumOfFriends < fbc.nFBMinFriends)
                {
                    fro.status = FacebookResponseStatus.MINFRIENDS.ToString();
                    fro.data = nNumOfFriends.ToString();
                    fro.minFriends = fbc.nFBMinFriends.ToString();
                }
                else
                {

                    UserBasicData ubd = Core.Social.Utils.GetFBBasicData(fbUser, sEncryptToken, fro.pic);
                    UserDynamicData udd = new UserDynamicData();

                    List<UserDynamicDataContainer> luddc = FacebookWrapper.GetFBDynamicData(fbUser);
                    //udd.m_sUserData = Social.Utils.GetFBDynamicData(fbUser);

                    //NewsLetter
                    if (!string.IsNullOrEmpty(GetQueryValue("news")) && GetQueryValue("news").Equals("1"))
                    {
                        Core.Social.Utils.AddToDynamicData("NewsLetter", "true", ref luddc);
                    }

                    if (!string.IsNullOrEmpty(GetQueryValue("mail")))
                    {
                        string val = GetQueryValue("mail");
                        Core.Social.Utils.AddToDynamicData("mailtemplate", val, ref luddc);
                    }

                    udd.m_sUserData = luddc.ToArray();
                    uObj = Core.Social.Utils.AddNewUser(nGroupID, ubd, udd, GetPassword(), string.Empty);

                    if (uObj.m_RespStatus == ResponseStatus.OK || uObj.m_RespStatus == ResponseStatus.UserExists)
                    {
                        fro.status = FacebookResponseStatus.NEWUSER.ToString();

                        fro.siteGuid = uObj.m_user != null ? uObj.m_user.m_sSiteGUID : string.Empty;
                        fro.tvinciName = (uObj.m_user != null && uObj.m_user.m_oBasicData != null) ? uObj.m_user.m_oBasicData.m_sUserName : string.Empty;
                        fro.data = HttpUtility.UrlEncode(Core.Social.Utils.GetEncryptPass(fro.siteGuid));

                        string sSubID = GetQueryValue("subid");
                        if (!string.IsNullOrEmpty(sSubID))
                        {
                            string sCouponCode = !string.IsNullOrEmpty(GetQueryValue("coupon")) ? HttpUtility.UrlDecode(GetQueryValue("coupon")) : string.Empty;
                            string sUserIP = Request.UserHostAddress;

                            BillingResponse bObj = Core.Social.Utils.DummyChargeUserForSubscription(nGroupID, uObj.m_user.m_sSiteGUID, sSubID, sCouponCode, sUserIP);

                            if (bObj.m_oStatus != BillingResponseStatus.Success)
                            {
                                fro.status = FacebookResponseStatus.ERROR.ToString();
                                fbWrapper.RemoveUser(uObj.m_user.m_sSiteGUID);
                                return fro;
                            }
                        }

                        string domain = GetQueryValue("domain");
                        if (!string.IsNullOrEmpty(domain) && domain.Equals("1"))
                        {
                            DomainResponseObject dObj = Core.Social.Utils.AddNewDomain(nGroupID, uObj.m_user);
                            if (dObj.m_oDomainResponseStatus != DomainResponseStatus.OK)
                            {
                                fbWrapper.RemoveUser(uObj.m_user.m_sSiteGUID);
                                fro.status = FacebookResponseStatus.ERROR.ToString();
                                return fro;
                            }

                        }
                    }
                    else
                    {
                        fro.status = FacebookResponseStatus.ERROR.ToString();
                    }


                }
            }

            return fro;
        }

        private string GetPassword()
        {
            string[] chuncks = Guid.NewGuid().ToString().Split('-');
            return chuncks[0];
        }

        private string GetDisplay(string platform)
        {
            string display = "popup";
            switch (platform)
            {
                case "0":
                    {
                        display = "popup";
                        break;
                    }
                case "1":
                    {
                        //iPad
                        display = "popup";
                        break;
                    }
                case "3":
                    {
                        //android
                        display = "touch";
                        break;
                    }
                case "10":
                    {
                        display = "page";
                        break;
                    }
                default:
                    {
                        display = "popup";
                        break;
                    }
            }
            return display;
        }

        private string GetStatusToLog()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("groupID : {0}, ", Session["groupId"] == null ? "null" : Session["groupId"].ToString());
            sb.AppendFormat("siteGuid : {0}, ", Session["siteGuid"] == null ? "null" : Session["siteGuid"].ToString());
            sb.AppendFormat("callbackURL : {0}, ", Session["callbackURL"] == null ? "null" : Session["callbackURL"].ToString());
            sb.AppendFormat("action : {0}, ", Session["action"] == null ? "null" : Session["action"].ToString());
            sb.AppendFormat("platform : {0}, ", Session["platform"] == null ? "null" : Session["platform"].ToString());
            sb.AppendFormat("domain : {0}", Session["domain"] == null ? "null" : Session["domain"].ToString());

            return sb.ToString();
        }

        private string GetQueryValue(string key)
        {
            try
            {
                return ((NameValueCollection)Session["QueryString"])[key].ToString();
            }
            catch 
            {
                return string.Empty;
            }
        }

        protected string GetCallbackURL()
        {
            string cbURL = GetQueryValue("callbackURL");
            return HttpUtility.UrlDecode(cbURL);
        }
    }
}