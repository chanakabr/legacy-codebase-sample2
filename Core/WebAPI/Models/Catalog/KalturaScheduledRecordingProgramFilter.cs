using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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

        /// <summary>
        /// Series to filter by
        /// </summary>
        [DataMember(Name = "seriesIdsIn")]
        [JsonProperty(PropertyName = "seriesIdsIn")]
        [XmlArray(ElementName = "seriesIdsIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string SeriesIdsIn { get; set; }

        private List<long> ConvertChannelsIn()
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(ChannelsIn, "KalturaScheduledRecordingProgramFilter.ChannelsIn");

        private List<string> ConvertSeriesIdsIn()
            => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(SeriesIdsIn, "KalturaScheduledRecordingProgramFilter.SeriesIdsIn");

        internal override void Validate()
        {
            base.Validate();
            if (!string.IsNullOrEmpty(SeriesIdsIn) && RecordingTypeEqual != KalturaScheduledRecordingAssetType.series)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaScheduledRecordingProgramFilter.SeriesIdsIn", "KalturaScheduledRecordingProgramFilter.RecordingTypeEqual");
            }
        }

        // returns assets that are scheduled to be recorded
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            return ClientsManager.CatalogClient().GetScheduledRecordingAssets(
                contextData,
                ConvertChannelsIn(),
                pager.GetRealPageIndex(),
                pager.PageSize.Value,
                StartDateGreaterThanOrNull,
                EndDateLessThanOrNull,
                Orderings,
                RecordingTypeEqual,
                ConvertSeriesIdsIn());
        }
    }
}