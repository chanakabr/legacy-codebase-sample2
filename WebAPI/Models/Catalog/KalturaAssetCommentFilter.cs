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
    public class KalturaAssetCommentFilter : KalturaFilter<KalturaAssetCommentOrderBy>    
    {
        /// <summary>
        ///Asset Id
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int AssetIdEqual { get; set; }

         /// <summary>
        ///Asset Type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType AssetTypeEqual { get; set; }




        public override KalturaAssetCommentOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetCommentOrderBy.CREATE_DATE_DESC;
        }
    }
}