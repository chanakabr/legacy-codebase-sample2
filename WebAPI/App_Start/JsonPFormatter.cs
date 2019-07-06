using Jil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using KLogMonitor;
using WebAPI.Managers.Scheme;
using System.Dynamic;
using Newtonsoft.Json;
using WebAPI.App_Start;
using WebAPI.Reflection;
using Newtonsoft.Json.Linq;
using WebAPI.Managers;
using WebAPI.Models.API;
using TVinciShared;

namespace WebAPI.Utils
{
    public class JsonPFormatter : JilFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public JsonPFormatter() : base(KalturaResponseType.JSONP)
        {
            MediaTypeMappings.Add(new QueryStringMapping("responseFormat", "jsonp", "application/json"));
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                string json = jsonManager.Serialize(value);

                string callback = HttpContext.Current.Request.GetQueryString()["callback"];
                if (string.IsNullOrEmpty(callback))
                {
                    callback = "callback";
                }

                string response = string.Format("{0}({1})", callback, json);
                streamWriter.Write(response);
                
                return Task.FromResult(writeStream);
            }
        }
    }
}