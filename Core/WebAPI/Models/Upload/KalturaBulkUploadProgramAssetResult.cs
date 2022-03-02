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
    public partial class KalturaBulkUploadProgramAssetResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// The programID that was created
        /// </summary>
        [DataMember(Name = "programId")]
        [JsonProperty(PropertyName = "programId")]
        [XmlElement(ElementName = "programId")]
        [SchemeProperty(ReadOnly = true)]
        public int? ProgramId { get; set; }

        /// <summary>
        /// The external program Id as was sent in the bulk xml file
        /// </summary>
        [DataMember(Name = "programExternalId")]
        [JsonProperty(PropertyName = "programExternalId")]
        [XmlElement(ElementName = "programExternalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ProgramExternalId { get; set; }

        /// <summary>
        /// The  live asset Id that was identified according liveAssetExternalId that was sent in bulk xml file
        /// </summary>
        [DataMember(Name = "liveAssetId")]
        [JsonProperty(PropertyName = "liveAssetId")]
        [XmlElement(ElementName = "liveAssetId")]
        [SchemeProperty(ReadOnly = true)]
        public int LiveAssetId { get; set; }

    }
}
