using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// FilterFile By AudioCodec
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "audioCodecIn" })]
    public abstract partial class KalturaFilterFileByAudioCodecAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated audioCodecs
        /// </summary>
        [DataMember(Name = "audioCodecIn")]
        [JsonProperty("audioCodecIn")]
        [XmlElement(ElementName = "audioCodecIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string AudioCodecIn { get; set; }

        public List<string> GetAudioCodecs()
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(AudioCodecIn, "audioCodecIn", true);
            return types;
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByAudioCodecInDiscoveryAction : KalturaFilterFileByAudioCodecAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByAudioCodecInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByAudioCodecInPlaybackAction : KalturaFilterFileByAudioCodecAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByAudioCodecInPlayback;
        }
    }
}