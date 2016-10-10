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
using WebAPI.Models.Renderers;
using System.Net.Http;
using System.Net;
using System.Web.Http;

namespace WebAPI.App_Start
{
    public class CustomResponseFormatter : MediaTypeFormatter
    {
        private string _sContentType;

        public CustomResponseFormatter(HttpConfiguration httpConfiguration, string contentType)
        {
            _sContentType = contentType;

            httpConfiguration.Formatters.Clear();
            httpConfiguration.Formatters.Add(this);
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

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue(_sContentType);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                if (type == typeof(StatusWrapper) && ((StatusWrapper)value).Result != null && ((StatusWrapper)value).Result is KalturaRenderer)
                {
                    KalturaRenderer renderer = (KalturaRenderer)((StatusWrapper)value).Result;
                    renderer.Output(writeStream, content, transportContext);
                }
            });
        }
    }
}