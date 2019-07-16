using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models.API;

namespace WebAPI.App_Start
{
    public abstract class BaseFormatter : MediaTypeFormatter
    {
        public BaseFormatter(KalturaResponseType format, string contentType) : this(format, new string[] { contentType })
        {

        }

        public BaseFormatter(KalturaResponseType format, string[] contentTypes)
        {
            MediaTypeMappings.Add(new QueryStringMapping("format", "" + format.GetHashCode(), contentTypes.First()));
            foreach (string contentType in contentTypes)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
            }
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
            Format = format;
            AcceptContentTypes = contentTypes;
        }

        public sealed override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            var responseString = await GetStringResponse(value);
            using (var streamWriter = new StreamWriter(writeStream))
            {
                streamWriter.Write(responseString);
            }
        }

        public KalturaResponseType Format { get; }
        public string[] AcceptContentTypes { get; }

        public abstract Task<string> GetStringResponse(object response);
    }
}