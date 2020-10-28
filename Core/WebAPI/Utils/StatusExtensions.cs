using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.Utils
{
    public static class StatusExtensions
    {
        public static void ThrowOnError(this ApiObjects.Response.Status status)
        {
            if (status == null) throw new ClientException(StatusCode.Error);
            if (!status.IsOkStatusCode()) throw new ClientException(status);
        }
    }
}
