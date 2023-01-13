using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Recording-asset info
    /// </summary>
    [Serializable]
    public partial class KalturaRecordingAsset : KalturaProgramAsset
    {
        public KalturaRecordingAsset(KalturaProgramAsset asset)
        {
            this.CatchUpEnabled = asset.EnableCatchUp;
            this.CdvrEnabled = asset.EnableCdvr;
            this.CreateDate = asset.CreateDate;
            this.Crid = this.Crid;
            this.Description = asset.Description;
            this.EnableCatchUp = asset.EnableCatchUp;
            this.EnableCdvr = asset.EnableCdvr;
            this.EnableStartOver = asset.EnableStartOver;
            this.EnableTrickPlay = asset.EnableTrickPlay;
            this.EndDate = asset.EndDate;
            this.EpgId = asset.Id?.ToString();
            this.EpgChannelId = asset.EpgChannelId;
            this.EpgId = asset.EpgId;
            this.TrickPlayEnabled = asset.EnableTrickPlay;
            this.Images = asset.Images;
            this.IndexStatus = asset.IndexStatus;
            this.MediaFiles = asset.MediaFiles;
            this.Metas = asset.Metas;
            this.Name = asset.Name;
            this.RelatedEntities = asset.RelatedEntities;
            this.relatedObjects = asset.relatedObjects;
            this.StartDate = asset.StartDate;
            this.StartOverEnabled = asset.EnableStartOver;
            this.Statistics = asset.Statistics;
            this.Tags = asset.Tags;
            this.Type = asset.Type;
            this.UpdateDate = asset.UpdateDate;
            this.RelatedMediaId = asset.RelatedMediaId;
            this.LinearAssetId = asset.LinearAssetId;
            this.Id = asset.Id;
            this.ExternalId = asset.ExternalId;
            this.ExternalOfferIds = asset.ExternalOfferIds;
        }

        /// <summary>
        /// Recording identifier
        /// </summary>
        [DataMember(Name = "recordingId")]
        [JsonProperty(PropertyName = "recordingId")]
        [XmlElement(ElementName = "recordingId")]
        public string RecordingId
        {
            get;
            set;
        }

        /// <summary>
        /// Recording Type: single/season/series
        /// </summary>
        [DataMember(Name = "recordingType")]
        [JsonProperty(PropertyName = "recordingType")]
        [XmlElement(ElementName = "recordingType", IsNullable = true)]
        public WebAPI.Models.ConditionalAccess.KalturaRecordingType? RecordingType { get; set; }

        /// <summary>
        /// Specifies until when the recording is available for viewing. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "viewableUntilDate")]
        [JsonProperty(PropertyName = "viewableUntilDate")]
        [XmlElement(ElementName = "viewableUntilDate")]
        public long ViewableUntilDate { get; set; }

        /// <summary>
        /// When TRUE indicates that there are multiple KalturaImmediateRecording instances for the event.
        /// </summary>
        [DataMember(Name = "multiRecord")]
        [JsonProperty(PropertyName = "multiRecord")]
        [XmlElement(ElementName = "multiRecord")]
        public bool MultiRecord { get; set; }
    }
}