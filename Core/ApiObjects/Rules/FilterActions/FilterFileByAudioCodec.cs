using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Rules.FilterActions
{
    public abstract class FilterFileByAudioCodec : AssetRuleFilterAction
    {
        [JsonProperty("AudioCodecs")]
        public List<string> AudioCodecs { get; set; }
    }
    
    [Serializable]
    public class FilterFileByAudioCodecInDiscovery : FilterFileByAudioCodec, IFilterFileInDiscovery
    {
        public FilterFileByAudioCodecInDiscovery()
        {
            Type = RuleActionType.FilterFileByAudioCodecInDiscovery;
        }
    }

    [Serializable]
    public class FilterFileByAudioCodecInPlayback : FilterFileByAudioCodec, IFilterFileInPlayback
    {
        public FilterFileByAudioCodecInPlayback()
        {
            Type = RuleActionType.FilterFileByAudioCodecInPlayback;
        }
    }
}