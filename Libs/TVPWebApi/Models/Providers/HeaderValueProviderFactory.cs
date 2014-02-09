using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace TVPWebApi.Models
{
    public class HeaderValueProviderFactory<T> : ValueProviderFactory where T : class
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new HeaderValueProvider<T>(actionContext);
        }
    }
}