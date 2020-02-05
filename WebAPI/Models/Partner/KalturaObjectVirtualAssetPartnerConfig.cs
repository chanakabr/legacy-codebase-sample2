using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// 
    /// </summary>
    public partial class KalturaObjectVirtualAssetPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        ///List of object virtual asset info
        /// </summary>
        [DataMember(Name = "objectVirtualAssets")]
        [JsonProperty("objectVirtualAssets")]
        [XmlElement(ElementName = "objectVirtualAssets")]
        public List<KalturaObjectVirtualAssetInfo> ObjectVirtualAssets { get; set; }

        internal override bool Update(int groupId)
        {
            return ClientsManager.ApiClient().UpdateObjectVirtualAssetPartnerConfiguration(groupId, this);
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.ObjectVirtualAsset; } }
    }
}