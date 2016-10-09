using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace WebAPI.Models.Renderers
{
    public class KalturaStringRenderer : KalturaRenderer
    {
        private String _sContent;

        public KalturaStringRenderer(HttpConfiguration httpConfiguration, string content, string contentType = null)
            : base(httpConfiguration, contentType)
        {
            _sContent = content;
        }

        public override void Output(Stream writeStream, HttpContent content, TransportContext transportContext) 
        {
            var buf = Encoding.UTF8.GetBytes(_sContent);
            writeStream.Write(buf, 0, buf.Length);
        }
    }
}