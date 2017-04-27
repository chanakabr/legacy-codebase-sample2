using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Managers.Models
{
    //[DataContract]
    //public class Status
    //{
    //    [DataMember(Name = "code")]
    //    public int Code { get; set; }

    //    [DataMember(Name = "message")]
    //    public string Message { get; set; }

    //    [DataMember(Name = "request_id")]
    //    public string RequestID { get; set; }

    //    [DataMember(Name = "execution_time")]
    //    public float ExecutionTime { get; set; }

    //    public Status(int code, string message, Guid reqID, float executionTime)
    //    {
    //        Code = code;
    //        Message = message;
    //        RequestID = reqID.ToString();
    //        ExecutionTime = executionTime;
    //    }

    //    public Status()
    //    {
    //    }
    //}

    [DataContract]
    public class StatusWrapper
    {
        public StatusWrapper()
        {

        }

        public StatusWrapper(int code, Guid reqID, float executionTime, object result = null, string msg = null)
        {
            ExecutionTime = executionTime;
            Result = result;
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
    }
}