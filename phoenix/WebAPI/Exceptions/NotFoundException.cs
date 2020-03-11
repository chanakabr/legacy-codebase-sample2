using WebAPI.Managers.Models;

namespace WebAPI.Exceptions
{
    public class NotFoundException : ApiException
    {
        public static ApiExceptionType OBJECT_NOT_FOUND = new ApiExceptionType(StatusCode.NotFound, "@objectType@ not found", "objectType");
        public static ApiExceptionType OBJECT_ID_NOT_FOUND = new ApiExceptionType(StatusCode.ObjectIdNotFound, StatusCode.NotFound, "@objectType@ id [@id@] not found", "objectType", "id");

        public NotFoundException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}
