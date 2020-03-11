using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaScheduledRecordingProgramFilter : KalturaAssetFilter
    {

        /// <summary>
        ///The type of recordings to return
        /// </summary>
        [DataMember(Name = "recordingTypeEqual")]
        [JsonProperty("recordingTypeEqual")]
        [XmlElement(ElementName = "recordingTypeEqual")]
        public KalturaScheduledRecordingAssetType RecordingTypeEqual { get; set; }

        /// <summary>
        /// Channels to filter by
        /// </summary>
        [DataMember(Name = "channelsIn")]
        [JsonProperty(PropertyName = "channelsIn")]
        [XmlArray(ElementName = "channelsIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string ChannelsIn { get; set; }

        /// <summary>
        /// start date
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrNull")]
        [JsonProperty(PropertyName = "startDateGreaterThanOrNull")]
        [XmlElement(ElementName = "startDateGreaterThanOrNull", IsNullable = true)]
        public long? StartDateGreaterThanOrNull { get; set; }

        /// <summary>
        /// end date
        /// </summary>
        [DataMember(Name = "endDateLessThanOrNull")]
        [JsonProperty(PropertyName = "endDateLessThanOrNull")]
        [XmlElement(ElementName = "endDateLessThanOrNull", IsNullable = true)]
        public long? EndDateLessThanOrNull { get; set; }

        public List<long> ConvertChannelsIn()
        {
            return this.GetItemsIn<List<long>, long>(ChannelsIn, "KalturaScheduledRecordingProgramFilter.ChannelsIn");
        }
    }
}