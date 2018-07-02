using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;
using WebAPI.Models.General;
using System.Text;
using WebAPI.Managers.Models;
using WebAPI.Exceptions;
using System.Web.Http;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using WebAPI.Filters;

namespace WebAPI.App_Start
{
    public class CustomXmlFormatter : MediaTypeFormatter
    {
        public CustomXmlFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
            MediaTypeMappings.Add(new QueryStringMapping("format", "2", "application/xml"));
        }

        public override bool CanReadType(Type type)
        {
            if (type == (Type)null)
                throw new ArgumentNullException("type");

            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content,
            System.Net.TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                StatusWrapper wrapper = (StatusWrapper)value;
                Version currentVersion = (Version)HttpContext.Current.Items[RequestParser.REQUEST_VERSION];
                string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + wrapper.PropertiesToXml(currentVersion, true) + "</xml>";
                streamWriter.Write(xml);
                return Task.FromResult(writeStream);
            }
        }
    }
}