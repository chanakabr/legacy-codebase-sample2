using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.Social
{
    public enum eSocialCommon
    {
        DO_ACTION = 0,
        GET_ACTION = 1,
        OBJECT_CALL = 2,
        PRIVACY_CALL = 3
    }
    
    public enum eSocialPlatform
    {
        Unknown = 0,
        InApp = 1,
        Facebook = 2,
        Twitter = 3
    }

    public enum eRequestType
    {
        GET = 0,
        SET = 1,
        DELETE = 2
    }

    public enum FacebookResponseStatus
    {
        UNKNOWN = 0,
        OK = 1,
        ERROR = 2,
        NOACTION = 3,
        NOTEXIST = 4,
        CONFLICT = 5,
        MERGE = 6,
        MERGEOK = 7,
        NEWUSER = 8,
        MINFRIENDS = 9,
        INVITEOK = 10,
        INVITEERROR = 11,
        ACCESSDENIED = 12,
        WRONGPASSWORDORUSERNAME = 13,
        UNMERGEOK = 14
    }

    public enum SocialActionResponseStatus
    {
        UNKNOWN = 0,
        OK = 1,
        ERROR = 2,
        UNKNOWN_ACTION = 3,
        INVALID_ACCESS_TOKEN = 4,
        INVALID_PLATFORM_REQUEST = 5,
        MEDIA_DOESNT_EXISTS = 6,
        MEDIA_ALREADY_LIKED = 7,
        INVALID_PARAMETERS = 8,
        USER_DOES_NOT_EXIST = 9,
        NO_FB_ACTION = 10,
        EMPTY_FB_OBJECT_ID = 11,
        MEDIA_ALEADY_FOLLOWED = 12,
        CONFIG_ERROR = 13,
        MEDIA_ALREADY_RATED = 14,
        NOT_ALLOWED = 15
    }

    public enum FB_MediaType
    {
        OTHER = 0,
        MOVIE = 1,
        TV_SHOW = 2,
        EPISODE = 3
    }

    [Serializable]
    public class FBActionResponse
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public ErrorDesc error { get; set; }
    }

    public class ErrorDesc
    {
        public string message { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string subcode { get; set; }
    }
    public class FBError
    {
        public FBError() { }
        public ErrorDesc error { get; set; }
    }

    [Serializable]
    public class FBPermissions
    {
        IEnumerable<FBPermission> m_data;
        [DataMember]
        public IEnumerable<FBPermission> data { get; set; }
    }

    [Serializable]
    public class FBPermission
    {
        [DataMember]
        public string publish_stream { get; set; }

        [DataMember]
        public string offline_access { get; set; }

        [DataMember]
        public string share_item { get; set; }

        [DataMember]
        public string installed { get; set; }

    }

}