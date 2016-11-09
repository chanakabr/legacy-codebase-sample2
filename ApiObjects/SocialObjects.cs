using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using ApiObjects.Response;
using ApiObjects.Social;

namespace ApiObjects
{
    public static class SocialObjects
    {
        public static IEnumerable<T> GetAllSelectedItems<T, U>(this Enum value)
        {

            int valueAsInt = Convert.ToInt32(value, CultureInfo.InvariantCulture);

            foreach (object item in Enum.GetValues(typeof(U)))
            {
                int itemAsInt = Convert.ToInt32(item, CultureInfo.InvariantCulture);

                if (itemAsInt == (valueAsInt & itemAsInt))
                {
                    yield return (T)item;
                }
            }
        }
    }

    public enum eSocialPrivacy
    {
        UNKNOWN = 0,
        EVERYONE = 2,
        ALL_FRIENDS = 4,
        FRIENDS_OF_FRIENDS = 8,
        SELF = 16,
        CUSTOM = 32
    }

    public enum SocialPlatform
    {
        UNKNOWN = 0,
        FACEBOOK = 1,
        GOOGLE = 2
    }

    [Flags]
    public enum eUserAction
    {
        UNKNOWN = 1,
        LIKE = 2,
        UNLIKE = 4,
        SHARE = 8,
        POST = 16,
        WATCHES = 32,
        WANTS_TO_WATCH = 64,
        RATES = 128,
        FOLLOWS = 256,
        UNFOLLOW = 512
    }

    public enum SocialActionShare
    {
        Internal = 0,
        External = 1
    }

    public enum SocialAction
    {
        UNKNOWN = 0,
        LIKE = 1,
        UNLIKE = 2,
        SHARE = 3,
        POST = 4
    }

    public enum eSocialActionPrivacy
    {
        UNKNOWN = 0,
        ALLOW = 1,
        DONT_ALLOW = 2
    }

    [Flags]
    public enum eAssetType
    {
        UNKNOWN = 1,
        MEDIA = 2,
        PROGRAM = 4
    }

    [Serializable]
    public class KeyValuePair
    {
        public string key { get; set; }
        public string value { get; set; }

        public KeyValuePair()
        {
            key = string.Empty;
            value = string.Empty;
        }

        public KeyValuePair(string k, string v)
        {
            key = k;
            value = v;
        }

        public static KeyValuePair GetKVPFromList(string sKey, List<KeyValuePair> lKVP)
        {
            KeyValuePair oRes = null;

            if (lKVP != null)
            {
                foreach (var kvp in lKVP)
                {
                    if (kvp != null && kvp.key.Equals(sKey))
                    {
                        oRes = kvp;
                        break;
                    }
                }
            }

            return oRes;
        }
    }

    public class TwitterConfig
    {
        public string sConsumerKey;
        public string sConsumerSecret;
        public string sTwittCallback;
        public int nTwittMinFriends;
        public string sTwittPermissions;
        public string sTwittRedirect;

        public TwitterConfig()
        {
            sConsumerKey = string.Empty;
            sConsumerSecret = string.Empty;
            sTwittCallback = string.Empty;
            sTwittPermissions = string.Empty;
            sTwittRedirect = string.Empty;
            nTwittMinFriends = 0;
        }
    }

    [Serializable]
    public class FBLoaction
    {
        public string name { get; set; }
        public string id { get; set; }
    }
    [Serializable]
    public class FBInterestData
    {
        public string name { get; set; }
        public string category { get; set; }
        public string id { get; set; }
        public string created_time { get; set; }
    }

    public class FBInterest
    {
        public List<FBInterestData> data { get; set; }
    }

    [Serializable]
    public class FBUser
    {
        string m_fbid;
        string m_name;
        string m_first_name;
        string m_last_name;
        string m_email;
        string m_gender;

        public string m_sSiteGuid { get; set; }

        public string Birthday { get; set; }

        public FBLoaction Location { get; set; }

        [DataMember]
        public FBInterest interests { get; set; }

        [DataMember]
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        [DataMember]
        public string id
        {
            get
            {
                return m_fbid;
            }
            set
            {
                m_fbid = value;
            }
        }

        [DataMember]
        public string uid
        {
            get
            {
                return m_fbid;
            }
            set
            {
                m_fbid = value;
            }
        }

        [DataMember]
        public string first_name
        {
            get
            {
                return m_first_name;
            }
            set
            {
                m_first_name = value;
            }
        }

        [DataMember]
        public string last_name
        {
            get
            {
                return m_last_name;
            }
            set
            {
                m_last_name = value;
            }
        }

        [DataMember]
        public string email
        {
            get
            {
                return m_email;
            }
            set
            {
                m_email = value;
            }
        }

        [DataMember]
        public string gender
        {
            get
            {
                return m_gender;
            }
            set
            {
                m_gender = value;
            }
        }
    }

    public class FacebookConfig : PlatformConfig
    {
        public string sFBKey;
        public string sFBSecret;
        public string sFBCallback;
        public int nFBMinFriends;
        public string sFBPermissions;
        public string sFBRedirect;
        
        [XmlIgnore] [ScriptIgnore]
        public string sFBToken;

        public FacebookConfig()
        {
            sFBKey = string.Empty;
            sFBSecret = string.Empty;
            sFBCallback = string.Empty;
            sFBPermissions = string.Empty;
            sFBRedirect = string.Empty;
            sFBToken = string.Empty;
            nFBMinFriends = 0;
        }

        public string AppSecret
        {
            get
            {
                return string.Format("{0}|{1}", sFBKey, sFBSecret);
            }
        }
    }

    public class FacebookConfigResponse
    {
        public FacebookConfig FacebookConfig { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public FacebookConfigResponse()
        {
            Status = new Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
        }
    }

    public class FriendWatchedObject
    {
        public int SiteGuid { get; set; }
        public int MediaID { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    [Serializable]
    [JsonObject(Id = "social_activity_doc")]
    public class SocialActivityDoc
    {
        public SocialActivityDoc()
        {
            DocType = "user_action";
            ActivityObject = new SocialActivityObject();
            ActivitySubject = new SocialActivitySubject();
            ActivityVerb = new SocialActivityVerb();
        }

        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("owner_site_guid", NullValueHandling = NullValueHandling.Ignore)]
        public string DocOwnerSiteGuid { get; set; }
        [JsonProperty("social_platform", NullValueHandling = NullValueHandling.Ignore)]
        public int SocialPlatform { get; set; }
        [JsonProperty("doc_type", NullValueHandling = NullValueHandling.Ignore)]
        public string DocType { get; set; }
        [JsonProperty("create_date", NullValueHandling = NullValueHandling.Ignore)]
        public long CreateDate { get; set; }
        [JsonProperty("last_update", NullValueHandling = NullValueHandling.Ignore)]
        public long LastUpdate { get; set; }
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
        [JsonProperty("permit_sharing")]
        public bool PermitSharing{ get; set; }

        [JsonProperty("object", NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivityObject ActivityObject { get; set; }
        [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivitySubject ActivitySubject { get; set; }
        [JsonProperty("verb", NullValueHandling = NullValueHandling.Ignore)]
        public SocialActivityVerb ActivityVerb { get; set; }
    }
    [Serializable]
    [JsonObject(Id = "social_activity_subject")]
    public class SocialActivitySubject
    {
        [JsonProperty("actor_site_guid", NullValueHandling = NullValueHandling.Ignore)]
        public string ActorSiteGuid { get; set; }
        [JsonProperty("pic_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ActorPicUrl { get; set; }
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string ActorTvinciUsername { get; set; }
        [JsonProperty("group_id", NullValueHandling = NullValueHandling.Ignore)]
        public int GroupID { get; set; }
        [JsonProperty("device_udid", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceUdid { get; set; }
    }
    [Serializable]
    [JsonObject(Id = "social_activity_object")]
    public class SocialActivityObject
    {
        [JsonProperty("asset_id", NullValueHandling = NullValueHandling.Ignore)]
        public int AssetID { get; set; }
        [JsonProperty("object_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectID { get; set; }
        [JsonProperty("asset_type", NullValueHandling = NullValueHandling.Ignore)]
        public eAssetType AssetType { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }
        [JsonProperty("pic_url", NullValueHandling = NullValueHandling.Ignore)]
        public string PicUrl { get; set; }
    }
    [Serializable]
    [JsonObject(Id = "social_activity_verb")]
    public class SocialActivityVerb
    {
        public SocialActivityVerb()
        {
            this.ActionProperties = new List<ActionProperties>();
        }

        [JsonProperty("social_action_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SocialActionID { get; set; }
        [JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
        public int ActionType { get; set; }
        [JsonProperty("action_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ActionName { get; set; }
        [JsonProperty("rate_value", NullValueHandling = NullValueHandling.Ignore)]
        public int RateValue { get; set; }
        [JsonProperty("action_props", NullValueHandling = NullValueHandling.Ignore)]
        public List<ActionProperties> ActionProperties { get; set; }

    }

    [Serializable]
    public class ActionProperties
    {
        [JsonProperty("prop_name", NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyName { get; set; }
        [JsonProperty("prop_value", NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyValue { get; set; }
    }

    public class SocialActivityResponse
    {
        public Status Status { get; set; }

        public List<SocialActivityDoc> SocialActivity { get; set; }

        public int TotalCount { get; set; }
    }
}
