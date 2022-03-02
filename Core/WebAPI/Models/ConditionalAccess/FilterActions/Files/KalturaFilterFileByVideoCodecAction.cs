using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// FilterFile By VideoCode
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "videoCodecIn" })]
    public abstract partial class KalturaFilterFileByVideoCodecAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated videoCodecs
        /// </summary>
        [DataMember(Name = "videoCodecIn")]
        [JsonProperty("videoCodecIn")]
        [XmlElement(ElementName = "videoCodecIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string VideoCodecIn { get; set; }

        public List<string> GetVideoCodecs()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(VideoCodecIn, "videoCodecIn", true);
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByVideoCodecInDiscoveryAction : KalturaFilterFileByVideoCodecAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByVideoCodecInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByVideoCodecInPlayback : KalturaFilterFileByVideoCodecAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByVideoCodecInPlayback;
        }
    }
}