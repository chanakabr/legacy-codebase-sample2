using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Phoenix.Context
{
    public interface IPhoenixRequestContext
    {
        string SessionId { get; set; }
        DateTime RequestDate { get; set; }
        long GroupId { get; set; }
        long UserId { get; set; }
        string Ks { get; set; }
        string Service { get; set; }
        string Action { get; set; }
        string PathData { get; set; }
        string Language { get; set; }
        string Currency { get; set; }
        string Format { get; set; }
        RequestType RequestType { get; set; }
        bool AbortOnError { get; set; }
        bool AbortAllOnError { get; set; }
        bool SkipCondition { get; set; }
        bool IsMultiRequest { get; }
        JObject RequestBody { get; set; }
        IEnumerable<IPhoenixRequestContext> MultiRequetContexts { get; set; }

    }
}
