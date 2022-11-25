using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Abstarct class - represents an asset parameter that can be used for grouping
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaAssetMetaOrTagGroupBy))]
    [XmlInclude(typeof(KalturaAssetFieldGroupBy))]
    public abstract partial class KalturaAssetGroupBy : KalturaOTTObject
    {
        public abstract string GetValue();
    }

    /// <summary>
    /// Group by a tag or meta - according to the name that appears in the system (similar to KSQL)
    /// </summary>
    [Serializable]
    public partial class KalturaAssetMetaOrTagGroupBy : KalturaAssetGroupBy
    {
        /// <summary>
        /// Group by a tag or meta - according to the name that appears in the system (similar to KSQL)
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }

        public override string GetValue()
        {
            return Value;
        }
    }

    /// <summary>
    /// Different fields that can be used for grouping
    /// </summary>
    public enum KalturaGroupByField
    {
        media_type_id,
        suppressed,
        crid,
        linear_media_id,
        name
    }

    /// <summary>
    /// Group by a field that is defined in enum
    /// </summary>
    [Serializable]
    public partial class KalturaAssetFieldGroupBy : KalturaAssetGroupBy
    {
        /// <summary>
        /// Group by a specific field that is defined in enum
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public KalturaGroupByField Value
        {
            get;
            set;
        }

        public override string GetValue()
        {
            return Value.ToString();
        }
    }
}