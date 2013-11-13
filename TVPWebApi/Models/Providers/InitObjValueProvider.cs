using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using TVPApi;

namespace TVPWebApi.Models
{
    public class InitObjValueProvider : IValueProvider
    {
        private const string _headerPrefix = "X-";

        private readonly HttpRequestHeaders _headers;

        public InitObjValueProvider(HttpActionContext actionContext)
        {
            _headers = actionContext.ControllerContext.Request.Headers;

            if (!_headers.Contains("X-InitObj"))
            {
                throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("X-InitObj is missing.")
                });
            }
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
                InitializationObject obj = null;

                try
                {
                    byte[] bytes = Convert.FromBase64String(values.First());

                    using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
                    {
                        ms.Write(bytes, 0, bytes.Length);

                        ms.Position = 0;

                        obj = (InitializationObject)new BinaryFormatter().Deserialize(ms);
                    }
                }
                catch (Exception)
                {
                    throw new HttpResponseException(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent("Token invalid.")
                    });
                }
                
                return new ValueProviderResult(obj, values.First(), CultureInfo.InvariantCulture);
            }
            
            return null; 
        }
    }
}