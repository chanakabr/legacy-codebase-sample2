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

namespace WebAPI.App_Start
{
    public class CustomXmlFormatter : MediaTypeFormatter
    {
        public CustomXmlFormatter()
        {
            SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("application/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
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

        [XmlRoot("xml")]
        public class XmlReponseWrapper
        {
            [XmlElement("result")]
            public object Result { get; set; }
        }

        public override Task WriteToStreamAsync(Type type, object value,
            Stream writeStream, System.Net.Http.HttpContent content,
            System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                StatusWrapper wrapper = (StatusWrapper)value;
                XmlReponseWrapper xrw = new XmlReponseWrapper() { Result = wrapper.Result };
                XmlSerializer xs = new XmlSerializer(typeof(XmlReponseWrapper), new Type[] { wrapper.Result.GetType() });

                using (System.IO.StringWriter sw = new System.IO.StringWriter())
                using (XmlWriter writer = XmlWriter.Create(sw))
                {
                    xs.Serialize(writeStream, xrw);
                }
            });
        }
    }
}