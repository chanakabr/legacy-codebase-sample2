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

namespace WebAPI.Utils
{
    public class JilFormatter : MediaTypeFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly Options _jilOptions;

        protected JsonManager jsonManager;

        public JilFormatter()
        {
            _jilOptions = new Options(dateFormat: DateTimeFormat.SecondsSinceUnixEpoch, excludeNulls: true, includeInherited: true);
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            MediaTypeMappings.Add(new QueryStringMapping("format", "1", "application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            jsonManager = JsonManager.GetInstance();
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }
        
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                string json = jsonManager.Serialize(value);
                streamWriter.Write(json);
                return Task.FromResult(writeStream);
            }
        }
    }
}