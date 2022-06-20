using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaLiveToVodInfoAsset : KalturaOTTObject
    {
        /// <summary>
        /// Linear Asset Id
        /// </summary>
        [DataMember(Name = "linearAssetId")]
        [JsonProperty(PropertyName = "linearAssetId")]
        [XmlElement(ElementName = "linearAssetId")]
        public long LinearAssetId { get; set; }
        
        /// <summary>
        /// EPG Id
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty(PropertyName = "epgId")]
        [XmlElement(ElementName = "epgId")]
        public string EpgId { get; set; }
        
        /// <summary>
        /// EPG Channel Id
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        public long EpgChannelId { get; set; }

        /// <summary>
        /// Crid
        /// </summary>
        [DataMember(Name = "crid")]
        [JsonProperty(PropertyName = "crid")]
        [XmlElement(ElementName = "crid")]
        public string Crid { get; set; }
        
        /// <summary>
        /// Original Start Date
        /// </summary>
        [DataMember(Name = "originalStartDate")]
        [JsonProperty(PropertyName = "originalStartDate")]
        [XmlElement(ElementName = "originalStartDate")]
        public long OriginalStartDate { get; set; }
        
        /// <summary>
        /// Original End Date
        /// </summary>
        [DataMember(Name = "originalEndDate")]
        [JsonProperty(PropertyName = "originalEndDate")]
        [XmlElement(ElementName = "originalEndDate")]
        public long OriginalEndDate { get; set; }
        
        /// <summary>
        /// Padding before program starts
        /// </summary>
        [DataMember(Name = "paddingBeforeProgramStarts")]
        [JsonProperty(PropertyName = "paddingBeforeProgramStarts")]
        [XmlElement(ElementName = "paddingBeforeProgramStarts")]
        public long PaddingBeforeProgramStarts { get; set; }
        
        /// <summary>
        /// Padding after program ends
        /// </summary>
        [DataMember(Name = "paddingAfterProgramEnds")]
        [JsonProperty(PropertyName = "paddingAfterProgramEnds")]
        [XmlElement(ElementName = "paddingAfterProgramEnds")]
        public long PaddingAfterProgramEnds { get; set; }
    }
}
