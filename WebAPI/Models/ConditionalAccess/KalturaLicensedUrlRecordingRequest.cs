using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Schema;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrlRecordingRequest : KalturaLicensedUrlBaseRequest
    {
        /// <summary>
        /// The start date of the recording (epoch)
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }
    }
}