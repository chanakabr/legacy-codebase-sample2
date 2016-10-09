using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebAPI.App_Start;

namespace WebAPI.Models.Renderers
{
    public abstract class KalturaRenderer
    {
        public KalturaRenderer(HttpConfiguration httpConfiguration, string contentType = null) 
        {
            new CustomResponseFormatter(httpConfiguration, contentType);
        }

        abstract public void Output(Stream writeStream, HttpContent content, TransportContext transportContext);
    }
}