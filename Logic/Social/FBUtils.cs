using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using ApiObjects;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social
{
    public static class FBUtils
    {
        internal static readonly int STATUS_OK = 200;
        internal static readonly string FB_GRAPH_URI_PREFIX = Utils.GetValFromConfig("FB_GRAPH_URI");
        internal static readonly string FB_GRAPH_URI_ME_PREFIX = string.Format("{0}/me", Utils.GetValFromConfig("FB_GRAPH_URI"));

        public static SocialActionResponseStatus DeleteUserActionOnObject(string sUserAccessToken, string sFBActionID)
        {
            int nStatus = 0;
            SocialActionResponseStatus eRes = SocialActionResponseStatus.UNKNOWN;
            string sRetVal;
            string sParams = string.Empty;

            sRetVal = Utils.SendDeleteHttpReq(string.Format("{0}/{1}?access_token={2}", FB_GRAPH_URI_PREFIX, sFBActionID, sUserAccessToken), ref nStatus, string.Empty, string.Empty, sParams);

            if (!string.IsNullOrEmpty(sRetVal) && sRetVal == "true")
            {
                eRes = SocialActionResponseStatus.OK;
            }
            else
            {
                eRes = SocialActionResponseStatus.ERROR;
            }
            return eRes;
        }      

        static public string CreateFBObject(FBMediaObject oFBMedia, string sAppAccessToken)
        {
            string sRes = string.Empty;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string serializedMediaObj = serializer.Serialize(oFBMedia);

            int nStatus = 0;

            if (!string.IsNullOrEmpty(serializedMediaObj))
            {
                string sParameters = string.Format("access_token={0}&object={1}", sAppAccessToken, serializedMediaObj);
                sRes = Utils.SendPostHttpReq(string.Format("{0}/app/objects/{1}?", FB_GRAPH_URI_PREFIX, oFBMedia.type), ref nStatus, string.Empty, string.Empty, sParameters);
            }

            return sRes;
        }

        static public int DeleteObject(string sAppAccessToken, string sObjectID, ref string sRetVal)
        {
            string sUrl = string.Format("{0}/{1}?access_token={2}", FB_GRAPH_URI_PREFIX, sObjectID, sAppAccessToken);
            int nStatus = -1;

            sRetVal = Utils.SendDeleteHttpReq(sUrl, ref nStatus, string.Empty, string.Empty, string.Empty);
            return nStatus;
        }

        public static string GetFBObjectType(string sAccessToken, string sObjectID)
        {
            string sRes = string.Empty;
            string sRetVal = string.Empty;
            int nStatus = 0;
            nStatus = FBUtils.GetGraphApiAction(sObjectID, string.Empty, sAccessToken, ref sRetVal);
            if (nStatus == STATUS_OK)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBObjectReponse response = serializer.Deserialize<FBObjectReponse>(sRetVal);
                if (response != null && response.type != null)
                {
                    string[] lSplit = response.type.Split('.');
                    sRes = (lSplit != null && lSplit.Length > 1) ? lSplit[1] : lSplit[0];
                    if (sRes == "other")
                    {
                        sRes = "video";
                    }
                }
            }

            return sRes;
        }

        static public Int32 GetGraphApiAction(string sID, string sConnectionType, string sFBToken, ref string sRetVal)
        {
            Int32 nStatus = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}/",FB_GRAPH_URI_PREFIX);
            sb.Append(sID);
            if (!string.IsNullOrEmpty(sConnectionType))
            {
                sb.Append("/" + sConnectionType);
            }
            sb.AppendFormat("{0}access_token={1}", sb.ToString().IndexOf("?") != -1 ? "&" : "?", sFBToken);

            string sUrl = sb.ToString();

            sRetVal = Utils.SendGetHttpReq(sUrl, ref nStatus, string.Empty, string.Empty);

            return nStatus;
        }

        static public bool CanUserShare(string sFacebookID, string sAccessToken)
        {
            bool bRes = false;
            try
            {
                string sRetVal = string.Empty;

                Int32 nStatus = GetGraphApiAction(sFacebookID, "permissions", sAccessToken, ref sRetVal);
                if (nStatus == STATUS_OK)
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    FBPermissions permissions = serializer.Deserialize<FBPermissions>(sRetVal);
                    if (permissions != null)
                    {
                        List<FBPermission> permissionsList = permissions.data.ToList<FBPermission>();

                        foreach (FBPermission fbp in permissionsList)
                        {
                            if (!string.IsNullOrEmpty(fbp.publish_stream))
                            {
                                bRes = true;
                                break;
                            }
                        }
                    }
                }

            }
            catch 
            {
                bRes = false;
            }

            return bRes;
        }

        static public SocialActionResponseStatus GetFacebookError(string sRetVal)
        {
            SocialActionResponseStatus eRes = SocialActionResponseStatus.ERROR;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FBError error = serializer.Deserialize<FBError>(sRetVal);
            if (error != null)
            {
                switch (error.error.code)
                {
                    case "3501":
                        eRes = SocialActionResponseStatus.MEDIA_ALREADY_LIKED;
                        break;
                    case "190":
                        eRes = SocialActionResponseStatus.INVALID_ACCESS_TOKEN;
                        break;
                    default:
                        eRes = SocialActionResponseStatus.ERROR;
                        break;
                }
            }

            return eRes;
        }
    }

    [Serializable()]
    public class FacebookException : System.Exception
    {
        public FacebookResponseStatus status;

        public FacebookException()
            : base()
        {
            status = FacebookResponseStatus.ERROR;
        }
        public FacebookException(FacebookResponseStatus exStatus)
            : base()
        {
            status = exStatus;
        }
        public FacebookException(FacebookResponseStatus exStatus, string message)
            : base(message)
        {
            status = exStatus;
        }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected FacebookException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    public class FacebookResponseObject
    {
        public FacebookResponseObject()
        {
            status = string.Empty;
            siteGuid = string.Empty;
            tvinciName = string.Empty;
            facebookName = string.Empty;
            pic = string.Empty;
            data = string.Empty;
            minFriends = string.Empty;
            fbUser = new FBUser();
            token = string.Empty;
        }

        public string status;
        public string siteGuid;
        public string tvinciName;
        public string facebookName;
        public string pic;
        public string data;
        public string minFriends;
        public FBUser fbUser;
        public string token;
    }

    [Serializable]
    public class FBPrivacySettingContainer
    {
        [DataMember]
        public IEnumerable<FBPrivacySetting> data { get; set; }
    }

    [Serializable]
    public class FBPrivacySetting
    {
        public FBPrivacySetting()
        {
        }

        //Friend list of users allowed to watch
        [DataMember]
        public string allow { get; set; }

        //Friend list of users not allowed to watch
        [DataMember]
        public string deny { get; set; }

        //Name of privacy settings (e.g. default_stream_privacy)
        [DataMember]
        public string name { get; set; }

        //Privacy group type (e.g. ALL_FRIENDS, CUSTOM, SELF etc.)
        [DataMember]
        public string value { get; set; }

        public eSocialPrivacy ToEnum()
        {
            eSocialPrivacy eRes = eSocialPrivacy.UNKNOWN;
            try
            {
                eRes = (eSocialPrivacy)Enum.Parse(typeof(eSocialPrivacy), value, true);
            }
            catch { }

            return eRes;
        }
    }

    [Serializable]
    public class FBResponse
    {
        string m_site_guid;
        string m_status;

        [DataMember]
        public string site_guid
        {
            get
            {
                return m_site_guid;
            }
            set
            {
                m_site_guid = value;
            }
        }

        [DataMember]
        public string status
        {
            get
            {
                return m_status;
            }
            set
            {
                m_status = value;
            }
        }
    }

    [Serializable]
    public class FBMediaObject
    {
        public FBMediaObject() { }

        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string image { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public FBObjectData data { get; set; }
    }

    [Serializable]
    public class FBObjectData
    {
        [DataMember]
        public string[] tag { get; set; }
        [DataMember]
        public string release_date { get; set; }
    }

    [Serializable]
    public class FBObjectReponse
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string type { get; set; }
    }

    [Serializable]
    public class FBFriendsContainer
    {
        //IEnumerable<FBFriend> m_data;
        [DataMember]
        public IEnumerable<FBUser> data { get; set; }
    }

    [Serializable]
    public class FBFriendsListContainer
    {
        [DataMember]
        public IEnumerable<FBFriendsList> data { get; set; }
    }

    [Serializable]
    public class FBFriendsList
    {
        public FBFriendsList()
        {
        }

        public FBFriendsList(string id, string name)
        {
            this.flid = id;
            this.name = name;
        }

        [DataMember]
        public string flid { get; set; }

        [DataMember]
        public string name { get; set; }
    }

    public class FacebookTokenResponse
    {
        public FacebookTokenResponse()
        {
            status = FacebookResponseStatus.ERROR.ToString();
            message = string.Empty;
        }

        public string status;
        public string message;
    }

    [Serializable]
    public class FBTokenRes
    {
        [DataMember]
        public FBTokenData data { get; set; }
    }

    public class FBTokenData
    {
        public bool is_valid { get; set; }
        public string app_id { get; set; }
        public string application { get; set; }
        public string expires_at { get; set; }
    }

    public class FBSignin
    {
        public FBSignin()
        {
            status = new ApiObjects.Response.Status();
            user = new UserResponseObject();
        }

        public ApiObjects.Response.Status status;
        public UserResponseObject user;
    }
}
