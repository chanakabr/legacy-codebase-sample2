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

        abstract public class XmlReponseWrapper
        {
            [XmlElement("executionTime")]
            public float ExecutionTime { get; set; }
        }

        [XmlRoot("xml")]
        public class XmlSingleReponseWrapper : XmlReponseWrapper
        {
            [XmlElement("result", IsNullable = true)]
            public object Result { get; set; }
        }

        [XmlRoot("xml")]
        public class XmlMultiReponseWrapper : XmlReponseWrapper
        {
            [XmlArrayItem("item")]
            public object[] result { get; set; }
        }
        
        private XmlDocument SerializeToXmlDocument(XmlReponseWrapper input, StatusWrapper wrapper)
        {
            List<Type> extraTypes = new List<Type>();
            if(wrapper.Result != null)
            {
                if (wrapper.Result is Array)
                {
                    foreach (object result in (object[])wrapper.Result)
                    {
                        extraTypes.Add(result.GetType());
                    }
                }
                else
                {
                    extraTypes.Add(wrapper.Result.GetType());
                }
            }

            XmlSerializer ser = new XmlSerializer(input.GetType(), extraTypes.ToArray());

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
                    xd.FirstChild.InnerText = "version=\"1.0\" encoding=\"utf-8\"";
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
                XmlReponseWrapper xrw;

                if (wrapper.Result is Array)
                {
                    xrw = new XmlMultiReponseWrapper()
                    {
                        result = (object[])wrapper.Result
                    };
                }
                else if (wrapper.Result is IList)
                {
                    IList result = (IList)wrapper.Result;
                    object[] array = new object[result.Count];
                    result.CopyTo(array, 0);
                    xrw = new XmlMultiReponseWrapper()
                    {
                        result = array
                    };
                }
                else
                {
                    xrw = new XmlSingleReponseWrapper()
                    {
                        Result = wrapper.Result
                    };
                }

                xrw.ExecutionTime = wrapper.ExecutionTime;
                
                XmlDocument doc = SerializeToXmlDocument(xrw, wrapper);
                var resnode = doc.GetElementsByTagName("result")[0];

                //if (wrapper.Result != null)
                //{
                //    var otype = doc.CreateElement("objectType");
                //    otype.InnerText = wrapper.Result != null ? wrapper.Result.GetType().Name : null;                    
                //    resnode.PrependChild(otype);
                //}

                // Removing unnecessary attributes such as NS, and type
                resnode.Attributes.RemoveAll();
                doc.GetElementsByTagName("xml")[0].Attributes.RemoveAll();

                var buf = Encoding.UTF8.GetBytes(doc.OuterXml);
                writeStream.Write(buf, 0, buf.Length);
            });
        }
    }
}