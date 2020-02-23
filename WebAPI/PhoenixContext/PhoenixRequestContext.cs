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
using WebAPI;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using Newtonsoft.Json;

namespace Phoenix.Context
{
    public class RequestRouteData
    {
        public string Service { get; set; }
        public string Action { get; set; }
        public string PathData { get; set; }

        public override string ToString()
        {
            return $"{Service}.{Action}";
        }
    }


    public class PhoenixRequestContext
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string PHOENIX_REQUEST_CONTEXT_KEY = "PHOENIX_REQUEST_CONTEXT";

        public string SessionId { get; set; }
        public string ClientTag { get; set; }
        public DateTime? RequestDate { get; set; } = DateTime.UtcNow;
        public int? GroupId { get; set; }
        public int? UserId { get; set; }
        
        // Cant log out ip address
        [JsonIgnore]
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
        
        // Might have PII cant log it
        [JsonIgnore]
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
        [JsonIgnore]
        public KMonitor ApiMonitorLog { get; set; }
        public string RawRequestUrl { get; set; }
        public JObject RawRequestBody { get; set; }
        public object Response { get; set; }
    }
}
