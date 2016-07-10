using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
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
        [Obsolete]
        [DataMember(Name = "assetIn")]
        [JsonProperty(PropertyName = "assetIn")]
        [XmlArray(ElementName = "assetIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSlimAsset> AssetIn { get; set; }

        /// <summary>
        /// Comma separated list of assets identifiers
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlArray(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        /// <summary>
        /// Asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty(PropertyName = "assetTypeEqual")]
        [XmlArray(ElementName = "assetTypeEqual", IsNullable = true)]
        public KalturaAssetType AssetTypeEqual { get; set; }

        public override KalturaBookmarkOrderBy GetDefaultOrderByValue()
        {
            return KalturaBookmarkOrderBy.POSITION_ASC;
        }

        internal void Validate()
        {
            if (AssetIn != null && AssetIn.Count > 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(AssetIdIn))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter.AssetIdIn cannot be empty");
            }

            if (AssetTypeEqual == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter.AssetTypeEqual cannot be empty");
            }
        }

        internal List<KalturaSlimAsset> getAssetIn()
        {
            if (AssetIn != null && AssetIn.Count > 0)
                return AssetIn;

            if (string.IsNullOrEmpty(AssetIdIn))
                return null;

            List<KalturaSlimAsset> values = new List<KalturaSlimAsset>();
            string[] stringValues = AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in stringValues)
            {
                KalturaSlimAsset asset = new KalturaSlimAsset();
                asset.Id = value;
                asset.Type = AssetTypeEqual;
                values.Add(asset);
            }

            return values;
        }
    }
}