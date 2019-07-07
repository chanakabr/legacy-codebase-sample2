using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using WebAPI.Models.General;

namespace Phoenix.Context
{
    public class RequestRouteData
    {
        public string Service { get; set; }
        public string Action { get; set; }
        public string PathData { get; set; }
    }


    public class PhoenixRequestContext
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string PHOENIX_REQUEST_CONTEXT_KEY = "PHOENIX_REQUEST_CONTEXT";

        public string SessionId { get; set; }
        public string ClientTag { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public string UserIpAdress { get; set; }
        public string Ks { get; set; }
        public RequestRouteData RouteData { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string Format { get; set; }
        public RequestType RequestType { get; set; }
        public bool AbortOnError { get; set; }
        public bool AbortAllOnError { get; set; }
        public bool SkipCondition { get; set; }
        public IDictionary<string, object> ActionParams { get; set; } = new Dictionary<string, object>();
        public bool IsMultiRequest => RouteData?.Service?.Equals("Multirequest", StringComparison.OrdinalIgnoreCase) == true;
        public IEnumerable<PhoenixRequestContext> MultiRequetContexts { get; set; } = new List<PhoenixRequestContext>();
        public KalturaOTTObject ResponseProfile { get; set; }
        public string RequestContentType { get; set; }
        public string RequestVersion { get; set; }

        /// <summary>
        /// This method sets all required HttpContext.Current.Items to suppport backward compatibility
        /// with the exsisting core code.
        /// If at the point of reading this HttpContext if fully replaced by the Phoenix.Context you can saflly remove this YAY!
        /// </summary>
        public void SetHttpContextForBackwardCompatibility()
        {
            HttpContext.Current.Items[ContextConstants.REQUEST_KS] = Ks;
            HttpContext.Current.Items[ContextConstants.REQUEST_GROUP_ID] = GroupId;
            HttpContext.Current.Items[ContextConstants.USER_IP] = UserId;
            HttpContext.Current.Items[ContextConstants.USER_IP] = UserIpAdress;
            HttpContext.Current.Items[ContextConstants.REQUEST_SERVICE] = RouteData.Service;
            HttpContext.Current.Items[ContextConstants.REQUEST_ACTION] = RouteData.Action;
            HttpContext.Current.Items[ContextConstants.REQUEST_PATH_DATA] = RouteData.PathData;
            HttpContext.Current.Items[ContextConstants.REQUEST_FORMAT] = Format;
            HttpContext.Current.Items[ContextConstants.REQUEST_CURRENCY] = Currency;
            HttpContext.Current.Items[ContextConstants.REQUEST_LANGUAGE] = Language;
            HttpContext.Current.Items[ContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR] = this.AbortOnError;
            HttpContext.Current.Items[ContextConstants.REQUEST_METHOD_PARAMETERS] = ActionParams;
            HttpContext.Current.Items[ContextConstants.REQUEST_RESPONSE_PROFILE] = ResponseProfile;
            HttpContext.Current.Items[ContextConstants.REQUEST_SERVE_CONTENT_TYPE] = RequestContentType;
            HttpContext.Current.Items[ContextConstants.REQUEST_TIME] = RequestDate;
            HttpContext.Current.Items[ContextConstants.REQUEST_TYPE] = RequestType;
            HttpContext.Current.Items[ContextConstants.REQUEST_VERSION] = RequestVersion;
        }
    }
}
