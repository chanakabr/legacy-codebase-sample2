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

        public List<eAssetTypes> GetAssetTypes()
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<KalturaAssetType>, KalturaAssetType>(AssetTypeIn, "assetTypeIn", true, true);
            var mapped = AutoMapper.Mapper.Map<List<eAssetTypes>>(types);
            return mapped;
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdForAssetTypeInDiscoveryAction : KalturaFilterFileByFileTypeIdForAssetTypeAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdForAssetTypeInPlaybackAction : KalturaFilterFileByFileTypeIdForAssetTypeAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInPlayback;
        }
    }
}