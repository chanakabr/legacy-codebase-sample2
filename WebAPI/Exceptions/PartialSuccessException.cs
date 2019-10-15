namespace WebAPI.Exceptions
{
    public class PartialSuccessException : ApiException
    {
        public PartialSuccessException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}