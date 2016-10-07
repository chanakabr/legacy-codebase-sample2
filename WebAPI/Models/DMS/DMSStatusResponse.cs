using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.DMS
{
    public class DMSStatusResponse
    {
        public string Message { get; set; }

        public string ID { get; set; }
       
        public DMSeResponseStatus Status { get; set; }
    }

    public enum DMSeResponseStatus
    {
        Unknown = 0,
        Error = 1,
        OK = 2,
        Forbidden = 3,
        IllegalQueryParams = 4,
        IllegalPostData = 5,
        NotExist = 6,
        PartnerMismatch = 7,
        AlreadyExist = 8
    }
}