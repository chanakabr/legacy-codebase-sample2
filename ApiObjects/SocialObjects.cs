using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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

    public class FacebookConfig
    {
        public string sFBKey;
        public string sFBSecret;
        public string sFBCallback;
        public int nFBMinFriends;
        public string sFBPermissions;
        public string sFBRedirect;

        public FacebookConfig()
        {
            sFBKey = string.Empty;
            sFBSecret = string.Empty;
            sFBCallback = string.Empty;
            sFBPermissions = string.Empty;
            sFBRedirect = string.Empty;
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

    public class FriendWatchedObject
    {
        public int SiteGuid { get; set; }
        public int MediaID { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
