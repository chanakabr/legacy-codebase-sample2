using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByQuality : AssetRuleAction
    {
        [JsonProperty("Qualities")]
        public List<MediaFileTypeQuality> Qualities { get; set; }
    }
    
    [Serializable]
    public class FilterFileByQualityInDiscovery : FilterFileByQuality, IFilterFileInDiscovery
    {
        public FilterFileByQualityInDiscovery()
        {
            Type = RuleActionType.FilterFileByQualityInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByQualityInPlayback : FilterFileByQuality, IFilterFileInPlayback
    {
        public FilterFileByQualityInPlayback()
        {
            Type = RuleActionType.FilterFileByQualityInPlayback;
        }
    }
}