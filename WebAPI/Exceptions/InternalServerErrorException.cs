using WebAPI.Managers.Models;

namespace WebAPI.Exceptions
{
    public class InternalServerErrorException : ApiException
    {
        public static ApiExceptionType INTERNAL_SERVER_ERROR = new ApiExceptionType(StatusCode.Error, "error");

        public static ApiExceptionType MISSING_CONFIGURATION = new ApiExceptionType(StatusCode.MissingConfiguration, "Missing configuration [@configuration@]", "configuration");

        public InternalServerErrorException()
            : this(INTERNAL_SERVER_ERROR)
        {
        }

        public InternalServerErrorException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}
