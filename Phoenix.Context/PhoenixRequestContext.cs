using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KLogMonitor;
using Newtonsoft.Json.Linq;

namespace Phoenix.Context
{
    public class PhoenixRequestContext
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string PHOENIX_REQUEST_CONTEXT_KEY = "PHOENIX_REQUEST_CONTEXT";

        public string SessionId { get; set; }
        public string ClientTag { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public string Ks { get; set; }
        public string Service { get; set; }
        public string Action { get; set; }
        public string PathData { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string Format { get; set; }
        public RequestType RequestType { get; set; }
        public bool AbortOnError { get; set; }
        public bool AbortAllOnError { get; set; }
        public bool SkipCondition { get; set; }
        public IDictionary<string, object> ActionParams { get; set; } = new Dictionary<string, object>();
        public object FormFiles { get; set; }
        public bool IsMultiRequest => Service?.Equals("Multirequest", StringComparison.OrdinalIgnoreCase) == true;
        public IEnumerable<PhoenixRequestContext> MultiRequetContexts { get; set; } = new List<PhoenixRequestContext>();

    }
}
