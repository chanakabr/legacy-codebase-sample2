using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FeatureFlag;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;

namespace WebAPI.App_Start
{
    public class CustomXmlFormatter : BaseFormatter
    {
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;
        
        public CustomXmlFormatter() : base(KalturaResponseType.XML, new string[] { "application/xml", "text/xml" })
        {
            _phoenixFeatureFlag = PhoenixFeatureFlagInstance.Get();
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
            var wrapper = (StatusWrapper)value;
            var currentVersion = OldStandardAttribute.getCurrentRequestVersion();

            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                if (_phoenixFeatureFlag.IsEfficientSerializationUsed())
                {
                    var stringBuilder = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>");
                    wrapper.AppendAsXml(stringBuilder, currentVersion, true);
                    stringBuilder.Append("</xml>");

#if NETCOREAPP3_1
                    return streamWriter.WriteAsync(stringBuilder);
#endif
#if NET48
                    return streamWriter.WriteAsync(stringBuilder.ToString());
#endif
                }
                else
                {
                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + wrapper.ToXml(currentVersion, true) + "</xml>";

                    return streamWriter.WriteAsync(xml);
                }
            }
        }

        /// <summary>
        /// This method is used for the .net core version of phoenix and will serialize the object async
        /// </summary>
        public override Task<string> GetStringResponse(object obj)
        {
            return Task.Run(() =>
            {
                var wrapper = (StatusWrapper)obj;
                var currentVersion = OldStandardAttribute.getCurrentRequestVersion();
                string xml;

                if (_phoenixFeatureFlag.IsEfficientSerializationUsed())
                {
                    var stringBuilder = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>");
                    wrapper.AppendAsXml(stringBuilder, currentVersion, true);
                    stringBuilder.Append("</xml>");

                    xml = stringBuilder.ToString();
                }
                else
                {
                    xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xml>" + wrapper.ToXml(currentVersion, true) + "</xml>";
                }

                return Task.FromResult(xml);
            });
        }
    }
}