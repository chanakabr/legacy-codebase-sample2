using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{

    /// <summary>
    /// 0 - reject input with holes
    /// 1 - autofill holes
    /// 2 - keep holes and don’t autofill
    /// </summary>
    public enum KalturaIngestProfileAutofillPolicy
    {
        REJECT = 0,
        AUTOFILL = 1,
        KEEP_HOLES = 2
    }

    /// <summary>
    /// indicates how overlaps in EPG should be managed
    /// (a setting per liniar media id will also be avaiable)
    /// 0 - reject input with overlap
    /// 1 - cut source
    /// 2 - cut target
    /// </summary>
    public enum KalturaIngestProfileOverlapPolicy
    {
        REJECT = 0,
        CUT_SOURCE = 1,
        CUT_TARGET = 2
    }
    /// <summary>
    /// Ingest profile
    /// </summary>
    public partial class KalturaIngestProfile : KalturaOTTObject
    {
        /// <summary>
        /// Ingest profile identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Ingest profile name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Ingest profile externalId
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        // TODO: Check if we should reuse KalturaAssetReferenceType enum or create a new one, or should it be separate objects with a base class
        /// <summary>
        /// Type of assets that this profile suppose to ingest: 0 - EPG, 1 - MEDIA
        /// </summary>
        [DataMember(Name = "assetTypeId")]
        [JsonProperty("assetTypeId")]
        [XmlElement(ElementName = "assetTypeId")]
        public int AssetTypeId { get; set; }

        /// <summary>
        /// Transformation Adapter URL
        /// </summary>
        [DataMember(Name = "transformationAdapterUrl")]
        [JsonProperty("transformationAdapterUrl")]
        [XmlElement(ElementName = "transformationAdapterUrl")]
        public string TransformationAdapterUrl { get; set; }

        /// <summary>
        /// Transformation Adapter settings
        /// </summary>
        [DataMember(Name = "transformationAdapterSettings")]
        [JsonProperty("transformationAdapterSettings")]
        [XmlElement("transformationAdapterSettings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }


        /// <summary>
        /// Transformation Adapter shared secret
        /// </summary>
        [DataMember(Name = "transformationAdapterSharedSecret")]
        [JsonProperty("transformationAdapterSharedSecret")]
        [XmlElement(ElementName = "transformationAdapterSharedSecret")]
        public string TransformationAdapterSharedSecret { get; set; }

        /// <summary>
        /// Ingest profile default Auto-fill policy
        /// </summary>
        [DataMember(Name = "defaultAutoFillPolicy")]
        [JsonProperty("defaultAutoFillPolicy")]
        [XmlElement(ElementName = "defaultAutoFillPolicy")]
        public KalturaIngestProfileAutofillPolicy DefaultAutoFillPolicy { get; set; }

        /// <summary>
        /// Ingest profile default Overlap policy
        /// </summary>
        [DataMember(Name = "defaultOverlapPolicy")]
        [JsonProperty("defaultOverlapPolicy")]
        [XmlElement(ElementName = "defaultOverlapPolicy")]
        public KalturaIngestProfileOverlapPolicy DefaultOverlapPolicy { get; set; }
    }
}