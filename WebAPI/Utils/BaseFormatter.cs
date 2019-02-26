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

namespace WebAPI.Utils
{
    public abstract class BaseFormatter : MediaTypeFormatter
    {
        public BaseFormatter(KalturaResponseType format, string contentType) : this(format, new string[] { contentType })
        {

        }

        public BaseFormatter(KalturaResponseType format, string[] contentTypes)
        {
            MediaTypeMappings.Add(new QueryStringMapping("format", "" + format.GetHashCode(), contentTypes.First()));
            foreach(string contentType in contentTypes)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
            }
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }
    }
}