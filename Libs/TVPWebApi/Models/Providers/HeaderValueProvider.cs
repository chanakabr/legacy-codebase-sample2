using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using TVPApi;

namespace TVPWebApi.Models
{
    public class HeaderValueProvider<T> : IValueProvider where T : class
    {
        private const string _headerPrefix = "X-";

        private readonly HttpRequestHeaders _headers;

        public HeaderValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            _headers = actionContext.ControllerContext.Request.Headers;
        }

        public bool ContainsPrefix(string prefix)
        {
            return _headers.Any(h => h.Key.Contains(_headerPrefix + prefix));
        }

        public ValueProviderResult GetValue(string key)
        {
            IEnumerable<string> values;

            if (_headers.TryGetValues(_headerPrefix + key, out values))
            {
                T obj = JsonConvert.DeserializeObject<T>(values.First(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                return new ValueProviderResult(obj, values.First(), CultureInfo.InvariantCulture);
            }
            
            return null; 
        }
    }
}