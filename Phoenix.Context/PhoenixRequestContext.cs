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

        public Guid? SessionId { get; set; }
        public string ClientTag { get; set; }
        public DateTime? RequestDate { get; set; } = DateTime.UtcNow;
        public int? GroupId { get; set; }
        public int? UserId { get; set; }
        public string UserIpAdress { get; set; }
        public KS Ks { get; set; }
        public RequestRouteData RouteData { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string Format { get; set; }
        public RequestType? RequestType { get; set; }
        public bool? AbortOnError { get; set; }
        public bool? AbortAllOnError { get; set; }
        public bool? SkipCondition { get; set; }
        public List<object> ActionParams { get; set; } = new List<object>();
        public bool IsMultiRequest => RouteData?.Service?.Equals("Multirequest", StringComparison.OrdinalIgnoreCase) == true;
        public IEnumerable<PhoenixRequestContext> MultiRequetContexts { get; set; } = new List<PhoenixRequestContext>();
        public KalturaOTTObject ResponseProfile { get; set; }
        public string RequestContentType { get; set; }
        public Version RequestVersion { get; set; }

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
            Ks = GetOrSetFromHttpContext(ContextConstants.REQUEST_KS, Ks);
            GroupId = GetOrSetStuctFromHttpContext(ContextConstants.REQUEST_GROUP_ID, GroupId);
            UserId = GetOrSetStuctFromHttpContext(ContextConstants.REQUEST_USER_ID, UserId);
            UserIpAdress = GetOrSetFromHttpContext(ContextConstants.USER_IP, UserIpAdress);
            RouteData.Service = GetOrSetFromHttpContext(ContextConstants.REQUEST_SERVICE, RouteData.Service);
            RouteData.Action = GetOrSetFromHttpContext(ContextConstants.REQUEST_ACTION, RouteData.Action);
            RouteData.PathData = GetOrSetFromHttpContext(ContextConstants.REQUEST_PATH_DATA, RouteData.PathData);
            Format = GetOrSetFromHttpContext(ContextConstants.REQUEST_FORMAT, Format);
            Currency = GetOrSetFromHttpContext(ContextConstants.REQUEST_CURRENCY, Currency);
            AbortOnError = GetOrSetStuctFromHttpContext(ContextConstants.MULTI_REQUEST_GLOBAL_ABORT_ON_ERROR, AbortOnError);
            ActionParams = GetOrSetFromHttpContext(ContextConstants.REQUEST_METHOD_PARAMETERS, ActionParams);
            ResponseProfile = GetOrSetFromHttpContext(ContextConstants.REQUEST_RESPONSE_PROFILE, ResponseProfile);
            RequestContentType = GetOrSetFromHttpContext(ContextConstants.REQUEST_SERVE_CONTENT_TYPE, RequestContentType);
            RequestDate = GetOrSetStuctFromHttpContext(ContextConstants.REQUEST_TIME, RequestDate);
            RequestType = GetOrSetStuctFromHttpContext(ContextConstants.REQUEST_TYPE, RequestType);
            RequestVersion = GetOrSetFromHttpContext(ContextConstants.REQUEST_VERSION, RequestVersion);
        }


        // These methods are here to facilitate the set and get from http context
        // If httpContext has the time we will take the vale into this object otherwise 
        // we check to see if the value in the object was populated and create the relevant key in http context


        private T GetOrSetFromHttpContext<T>(string key, T value) where T : class
        {
            if (!HttpContext.Current.Items.ContainsKey(key) && value != null)
            {
                HttpContext.Current.Items[key] = value;
                return (T)HttpContext.Current.Items[key];
            }
            else
            {
                return value;
            }

        }

        private string GetOrSetStringFromHttpContext(string key, string value)
        {
            if (!HttpContext.Current.Items.ContainsKey(key) && value != null)
            {
                HttpContext.Current.Items[key] = value;
                return (string)HttpContext.Current.Items[key];
            }
            else
            {
                return value;
            }

        }

        private Nullable<U> GetOrSetStuctFromHttpContext<U>(string key, Nullable<U> value) where U : struct
        {
            if (!HttpContext.Current.Items.ContainsKey(key) && value.HasValue)
            {
                HttpContext.Current.Items[key] = value;
                return (U)HttpContext.Current.Items[key];
            }
            else
            {
                return value;
            }

        }
    }
}
