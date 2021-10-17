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
    /// FilterFile By FileType
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "fileTypeIdIn" })]
    public abstract partial class KalturaFilterFileByFileTypeIdAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated fileTypesIds
        /// </summary>
        [DataMember(Name = "fileTypeIdIn")]
        [JsonProperty("fileTypeIdIn")]
        [XmlElement(ElementName = "fileTypeIdIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string FileTypeIdIn { get; set; }

        public HashSet<long> GetFileTypesIds()
        {
            var types = this.GetItemsIn<HashSet<long>, long>(FileTypeIdIn, "fileTypeIdIn", true);
            return types;
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdInDiscoveryAction : KalturaFilterFileByFileTypeIdAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdInPlaybackAction : KalturaFilterFileByFileTypeIdAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdInPlayback;
        }
    }
}