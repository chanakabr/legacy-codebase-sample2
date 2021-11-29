using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using System.Collections.Generic;
using ApiObjects;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// Filter file By FileType For AssetType
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new [] { "assetTypeIn" })]
    public abstract partial class KalturaFilterFileByFileTypeIdForAssetTypeAction : KalturaFilterFileByFileTypeIdAction
    {
        /// <summary>
        /// List of comma separated assetTypes
        /// </summary>
        [DataMember(Name = "assetTypeIn")]
        [JsonProperty("assetTypeIn")]
        [XmlElement(ElementName = "assetTypeIn")]
        [SchemeProperty(DynamicType = typeof(KalturaAssetType), MinLength = 1)]
        public string AssetTypeIn { get; set; }  // apply action for this asset types only
    }
}