using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// KSQL Channel
    /// </summary>
    public class KalturaKSQLChannelProfile : KalturaOTTObject
    {
        /// <summary>
        /// Channel id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// active status
        /// </summary>
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active")]
        public bool IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filter_expression")]
        [JsonProperty("filter_expression")]
        [XmlElement(ElementName = "filter_expression")]
        public string FilterExpression
        {
            get;
            set;
        }

        /// <summary>
        /// Asset types. Media types - taken from group's definition. EPG is -26.
        /// </summary>
        [DataMember(Name = "asset_types")]
        [JsonProperty("asset_types")]
        [XmlArray(ElementName = "asset_types", IsNullable = true)]
        [XmlArrayItem("asset_types")]
        public List<int> AssetTypes
        {
            get;
            set;
        }

        /// <summary>
        /// Channel order
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        [XmlElement(ElementName = "order")]
        public KalturaOrder Order
        {
            get;
            set;
        }
    }

    //[DataContract]
    //public enum OrderBy
    //{
    //    [EnumMember]
    //    ID = 0,
    //    [EnumMember]
    //    VIEWS = -7,
    //    [EnumMember]
    //    RATING = -8,
    //    [EnumMember]
    //    VOTES_COUNT = -80,
    //    [EnumMember]
    //    LIKE_COUNTER = -9,
    //    [EnumMember]
    //    START_DATE = -10,
    //    [EnumMember]
    //    NAME = -11,
    //    [EnumMember]
    //    CREATE_DATE = -12,
    //    [EnumMember]
    //    META = 100,
    //    [EnumMember]
    //    RANDOM = -6,
    //    [EnumMember]
    //    RELATED = 31,
    //    [EnumMember]
    //    NONE = 101,
    //    [EnumMember]
    //    RECOMMENDATION = -13
    //}
}