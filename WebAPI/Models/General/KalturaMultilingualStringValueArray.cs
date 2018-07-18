using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Array of translated strings
    /// </summary>
    public partial class KalturaMultilingualStringValueArray : KalturaOTTObject
    {
        /// <summary>
        /// List of string values
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMultilingualStringValue> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            Objects = new List<KalturaMultilingualStringValue>();
        }
    }
}