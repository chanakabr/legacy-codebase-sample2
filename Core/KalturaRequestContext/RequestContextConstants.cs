namespace KalturaRequestContext
{
    public static class RequestContextConstants
    {
        public const string SESSION_ID_KEY = "x-kaltura-session-id";
        public const string REQUEST_ID_KEY = "kmon_req_id";
        public const string REQUEST_METHOD_PARAMETERS = "requestMethodParameters";
        public const string REQUEST_VERSION = "requestVersion";
        public const string REQUEST_CLIENT_TAG = "requestClientTag";
        public const string REQUEST_USER_ID = "user_id";
        public const string REQUEST_GROUP_ID = "group_id";
        public const string REQUEST_KS = "KS";
        public const string REQUEST_LANGUAGE = "language";
        public const string REQUEST_CURRENCY = "currency";
        public const string REQUEST_UDID = "request_udid";
        public const string REQUEST_FORMAT = "format";
        public const string USER_IP = "USER_IP";
        public const string RESPONSE_FORMAT = "responseFormat";

        // same key as in REST solution Phx.Lib.Log.Constants
        // in-case changing this  - you must change there  as well
        public const string REQUEST_GLOBAL_KS = "global_ks";

        public const string REQUEST_GLOBAL_USER_ID = "global_user_id";
        public const string REQUEST_GLOBAL_LANGUAGE = "global_language";
        public const string REQUEST_GLOBAL_CURRENCY = "global_currency";
        public const string REQUEST_SERVICE = "requestService";
        public const string REQUEST_ACTION = "requestAction";
        public const string REQUEST_TIME = "requestTime";
        public const string REQUEST_TYPE = "requestType";
        public const string REQUEST_SERVE_CONTENT_TYPE = "requestServeContentType";
        public const string REQUEST_PATH_DATA = "pathData";
        public const string REQUEST_RESPONSE_PROFILE = "responseProfile";
        public const string REQUEST_KS_ORIGINAL_USER_ID = "ks_original_user_id";

        public const string MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR = "global_abort_on_error";

        public const string REQUEST_TAGS = "request_tags";
        public const string REQUEST_TAGS_PARTNER_ROLE = "partner_role";

        public const string REQUEST_IMPERSONATE = "impersonate";

        public const string RECORDING_CONVERT_KEY = "GetPlaybackContextAssetConvert";
    }
}