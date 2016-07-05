using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Filtering Assets requests
    /// </summary>
    [Serializable]
    public class KalturaBookmarkFilter : KalturaFilter<KalturaBookmarkOrderBy>
    {

        /// <summary>
        /// List of assets identifier
        /// </summary>
        [DataMember(Name = "assetIn")]
        [JsonProperty(PropertyName = "assetIn")]
        [XmlArray(ElementName = "assetIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSlimAsset> AssetIn { get; set; }

        public override KalturaBookmarkOrderBy GetDefaultOrderByValue()
        {
            return KalturaBookmarkOrderBy.POSITION_ASC;
        }
    }
}