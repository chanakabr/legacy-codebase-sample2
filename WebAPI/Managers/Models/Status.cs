using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Reflection;

namespace WebAPI.Managers.Models
{
    [DataContract]
    public class StatusWrapper : KalturaSerializable
    {
        public StatusWrapper()
        {

        }

        public StatusWrapper(int code, Guid reqID, float executionTime, object result = null, string msg = null)
        {
            ExecutionTime = executionTime;
            Result = result;
        }

        protected override Dictionary<string, string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            Dictionary<string, string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("executionTime", "\"executionTime\": " + ExecutionTime);
            if (Result != null)
            {
                if (Result is IKalturaSerializable)
                {
                    propertyValue = (Result as IKalturaSerializable).ToJson(currentVersion, omitObsolete);
                }
                else
                {
                    JsonManager jsonManager = JsonManager.GetInstance();
                    if (Result.GetType().IsArray)
                    {
                        propertyValue = "[" + String.Join(", ", (Result as IEnumerable<object>).Select(item => jsonManager.Serialize(item))) + "]";
                    }
                    else
                    {
                        propertyValue = jsonManager.Serialize(Result);
                    }
                };
                ret.Add("result", "\"result\": " + propertyValue);
            }
            return ret;
        }

        protected override Dictionary<string, string> PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            Dictionary<string, string> ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("executionTime", "<executionTime>" + ExecutionTime + "</executionTime>");
            if (Result != null)
            {
                if(Result is IKalturaSerializable)
                {
                    propertyValue = (Result as IKalturaSerializable).ToXml(currentVersion, omitObsolete);
                }
                else
                {
                    JsonManager jsonManager = JsonManager.GetInstance();
                    if (Result.GetType().IsArray)
                    {
                        propertyValue = "<item>" + String.Join("</item><item>", (Result as IEnumerable<object>).Select(item => (item is IKalturaSerializable) ? (item as IKalturaSerializable).ToXml(currentVersion, omitObsolete) : item.ToString())) + "</item>";
                    }
                    else
                    {
                        propertyValue = Result.ToString();
                    }
                }
                
                ret.Add("result", "<result>" + propertyValue + "</result>");
            }
            return ret;
        }

        [DataMember(Name = "result", Order = 0)]
        public object Result { get; set; }

        [DataMember(Name = "executionTime", Order = 1)]
        public float ExecutionTime { get; set; }
    }

    public enum StatusCode
    {
        OK = 0,
        Error = 1,

        // 500000 - 599999 - TVPAPI Statuses
        NotImplemented = 500000,
        InternalConnectionIssue = 500001,
        Timeout = 500002,
        BadRequest = 500003,
        ServiceForbidden = 500004,
        Unauthorized = 500005,
        MissingConfiguration = 500006,
        NotFound = 500007,
        PartnerInvalid = 500008,
        UserIDInvalid = 500009,
        HouseholdInvalid = 500010,
        InvalidService = 500011,
        InvalidAction = 500012,
        InvalidActionParameters = 500013,
        InvalidJSONRequest = 500014,
        InvalidKS = 500015,
        ExpiredKS = 500016,
        InvalidRefreshToken = 500017,
        AbstractParameter = 500018,
        InvalidPaging = 500019,
        //InvalidAppToken = 50020,  // currently not in use
        ExpiredAppToken = 50021,
        InvalidAppTokenHash = 50022,
        NotActiveAppToken = 50023,
        SwitchingUsersIsNotAllowedForPartner = 50024,
        InvalidMultirequestToken = 50025,
        InvalidArgument = 50026,
        ArgumentCannotBeEmpty = 50027,
        HouseholdForbidden = 500028,
        MediaIdsMustBeNumeric = 500029,
        EpgInternalIdsMustBeNumeric = 500030,
        ArgumentMustBeNumeric = 500031,
        ListTypeCannotBeEmptyOrAll = 500032,
        ActionNotSpecified = 500033,
        RefreshTokenFailed = 500034,
        UnauthorizedUser = 500035,
        ArgumentReadonly = 500036,
        ArgumentInsertonly = 500037,
        ArgumentsConflictsEachOther = 500038,
        TimeInPast = 500039,
        EnumValueNotSupported = 500041,
        MultirequestIndexNotZeroBased = 500042,
        MultirequestInvalidIndex = 500043,
        ArgumentShouldBeEnum = 500044,
        ArgumentMaxLengthCrossed = 500045,
        ArgumentMinLengthCrossed = 500046,
        ArgumentMaxValueCrossed = 500047,
        ArgumentMinValueCrossed = 500048,
        DuplicateAsset = 500049,
        DuplicateFile = 500050,
        PropertyActionForbidden = 500051,
        ActionArgumentForbidden = 500052,
        MissingParameter = 500053,
        InvalidActionParameter = 500054,
        ObjectIdNotFound = 500055,
        ArgumentsCannotBeEmpty = 500056,
        InvalidVersion = 500057,
        ArgumentShouldContainMinValueCrossed = 500058,
        ArgumentShouldContainMaxValueCrossed = 500059,
        InvalidUdid = 500060,
        ArgumentsConflictEachOther = 500061,
        UnableToCreateHouseholdForRole = 500062,
        ExternalError = 500063,
        MultirequestGenericMethod = 500064,
        HttpMethodNotSupported = 500065,
        ArgumentsDuplicate = 500066,
        InvalidArgumentValue = 500067,
        UnknownEnumValue = 500068,
        DuplicateLanguageSent = 500069,
        InvalidValueForFeature = 500070,
        DefaultLanguageMustBeSent = 500071,
        GroupDoesNotContainLanguage = 500072,
        GlobalLanguageParameterMustBeAsterisk = 500073,
        MultiValueWasNotSentForMetaDataTypeString = 500074,
        TagTranslationNotAllowed = 500075,
        InvalidObjectType = 500076,
    }
}