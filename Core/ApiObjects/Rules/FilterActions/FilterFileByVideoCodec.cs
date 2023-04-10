using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByVideoCodec : AssetRuleFilterAction
    {
        [JsonProperty("VideoCodecs")]
        public List<string> VideoCodecs { get; set; }
    }
    
    [Serializable]
    public class FilterFileByVideoCodecInDiscovery : FilterFileByVideoCodec, IFilterFileInDiscovery
    {
        public FilterFileByVideoCodecInDiscovery()
        {
            Type = RuleActionType.FilterFileByVideoCodecInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByVideoCodecInPlayback : FilterFileByVideoCodec, IFilterFileInPlayback
    {
        public FilterFileByVideoCodecInPlayback()
        {
            Type = RuleActionType.FilterFileByVideoCodecInPlayback;
        }
    }
}