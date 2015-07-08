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

        private XmlDocument SerializeToXmlDocument(XmlReponseWrapper input, StatusWrapper wrapper)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType(), new Type[] { wrapper.Result.GetType() });

            XmlDocument xd = null;

            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, input);

                memStm.Position = 0;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            return xd;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content,
            System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                StatusWrapper wrapper = (StatusWrapper)value;
                XmlReponseWrapper xrw = new XmlReponseWrapper() { Result = wrapper.Result };
                //XmlSerializer xs = new XmlSerializer(typeof(XmlReponseWrapper), new Type[] { wrapper.Result.GetType() });

                XmlDocument doc = SerializeToXmlDocument(xrw, wrapper);
                var otype = doc.CreateElement("objectType");
                otype.InnerText = wrapper.Result.GetType().Name;
                doc.GetElementsByTagName("result")[0].PrependChild(otype);
                //using (System.IO.StringWriter sw = new System.IO.StringWriter())
                //using (XmlWriter writer = XmlWriter.Create(sw))
                {
                    var buf = Encoding.UTF8.GetBytes(doc.OuterXml);
                    writeStream.Write(buf, 0, buf.Length);
                    //xs.Serialize(writeStream, xrw);
                }
            });
        }
    }
}