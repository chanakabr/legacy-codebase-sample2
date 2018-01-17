using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaKsqlChannel : KalturaChannel
    {
        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]        
        public string FilterExpression
        {
            get;
            set;
        }

        /// <summary>
        /// Asset types in the channel.
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "assetTypes")]
        [JsonProperty(PropertyName = "assetTypes")]
        [XmlArray(ElementName = "assetTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaIntegerValue> AssetTypes { get; set; }

        /// <summary>
        /// Channel order
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        [XmlElement(ElementName = "order")]
        public KalturaAssetOrderBy Order
        {
            get;
            set;
        }

        /// <summary>
        /// Channel group by
        /// </summary>
        [DataMember(Name = "groupBy")]
        [JsonProperty("groupBy")]
        [XmlElement(ElementName = "groupBy")]
        public KalturaAssetGroupBy GroupBy
        {
            get;
            set;
        }

        public int[] getAssetTypes()
        {
            if (AssetTypes == null && MediaTypes != null)
                AssetTypes = MediaTypes;

            if (AssetTypes == null)
                return new int[0];

            int[] assetTypes = new int[AssetTypes.Count];
            for (int i = 0; i < AssetTypes.Count; i++)
            {
                assetTypes[i] = AssetTypes[i].value;
            }

            return assetTypes;
        }

    }
}