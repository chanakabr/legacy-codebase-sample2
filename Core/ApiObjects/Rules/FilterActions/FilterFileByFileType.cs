using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByFileType : AssetRuleFilterAction
    {
        [JsonProperty("FileTypeIds")]
        public HashSet<long> FileTypeIds { get; set; }
    }
    
    [Serializable]
    public class FilterFileByFileTypeInDiscovery : FilterFileByFileType, IFilterFileInDiscovery
    {
        public FilterFileByFileTypeInDiscovery()
        {
            Type = RuleActionType.FilterFileByFileTypeIdInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByFileTypeInPlayback : FilterFileByFileType, IFilterFileInPlayback
    {
        public FilterFileByFileTypeInPlayback()
        {
            Type = RuleActionType.FilterFileByFileTypeIdInPlayback;
        }
    }
}