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
using WebAPI.Managers.Scheme;
using WebAPI.Utils;
using WebAPI.Models.API;

namespace WebAPI.App_Start
{
    public class CustomXmlFormatter : BaseFormatter
    {
        public CustomXmlFormatter() : base(KalturaResponseType.XML, new string[] { "application/xml", "text/xml" })
        {
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
                Version currentVersion = OldStandardAttribute.getCurrentRequestVersion();
                string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + wrapper.ToXml(currentVersion, true) + "</xml>";
                streamWriter.Write(xml);
                return Task.FromResult(writeStream);
            }
        }
    }
}