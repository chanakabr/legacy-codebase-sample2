using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebAPI.Managers.Models;
using WebAPI.Models.Renderers;
using System.Net.Http;
using System.Net;
using System.Web;

namespace WebAPI.App_Start
{
    public class CustomResponseFormatter : JilFormatter
    {
        class ServeActionMapping : MediaTypeMapping
        {
            public ServeActionMapping() : base("application/json")
            {
            }

            public override double TryMatchMediaType(HttpRequestMessage request)
            {
                if (HttpContext.Current.Items[RequestContext.REQUEST_SERVE_CONTENT_TYPE] != null)
                    return 1;

                return 0;
            }
        }

        public CustomResponseFormatter()
        {
            MediaTypeMappings.Add(new ServeActionMapping());
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
            string contentType = (string)HttpContext.Current.Items[RequestContext.REQUEST_SERVE_CONTENT_TYPE];
            headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == typeof(StatusWrapper) && ((StatusWrapper)value).Result != null && ((StatusWrapper)value).Result is WebAPI.App_Start.KalturaAPIExceptionWrapper)
                return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);


            var renderer = (KalturaRenderer)((StatusWrapper)value).Result;

            renderer.Output(writeStream);

            return Task.FromResult(writeStream);

        }
    }
}