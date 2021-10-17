using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using System.Collections.Generic;
using ApiObjects;
using WebAPI.Models.ConditionalAccess.FilterActions;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// FilterFile By StreamerType
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "streamerTypeIn" })]
    public abstract partial class KalturaFilterFileByStreamerTypeAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated streamerTypes
        /// </summary>
        [DataMember(Name = "streamerTypeIn")]
        [JsonProperty("streamerTypeIn")]
        [XmlElement(ElementName = "streamerTypeIn")]
        [SchemeProperty(DynamicType = typeof(KalturaMediaFileStreamerType), MinLength = 1)]
        public string StreamerTypeIn { get; set; }

        public List<StreamerType> GetStreamerTypes()
        {
            var streamerTypes = this.GetItemsIn<List<KalturaMediaFileStreamerType>, KalturaMediaFileStreamerType>(StreamerTypeIn, "streamerTypeIn", true, true);
            return AutoMapper.Mapper.Map<List<StreamerType>>(streamerTypes);
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByStreamerTypeInDiscovery : KalturaFilterFileByStreamerTypeAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByStreamerTypeInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByStreamerTypeInPlayback : KalturaFilterFileByStreamerTypeAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByStreamerTypeInPlayback;
        }
    }
}