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
        public const string RESPONSE_FORMAT = "responseFormat";

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

        private const string REQUEST_TAGS = "request_tags";
        private const string REQUEST_TAGS_PARTNER_ROLE = "partner_role";

        public static bool GetRequestContextValue<T>(string key, out T value)
        {
            value = default(T);
            bool res = false;
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items != null && System.Web.HttpContext.Current.Items.ContainsKey(key))
            {
                value = (T)System.Web.HttpContext.Current.Items[key];
                res = true;
            }
            return res;
        }

        public static long GetOriginalUserId()
        {
            GetRequestContextValue<long>(REQUEST_KS_ORIGINAL_USER_ID, out long originalUserId);

            return originalUserId;
        }

        public static void SetIsPartnerRequest()
        {
            if (System.Web.HttpContext.Current.Items.ContainsKey(REQUEST_TAGS))
            {
                var tags = (HashSet<string>)System.Web.HttpContext.Current.Items[REQUEST_TAGS];
                if (!tags.Contains(REQUEST_TAGS_PARTNER_ROLE))
                {
                    tags.Add(REQUEST_TAGS_PARTNER_ROLE);
                    System.Web.HttpContext.Current.Items[REQUEST_TAGS] = tags;
                }
            }
            else
            {
                System.Web.HttpContext.Current.Items.Add(REQUEST_TAGS, new HashSet<string>() { REQUEST_TAGS_PARTNER_ROLE });
            }
        }

        // TODO duplicate with LayeredCache.isPartnerRequest
        public static bool IsPartnerRequest()
        {
            var isPartner = GetRequestContextValue(REQUEST_TAGS, out HashSet<string> tags) 
                && tags != null && tags.Contains(REQUEST_TAGS_PARTNER_ROLE);
            return isPartner;
        }
    }
}
