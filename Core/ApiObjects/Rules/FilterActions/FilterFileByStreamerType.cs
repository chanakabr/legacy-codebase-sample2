using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    [Serializable]
    public abstract class FilterFileByStreamerType : AssetRuleFilterAction
    {
        [JsonProperty("StreamerTypes")]
        public List<StreamerType> StreamerTypes { get; set; }
    }
    
    [Serializable]
    public class FilterFileByStreamerTypeInDiscovery : FilterFileByStreamerType, IFilterFileInDiscovery
    {
        public FilterFileByStreamerTypeInDiscovery()
        {
            Type = RuleActionType.FilterFileByStreamerTypeInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByStreamerTypeInPlayback : FilterFileByStreamerType, IFilterFileInPlayback
    {
        public FilterFileByStreamerTypeInPlayback()
        {
            Type = RuleActionType.FilterFileByStreamerTypeInPlayback;
        }
    }
}