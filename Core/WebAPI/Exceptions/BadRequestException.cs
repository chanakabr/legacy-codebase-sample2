using WebAPI.Managers.Models;

namespace WebAPI.Exceptions
{
    public class BadRequestException : ApiException
    {
        public static ApiExceptionType BAD_REQUEST = new ApiExceptionType(StatusCode.BadRequest, "Bad request");

        public static ApiExceptionType INVALID_VERSION = new ApiExceptionType(StatusCode.InvalidVersion, "Invalid version [@version@]", "version");
        public static ApiExceptionType SERVICE_FORBIDDEN = new ApiExceptionType(StatusCode.ServiceForbidden, "Service Forbidden");
        public static ApiExceptionType PROPERTY_ACTION_FORBIDDEN = new ApiExceptionType(StatusCode.PropertyActionForbidden, StatusCode.ServiceForbidden, "Action [@action@] is forbidden for property [@type@].[@property@]", "action", "type", "property");
        public static ApiExceptionType ACTION_ARGUMENT_FORBIDDEN = new ApiExceptionType(StatusCode.ActionArgumentForbidden, StatusCode.ServiceForbidden, "Argument [@argument@] in action [@service@].[@action@] is forbidden", "argument", "service", "action");
        public static ApiExceptionType INVALID_KS_FORMAT = new ApiExceptionType(StatusCode.InvalidKS, "Invalid KS format");
        public static ApiExceptionType KS_EXPIRED = new ApiExceptionType(StatusCode.ExpiredKS, "KS expired");
        public static ApiExceptionType PARTNER_INVALID = new ApiExceptionType(StatusCode.PartnerInvalid, "Partner invalid");
        public static ApiExceptionType INVALID_REFRESH_TOKEN = new ApiExceptionType(StatusCode.InvalidRefreshToken, "Invalid refresh token");
        public static ApiExceptionType REFRESH_TOKEN_FAILED = new ApiExceptionType(StatusCode.RefreshTokenFailed, StatusCode.Error, "Refresh token failed");
        public static ApiExceptionType INVALID_USER_ID = new ApiExceptionType(StatusCode.UnauthorizedUser, StatusCode.Unauthorized, "Invalid user [@id@]", "id");
        public static ApiExceptionType INVALID_UDID = new ApiExceptionType(StatusCode.InvalidUdid, StatusCode.Unauthorized, "Invalid UDID [@id@]", "id");

        public static ApiExceptionType INVALID_SERVICE = new ApiExceptionType(StatusCode.InvalidService, "Service [@service@] not found", "service");
        public static ApiExceptionType INVALID_ACTION = new ApiExceptionType(StatusCode.InvalidAction, "Action [@service@.@action@] not found", "service", "action");
        public static ApiExceptionType ACTION_NOT_SPECIFIED = new ApiExceptionType(StatusCode.ActionNotSpecified, StatusCode.Error, "Action not specified");
        public static ApiExceptionType INVALID_ACTION_PARAMETER = new ApiExceptionType(StatusCode.InvalidActionParameter, StatusCode.InvalidActionParameters, "Invalid action parameter [@parameter@]", "parameter");
        public static ApiExceptionType INVALID_ACTION_PARAMETERS = new ApiExceptionType(StatusCode.InvalidActionParameters, "Invalid action parameters");
        public static ApiExceptionType REQUEST_ABORTED = new ApiExceptionType(StatusCode.RequestAborted, "This request aborted because there was an error in request number [@index@]", "index");
        public static ApiExceptionType REQUEST_SKIPPED = new ApiExceptionType(StatusCode.RequestSkipped, "This request skipped [@reason@]", "reason");

        public static ApiExceptionType INVALID_ARGUMENT = new ApiExceptionType(StatusCode.InvalidArgument, StatusCode.BadRequest, "Argument [@argument@] is invalid", "argument");
        public static ApiExceptionType ARGUMENT_MUST_BE_NUMERIC = new ApiExceptionType(StatusCode.ArgumentMustBeNumeric, StatusCode.BadRequest, "Argument [@argument@] must be numeric", "argument");
        public static ApiExceptionType ARGUMENT_CANNOT_BE_EMPTY = new ApiExceptionType(StatusCode.ArgumentCannotBeEmpty, StatusCode.BadRequest, "Argument [@argument@] cannot be empty", "argument");
        public static ApiExceptionType ARGUMENT_IS_READONLY = new ApiExceptionType(StatusCode.ArgumentReadonly, StatusCode.InvalidActionParameters, "Argument [@argument@] is not writeable", "argument");
        public static ApiExceptionType ARGUMENT_IS_INSERTONLY = new ApiExceptionType(StatusCode.ArgumentInsertonly, StatusCode.InvalidActionParameters, "Argument [@argument@] is not updateable", "argument");
        public static ApiExceptionType ARGUMENT_ENUM_VALUE_NOT_SUPPORTED = new ApiExceptionType(StatusCode.EnumValueNotSupported, StatusCode.BadRequest, "Enumerator value [@value@] is not supported for argument [@argument@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_STRING_SHOULD_BE_ENUM = new ApiExceptionType(StatusCode.ArgumentShouldBeEnum, StatusCode.InvalidActionParameters, "Argument [@argument@] values must be of type [@enum@]", "argument", "enum");
        public static ApiExceptionType ARGUMENT_STRING_CONTAINED_MIN_VALUE_CROSSED = new ApiExceptionType(StatusCode.ArgumentShouldContainMinValueCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] values must have minimum value of [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_STRING_CONTAINED_MAX_VALUE_CROSSED = new ApiExceptionType(StatusCode.ArgumentShouldContainMaxValueCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] values must have max value of [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENTS_CANNOT_BE_EMPTY = new ApiExceptionType(StatusCode.ArgumentsCannotBeEmpty, StatusCode.BadRequest, "One of the arguments [@arguments@] must have a value", "arguments");
        public static ApiExceptionType ARGUMENTS_CONFLICTS_EACH_OTHER = new ApiExceptionType(StatusCode.ArgumentsConflictsEachOther, StatusCode.BadRequest, "Only one of @argument1@ or @argument2@ can be used, not both of them", "argument1", "argument2");
        public static ApiExceptionType MULTIPLE_ARGUMENTS_CONFLICTS_EACH_OTHER = new ApiExceptionType(StatusCode.MultipleArgumentsConflictsEachOther, StatusCode.BadRequest, "Only one of the arguments [@arguments@] can be used, not all of them", "arguments");
        public static ApiExceptionType TIME_ARGUMENT_IN_PAST = new ApiExceptionType(StatusCode.TimeInPast, StatusCode.BadRequest, "Argument [@argument@] time have passed", "argument");
        public static ApiExceptionType ARGUMENT_MAX_LENGTH_CROSSED = new ApiExceptionType(StatusCode.ArgumentMaxLengthCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] maximum length is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MIN_LENGTH_CROSSED = new ApiExceptionType(StatusCode.ArgumentMinLengthCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] minimum length is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MAX_VALUE_CROSSED = new ApiExceptionType(StatusCode.ArgumentMaxValueCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] maximum value is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MIN_VALUE_CROSSED = new ApiExceptionType(StatusCode.ArgumentMinValueCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] minimum value is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_NOT_IN_PREDEFINED_RANGE = new ApiExceptionType(StatusCode.ArgumentNotInPredefinedRange, StatusCode.InvalidActionParameters, "Argument [@argument@] not in predefined range [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENTS_VALUES_CONFLICT_EACH_OTHER = new ApiExceptionType(StatusCode.ArgumentsValuesConflictEachOther, StatusCode.BadRequest, "Argument [@argument1@] value conflicts Argument [@argument2@] value", "argument1", "argument2");
        public static ApiExceptionType ARGUMENTS_VALUES_DUPLICATED = new ApiExceptionType(StatusCode.ArgumentsDuplicate, StatusCode.BadRequest, "Argument [@argument@] can not appear twice", "argument");
        public static ApiExceptionType INVALID_AGRUMENT_VALUE = new ApiExceptionType(StatusCode.InvalidArgumentValue, StatusCode.BadRequest, "Argument [@argument@] value must be of type [@value@]", "argument", "value");

        public static ApiExceptionType DUPLICATE_LANGUAGE_SENT = new ApiExceptionType(StatusCode.DuplicateLanguageSent, "languageCode: @lngCode@ has been sent more than once", "lngCode");
        public static ApiExceptionType INVALID_VALUE_FOR_FEATURE = new ApiExceptionType(StatusCode.InvalidValueForFeature, "Invalid value for feature: @feature@. feature can only contain alphanumeric values and/or underscore up until 64 characters", "feature");
        public static ApiExceptionType DEFUALT_LANGUAGE_MUST_BE_SENT = new ApiExceptionType(StatusCode.DefaultLanguageMustBeSent, "Default language must be one of the values sent for @object@", "object");
        public static ApiExceptionType GROUP_DOES_NOT_CONTAIN_LANGUAGE = new ApiExceptionType(StatusCode.GroupDoesNotContainLanguage, "language: @lng@ is not part of group supported languages", "lng");
        public static ApiExceptionType GLOBAL_LANGUAGE_MUST_BE_ASTERISK_FOR_WRITE_ACTIONS = new ApiExceptionType(StatusCode.GlobalLanguageParameterMustBeAsterisk, "Global language parameter must be asterisk for write actions of multilingualName");
        public static ApiExceptionType MULTI_VALUE_NOT_SENT_FOR_META_DATA_TYPE_STRING = new ApiExceptionType(StatusCode.MultiValueWasNotSentForMetaDataTypeString, "multipleValue property must have a value when KalturaMetaDataType equals STRING");
        public static ApiExceptionType TAG_TRANSLATION_NOT_ALLOWED = new ApiExceptionType(StatusCode.TagTranslationNotAllowed, "Tag translations are not allowed using asset controller, please use tag controller");

        public static ApiExceptionType BOTH_ARGUMENTS_MUST_HAVE_VALUE = new ApiExceptionType(StatusCode.OneOfArgumentsCannotBeEmpty, StatusCode.BadRequest, "Argument [@argument1@] cannot be empty if [@argument2@] not empty", "argument1", "argument2");
        public static ApiExceptionType TYPE_NOT_SUPPORTED = new ApiExceptionType(StatusCode.TypeNotSupported, StatusCode.BadRequest, "Type [@value@] is not supported for argument [@argument@]", "argument", "value");
        public static ApiExceptionType FORMAT_NOT_SUPPORTED = new ApiExceptionType(StatusCode.FormatNotSupported, StatusCode.BadRequest, "Format @formatName@(@format@) is not supported.", "formatName", "format");

        public static ApiExceptionType MEDIA_IDS_MUST_BE_NUMERIC = new ApiExceptionType(StatusCode.MediaIdsMustBeNumeric, StatusCode.BadRequest, "Media ids must be numeric");
        public static ApiExceptionType EPG_INTERNAL_IDS_MUST_BE_NUMERIC = new ApiExceptionType(StatusCode.EpgInternalIdsMustBeNumeric, StatusCode.BadRequest, "EPG internal ids must be numeric");
        public static ApiExceptionType LIST_TYPE_CANNOT_BE_EMPTY_OR_ALL = new ApiExceptionType(StatusCode.ListTypeCannotBeEmptyOrAll, StatusCode.BadRequest, "Argument [@argument@] cannot be empty or all", "argument");
        public static ApiExceptionType DUPLICATE_ASSET = new ApiExceptionType(StatusCode.DuplicateAsset, StatusCode.BadRequest, "Duplicate asset: id [@id@] type = [@type@]", "id", "type");
        public static ApiExceptionType DUPLICATE_FILE = new ApiExceptionType(StatusCode.DuplicateFile, StatusCode.BadRequest, "Duplicate file: id [@id@]", "id");
        public static ApiExceptionType UNABLE_TO_CREATE_HOUSEHOLD_FOR_USER_ROLE = new ApiExceptionType(StatusCode.UnableToCreateHouseholdForRole, StatusCode.BadRequest, "Unable to create household for role");
        public static ApiExceptionType EXTERNAL_ERROR = new ApiExceptionType(StatusCode.ExternalError, "externalCode: [@externalCode@], externalMessage: [@externalMessage@]", "externalCode", "externalMessage");
        public static ApiExceptionType HTTP_METHOD_NOT_SUPPORTED = new ApiExceptionType(StatusCode.HttpMethodNotSupported, StatusCode.BadRequest, "HTTP [@argument@] method not supported", "argument" );
        public static ApiExceptionType PROPERTY_IS_OPC_SUPPORTED = new ApiExceptionType(StatusCode.PropertyIsOpcSupported, StatusCode.InvalidActionParameters, "Property [@property@] is supported only for OPC accounts", "property");
        public static ApiExceptionType KEY_CANNOT_BE_EMPTY_OR_NULL = new ApiExceptionType(StatusCode.KeyCannotBeEmptyOrNull, StatusCode.BadRequest, "Key of [@property@] cannot be empty or null", "property");
        public static ApiExceptionType MISSING_MANDATORY_ARGUMENT_IN_PROPERTY = new ApiExceptionType(StatusCode.MissingMandatoryArgumentInProperty, StatusCode.BadRequest, "[@property@] must contain one argument from type [@type@]", "property", "type");
        public static ApiExceptionType ARGUMENT_MAX_ITEMS_CROSSED = new ApiExceptionType(StatusCode.ArgumentMaxItemsCrossed, StatusCode.BadRequest, "Argument [@argument@] maximum items is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MATCH_PATTERN_CROSSED = new ApiExceptionType(StatusCode.ArgumentMatchPatternCrossed, StatusCode.BadRequest, "Argument [@argument@] value must be match with in pattern [@pattern@]", "argument", "pattern");
        public static ApiExceptionType ARGUMENT_MIN_ITEMS_CROSSED = new ApiExceptionType(StatusCode.ArgumentMinItemsCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] minimum items is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MIN_PROPERTIES_CROSSED = new ApiExceptionType(StatusCode.ArgumentMinPropertiesCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] minimum properties is [@value@]", "argument", "value");
        public static ApiExceptionType ARGUMENT_MAX_PROPERTIES_CROSSED = new ApiExceptionType(StatusCode.ArgumentMaxPropertiesCrossed, StatusCode.InvalidActionParameters, "Argument [@argument@] maximum properties is [@value@]", "argument", "value");

        public BadRequestException()
            : this(BAD_REQUEST)
        {
        }

        public BadRequestException(ApiExceptionType type, params object[] parameters)
            : base(type, parameters)
        {
        }

        protected BadRequestException(ApiException ex)
            : base(ex)
        {
        }
    }
}
