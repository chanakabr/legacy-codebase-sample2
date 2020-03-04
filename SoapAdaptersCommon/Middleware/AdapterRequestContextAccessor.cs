using KLogMonitor;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoapAdaptersCommon.Middleware
{
    public interface IAdapterRequestContextAccessor
    {
        AdapterRequestContext Current { get; }
    }

    /// <summary>
    /// This class is a utility class to access the request context sent from phoenix to the adapter
    /// It will use the HttpContext to retrive the typed info such as:
    ///  - the original request id that was generated at the calling phoenix request
    ///  - the KS tht was used to invoke the original phoenix request that called the adapter
    ///  
    /// NOTE: this calss depends on the class AdapterRequestContextExtractor to inspect the SOAP message
    /// and extract all relevant information into the HttpContext
    /// </summary>
    public class AdapterRequestContextAccessor : IAdapterRequestContextAccessor
    {
        private readonly IHttpContextAccessor _HttpContextAccessor;

        public AdapterRequestContext Current { get => new AdapterRequestContext(_HttpContextAccessor); }
        public AdapterRequestContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _HttpContextAccessor = httpContextAccessor;
        }
    }


    public class AdapterRequestContext
    {
        private readonly IHttpContextAccessor _HttpContextAccessor;

        public string RequestId { get => _HttpContextAccessor.HttpContext.Items[Constants.REQUEST_ID_KEY]?.ToString(); }
        public string Ks { get => _HttpContextAccessor.HttpContext.Items[Constants.KS]?.ToString(); }

        public AdapterRequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _HttpContextAccessor = httpContextAccessor;
        }
    }
}
