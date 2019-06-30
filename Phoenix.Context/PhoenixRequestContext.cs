using System;
using System.Collections.Generic;
using System.Reflection;
using KLogMonitor;
using Newtonsoft.Json.Linq;

namespace Phoenix.Context
{
    public class PhoenixRequestContext : IPhoenixRequestContext
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string SessionId { get; set; }
        public DateTime RequestDate { get; set; }
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
        public JObject RequestBody { get; set; }
        public bool IsMultiRequest => Service?.Equals("Multirequest", StringComparison.OrdinalIgnoreCase) == true;
        public IEnumerable<IPhoenixRequestContext> MultiRequetContexts { get; set; }

        public PhoenixRequestContext()
        {
        }

    }
}
