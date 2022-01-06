using Grpc.Core;
using Grpc.Core.Interceptors;
using Phx.Lib.Log;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SoapAdaptersCommon.GrpcAdapters
{
    public class AdapterRequestInterceptor : Interceptor
    {
        public const string HTTP_CONTEXT_ITEM_ADAPTER_ID = "adapterId";
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IHttpContextAccessor _HttpContextAccessor;
        public AdapterRequestInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _Logger.Debug("Initializing AdapterRequestInterceptor...");
            _HttpContextAccessor = httpContextAccessor;
        }


        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                var adapterIdProp = request.GetType().GetProperties().FirstOrDefault(p=>p.Name.Equals("AdapterId", StringComparison.OrdinalIgnoreCase));
                if (adapterIdProp != null)
                {
                    var adapterIdVal = adapterIdProp.GetValue(request);
                    _HttpContextAccessor.HttpContext.Items[HTTP_CONTEXT_ITEM_ADAPTER_ID] = adapterIdVal;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while trying to extract Adapter Id from GRPC request", e);
            }
            return continuation(request, context);
        }

    }
}