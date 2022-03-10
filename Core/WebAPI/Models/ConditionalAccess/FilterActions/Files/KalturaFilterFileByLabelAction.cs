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
    /// FilterFile By Label
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "labelIn" })]
    public abstract partial class KalturaFilterFileByLabelAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated labels
        /// </summary>
        [DataMember(Name = "labelIn")]
        [JsonProperty("labelIn")]
        [XmlElement(ElementName = "labelIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string LabelIn { get; set; }

        public List<string> GetLabels()
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(LabelIn, "labelIn", true);
            return types;
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByLabelInDiscoveryAction : KalturaFilterFileByLabelAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByLabelInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByLabelInPlaybackAction : KalturaFilterFileByLabelAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByLabelInPlayback;
        }
    }
}