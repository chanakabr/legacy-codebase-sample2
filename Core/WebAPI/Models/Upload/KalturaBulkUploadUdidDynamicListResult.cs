using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    [Serializable]
    public partial class KalturaBulkUploadUdidDynamicListResult : KalturaBulkUploadDynamicListResult
    {
        /// <summary>
        /// The udid from the excel to add to DynamicLis values
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        [SchemeProperty(ReadOnly = true)]
        public string Udid { get; set; }
    }
}
