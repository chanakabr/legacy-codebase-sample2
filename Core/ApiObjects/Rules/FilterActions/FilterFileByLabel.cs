using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByLabel : AssetRuleFilterAction
    {
        [JsonProperty("Labels")]
        public List<string> Labels { get; set; }
    }
    
    [Serializable]
    public class FilterFileByLabelInDiscovery : FilterFileByLabel, IFilterFileInDiscovery
    {
        public FilterFileByLabelInDiscovery()
        {
            Type = RuleActionType.FilterFileByLabelInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByLabelInPlayback : FilterFileByLabel, IFilterFileInPlayback
    {
        public FilterFileByLabelInPlayback()
        {
            Type = RuleActionType.FilterFileByLabelInPlayback;
        }
    }
}