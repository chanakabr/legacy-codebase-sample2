namespace WebAPI.Exceptions
{
    public class UnauthorizedException : BadRequestException
    {
        public UnauthorizedException(ApiExceptionType type, params string[] parameters)
            : base(type, parameters)
        {
        }
    }
}