using System;
using System.Collections.Generic;
using System.Text;

namespace TVinciShared
{
    public class RequestContextUtils
    {
        public const string REQUEST_METHOD_PARAMETERS = "requestMethodParameters";
        public const string REQUEST_VERSION = "requestVersion";
        public const string REQUEST_USER_ID = "user_id";
        public const string REQUEST_GROUP_ID = "group_id";
        public const string REQUEST_KS = "KS";
        public const string REQUEST_LANGUAGE = "language";
        public const string REQUEST_CURRENCY = "currency";
        public const string REQUEST_FORMAT = "format";
        public const string USER_IP = "USER_IP";

        // same key as in REST solution KLogMonitor.Constants
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

        public static bool GetRequestContextValue<T>(string key, out T value)
        {
            value = default(T);
            bool res = false;
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items != null && System.Web.HttpContext.Current.Items.ContainsKey(key))
            {
                //long.TryParse((string)System.Web.HttpContext.Current.Items[key], out originalUserId);
                value = (T)System.Web.HttpContext.Current.Items[key];
                res = true;
            }
            return res;
        }
    }
}