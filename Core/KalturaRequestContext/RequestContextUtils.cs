using System.Collections.Generic;

namespace KalturaRequestContext
{
    public class RequestContextUtils : IRequestContextUtils
    {
        private const string REQUEST_REGION = "region_id";
        private const string SCK = "sck"; // session characteristic key

        public string GetRequestId() => GetValueOrDefault<object>(RequestContextConstants.SESSION_ID_KEY)?.ToString()
                                        ?? GetValueOrDefault<object>(RequestContextConstants.REQUEST_ID_KEY)?.ToString();

        public long? GetPartnerId()
        {
            if (GetRequestContextValue(RequestContextConstants.REQUEST_GROUP_ID, out object partnerIdObject)
                && long.TryParse(partnerIdObject.ToString(), out var partnerId))
            {
                return partnerId;
            }

            return null;
        }

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

        public bool IsImpersonateRequest() => GetValueOrDefault(RequestContextConstants.REQUEST_IMPERSONATE, false);

        public void SetKsPayload(int regionId, string sessionCharacteristicKey)
        {
            SetValue(REQUEST_REGION, regionId);
            SetValue(SCK, sessionCharacteristicKey);
        }
        public void RemoveKsPayload()
        {
            RemoveValue(REQUEST_REGION);
            RemoveValue(SCK);
        }
        public int? GetRegionId() => GetValueOrDefault<int?>(REQUEST_REGION, null);
        public string GetSessionCharacteristicKey() => GetValueOrDefault<string>(SCK, null);

        private T GetValueOrDefault<T>(string key, T defaultValue = default)
        {
            return GetRequestContextValue<T>(key, out var value)
                ? value
                : defaultValue;
        }

        private bool GetRequestContextValue<T>(string key, out T value)
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

        private void SetValue<T>(string key, T value)
        {
            System.Web.HttpContext.Current.Items[key] = value;
        }
        
        private void RemoveValue(string key)
        {
            System.Web.HttpContext.Current.Items.Remove(key);
        }

        public bool TryGetRecordingConvertId(out long programId)
        {
            return GetRequestContextValue(RequestContextConstants.RECORDING_CONVERT_KEY, out programId);
        }
    }
}
