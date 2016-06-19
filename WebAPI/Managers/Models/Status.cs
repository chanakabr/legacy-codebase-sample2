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
        InvalidAppToken = 50020, 
        ExpiredAppToken = 50021, // currently not in use
        InvalidAppTokenHash = 50022,
        NotActiveAppToken = 50023,
        SwitchingUsersIsNotAllowedForPartner = 50024,
        InvalidMultirequestToken = 50025

    }
}