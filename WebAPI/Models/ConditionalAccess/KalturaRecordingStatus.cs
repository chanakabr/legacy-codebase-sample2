using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingStatus
    {
        SCHEDULED = 0,
        RECORDING = 1,
        RECORDED = 2,
        CANCELED = 3,
        FAILED = 4,
        DELETED = 5
    } 
}