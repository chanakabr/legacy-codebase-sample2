using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Abstarct class - represents an asset parameter that can be used for grouping
    /// </summary>
    [Serializable]
    public abstract class KalturaAssetGroupBy : KalturaOTTObject
    {
        public abstract string GetValue();
    }

    /// <summary>
    /// Group by a tag or meta - according to the name that appears in the system (similar to KSQL)
    /// </summary>
    [Serializable]
    public class KalturaAssetMetaOrTagGroupBy : KalturaAssetGroupBy
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
    /// Different fields that can be used for groupind
    /// </summary>
    public enum KalturaGroupByField
    {
        media_type_id
    }

    /// <summary>
    /// Group by a field that is defined in enum
    /// </summary>
    [Serializable]
    public class KalturaAssetFieldGroupBy : KalturaAssetGroupBy
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