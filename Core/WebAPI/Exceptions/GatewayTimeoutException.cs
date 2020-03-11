using WebAPI.Exceptions;

namespace WebAPI.Utils
{
    public class GatewayTimeoutException : ApiException
    {
        public GatewayTimeoutException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}
