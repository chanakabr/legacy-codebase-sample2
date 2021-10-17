using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByFileTypeForAssetType : FilterFileByFileType
    {
        [JsonProperty("AssetTypes")]
        public List<eAssetTypes> AssetTypes { get; set; }  // apply action for this asset types only
    }
    
    [Serializable]
    public class FilterFileByFileTypeForAssetTypeInDiscovery : FilterFileByFileTypeForAssetType, IFilterFileInDiscovery
    {
        public FilterFileByFileTypeForAssetTypeInDiscovery()
        {
            Type = RuleActionType.FilterFileByAssetTypeInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByFileTypeForAssetTypenPlayback : FilterFileByFileTypeForAssetType, IFilterFileInPlayback
    {
        public FilterFileByFileTypeForAssetTypenPlayback()
        {
            Type = RuleActionType.FilterFileByAssetTypeInPlayback;
        }
    }
}