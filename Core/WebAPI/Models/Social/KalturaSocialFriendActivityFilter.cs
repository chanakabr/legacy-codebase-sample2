using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialFriendActivityFilter : KalturaFilter<KalturaSocialFriendActivityOrderBy>
    {
        /// <summary>
        /// Asset ID to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long? AssetIdEqual { get; set; }

        /// <summary>
        /// Asset type to filter by, currently only VOD (media)
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        /// <summary>
        /// Comma separated list of social actions to filter by
        /// </summary>
        [DataMember(Name = "actionTypeIn")]
        [JsonProperty("actionTypeIn")]
        [XmlElement(ElementName = "actionTypeIn")]
        public string ActionTypeIn { get; set; }
        
        public override KalturaSocialFriendActivityOrderBy GetDefaultOrderByValue()
        {
            return KalturaSocialFriendActivityOrderBy.UPDATE_DATE_DESC;
        }
    }
}