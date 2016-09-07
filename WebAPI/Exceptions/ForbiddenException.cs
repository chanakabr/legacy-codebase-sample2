using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Exceptions
{
    public class ForbiddenException : ApiException
    {
        public static ApiExceptionType HOUSEHOLD_FORBIDDEN = new ApiExceptionType(StatusCode.HouseholdForbidden, StatusCode.ServiceForbidden, "Household [@household@] forbidden", "household");
        public static ApiExceptionType SWITCH_USER_NOT_ALLOWED_FOR_PARTNER = new ApiExceptionType(StatusCode.SwitchingUsersIsNotAllowedForPartner, "Switching users is not allowed for partner");
        public static ApiExceptionType NOT_ACTIVE_APP_TOKEN = new ApiExceptionType(StatusCode.NotActiveAppToken, "Application-token [@id@] is not active", "id");
        public static ApiExceptionType INVALID_APP_TOKEN_HASH = new ApiExceptionType(StatusCode.InvalidAppTokenHash, "Invalid application-token hash");
        public static ApiExceptionType APP_TOKEN_EXPIRED = new ApiExceptionType(StatusCode.ExpiredAppToken, "Application-token is expired");

        public ForbiddenException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }
    }
}