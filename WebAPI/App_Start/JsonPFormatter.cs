using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using KLogMonitor;
using WebAPI.App_Start;
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

        public override Task<string> GetStringResponse(object response)
        {
            string json = jsonManager.Serialize(response);

            string callback = HttpContext.Current.Request.GetQueryString()["callback"];
            if (string.IsNullOrEmpty(callback))
            {
                callback = "callback";
            }

            string result = string.Format("{0}({1})", callback, json);
            return Task.FromResult(result);
        }

    }
}