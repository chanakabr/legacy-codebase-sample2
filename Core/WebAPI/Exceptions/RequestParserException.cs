using WebAPI.Exceptions;
using WebAPI.Managers.Models;


// TODO: Arthur, move to a different namespace (Exceptions, need to update reflector as well because of this)
namespace WebAPI.Filters
{
    public class RequestParserException : BadRequestException
    {
        public static ApiExceptionType INVALID_MULTIREQUEST_TOKEN = new ApiExceptionType(StatusCode.InvalidMultirequestToken, "Invalid multirequest token");

        public static ApiExceptionType INVALID_OBJECT_TYPE = new ApiExceptionType(StatusCode.InvalidObjectType, "Invalid object type [@type@]", "type");
        public static ApiExceptionType ABSTRACT_PARAMETER = new ApiExceptionType(StatusCode.AbstractParameter, "Abstract parameter type [@type@]", "type");
        public static ApiExceptionType MISSING_PARAMETER = new ApiExceptionType(StatusCode.MissingParameter, StatusCode.InvalidActionParameters, "Missing parameter [@parameter@]", "parameter");
        public static ApiExceptionType INDEX_NOT_ZERO_BASED = new ApiExceptionType(StatusCode.MultirequestIndexNotZeroBased, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, response index is not zero based");
        public static ApiExceptionType INVALID_INDEX = new ApiExceptionType(StatusCode.MultirequestInvalidIndex, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, invalid response index");
        public static ApiExceptionType GENERIC_METHOD = new ApiExceptionType(StatusCode.MultirequestGenericMethod, StatusCode.InvalidService, "Invalid multirequest service, invalid service: [@service@], action: [@action@]", "service", "action");
        public static ApiExceptionType INVALID_OPERATOR = new ApiExceptionType(StatusCode.MultirequestInvalidOperatorForConditionType, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, invalid operator [@operator@] for condition type [@conditionType@]", "operator", "conditionType");
        public static ApiExceptionType INVALID_CONDITION_VALUE = new ApiExceptionType(StatusCode.MultirequestInvalidConditionValue, StatusCode.InvalidMultirequestToken, "Invalid multirequest token, invalid condition value [@conditionValue@] for condition type [@conditionType@]", "conditionValue", "conditionType");

        public RequestParserException()
            : this(INVALID_MULTIREQUEST_TOKEN)
        {
        }

        public RequestParserException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }

        public RequestParserException(ApiException ex) : base(ex)
        {
        }
    }
}