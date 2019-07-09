using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using WebAPI.Models.General;
using TVinciShared;
using System.Collections;
using WebAPI.Managers.Models;
using WebAPI.Models.API;

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

        public Guid SessionId { get; set; }
        public string ClientTag { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public string UserIpAdress { get; set; }
        public KS Ks { get; set; }
        public RequestRouteData RouteData { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string Format { get; set; }
        public RequestType RequestType { get; set; }
        public bool AbortOnError { get; set; }
        public bool AbortAllOnError { get; set; }
        public bool SkipCondition { get; set; }
        public List<object> ActionParams { get; set; } = new List<object>();
        public bool IsMultiRequest => RouteData?.Service?.Equals("Multirequest", StringComparison.OrdinalIgnoreCase) == true;
        public IEnumerable<PhoenixRequestContext> MultiRequetContexts { get; set; } = new List<PhoenixRequestContext>();
        public KalturaOTTObject ResponseProfile { get; set; }
        public string RequestContentType { get; set; }
        public string RequestVersion { get; set; }
        
        /// <summary>
        /// This property is set by the SessionId middleware only to allow access to the elapes time from anywere in the
        /// Request proccessing pipeline.
        /// </summary>
        public KMonitor ApiMonitorLog { get; set; }

        /// <summary>
        /// This method sets all required HttpContext.Current.Items to suppport backward compatibility
        /// with the exsisting core code.
        /// If at the point of reading this HttpContext if fully replaced by the Phoenix.Context you can saflly remove this YAY!
        /// </summary>
        public void SetHttpContextForBackwardCompatibility()
        {
            var ctx = HttpContext.Current.Items;

            Ks = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_KS, Ks);
            GroupId = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_GROUP_ID, GroupId);
            UserId = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_USER_ID, UserId);
            UserIpAdress = GetOrSetFromHttpContext(ctx, ContextConstants.USER_IP, UserIpAdress);
            RouteData.Service = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_SERVICE, RouteData.Service);
            RouteData.Action = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_ACTION, RouteData.Action);
            RouteData.PathData = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_PATH_DATA, RouteData.PathData);
            Format = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_FORMAT, Format);
            Currency = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_CURRENCY, Currency);
            AbortOnError = GetOrSetFromHttpContext(ctx, ContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR, AbortOnError);
            ActionParams = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_METHOD_PARAMETERS, ActionParams);
            ResponseProfile = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_RESPONSE_PROFILE, ResponseProfile);
            RequestContentType = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_SERVE_CONTENT_TYPE, RequestContentType);
            RequestDate = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_TIME, RequestDate);
            RequestType = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_TYPE, RequestType);
            RequestVersion = GetOrSetFromHttpContext(ctx, ContextConstants.REQUEST_VERSION, RequestVersion);
        }

        #if NET461
        private T GetOrSetFromHttpContext<T>(IDictionary ctx, string key, T value)
        #endif
        #if NETSTANDARD2_0
        private T GetOrSetFromHttpContext<T>(IDictionary<object, object> ctx, string key, T value)
        #endif
        {
            if (!ctx.ContainsKey(key)) { ctx[key] = value; }
            return (T)ctx[key];
        }
    }
}
