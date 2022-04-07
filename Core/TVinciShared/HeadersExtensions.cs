using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;

namespace TVinciShared
{
    public static class HeadersExtensions
    {
        public const string ServiceUrlHeaderName = "x-service-url";

        public static string TryGetHeaderValue(this NameValueCollection headers, string headerName)
        {
            return headers.Get(headerName);
        }
        
        public static string TryGetOriginalUriValue(this NameValueCollection headers)
        {
            return headers.TryGetHeaderValue(ServiceUrlHeaderName);
        }
        
        public static string TryGetHeaderValue(this IHeaderDictionary headers, string headerName)
        {
            return headers.TryGetValue(headerName, out var values) ? (string)values : null;
        }

        public static string TryGetOriginalUriValue(this IHeaderDictionary headers)
        {
            return headers.TryGetHeaderValue(ServiceUrlHeaderName);
        }
    }
}
