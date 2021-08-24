using System.Collections.Generic;

namespace KalturaRequestContext
{
    public class RequestContextUtils : IRequestContextUtils
    {
        public string GetRequestId() => GetValueOrDefault<object>(RequestContextConstants.REQUEST_ID_KEY)?.ToString();

        public long? GetUserId()
        {
            if (GetRequestContextValue(RequestContextConstants.REQUEST_USER_ID, out object userIdObject))
            {
                return long.TryParse(userIdObject.ToString(), out var userId)
                    ? userId
                    : (long?)null;
            }

            return null;
        }

        public long GetOriginalUserId()
        {
            GetRequestContextValue(RequestContextConstants.REQUEST_KS_ORIGINAL_USER_ID, out long originalUserId);

            return originalUserId;
        }

        public string GetUdid() => GetValueOrDefault<object>(RequestContextConstants.REQUEST_UDID)?.ToString();

        public string GetUserIp() => GetValueOrDefault<string>(RequestContextConstants.USER_IP);

        public void SetIsPartnerRequest()
        {
            if (System.Web.HttpContext.Current.Items.ContainsKey(RequestContextConstants.REQUEST_TAGS))
            {
                var tags = (HashSet<string>)System.Web.HttpContext.Current.Items[RequestContextConstants.REQUEST_TAGS];
                if (!tags.Contains(RequestContextConstants.REQUEST_TAGS_PARTNER_ROLE))
                {
                    tags.Add(RequestContextConstants.REQUEST_TAGS_PARTNER_ROLE);
                    System.Web.HttpContext.Current.Items[RequestContextConstants.REQUEST_TAGS] = tags;
                }
            }
            else
            {
                System.Web.HttpContext.Current.Items.Add(RequestContextConstants.REQUEST_TAGS, new HashSet<string> { RequestContextConstants.REQUEST_TAGS_PARTNER_ROLE });
            }
        }

        // TODO duplicate with LayeredCache.isPartnerRequest
        public bool IsPartnerRequest()
        {
            var isPartner = GetRequestContextValue(RequestContextConstants.REQUEST_TAGS, out HashSet<string> tags)
                            && tags != null
                            && tags.Contains(RequestContextConstants.REQUEST_TAGS_PARTNER_ROLE);

            return isPartner;
        }

        private T GetValueOrDefault<T>(string key, T defaultValue = default)
        {
            return GetRequestContextValue<T>(key, out var value)
                ? value
                : defaultValue;
        }

        public bool GetRequestContextValue<T>(string key, out T value)
        {
            value = default;
            var res = false;
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Items.ContainsKey(key))
            {
                value = (T)System.Web.HttpContext.Current.Items[key];
                res = true;
            }

            return res;
        }

        public bool IsImpersonateRequest()
        {
            GetRequestContextValue(RequestContextConstants.REQUEST_IMPERSONATE, out bool isImpersonateRequest);

            return isImpersonateRequest;
        }
    }
}
