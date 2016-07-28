using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Schema;
using WebAPI.Exceptions;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrlEpgRequest : KalturaLicensedUrlMediaRequest
    {
        /// <summary>
        /// The stream type to get the URL for
        /// </summary>
        [DataMember(Name = "streamType")]
        [JsonProperty("streamType")]
        [XmlElement(ElementName = "streamType")]
        public KalturaStreamType StreamType { get; set; }

        /// <summary>
        /// The start date of the stream (epoch)
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }
    }
}