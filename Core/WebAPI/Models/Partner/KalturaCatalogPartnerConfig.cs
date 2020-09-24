using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner catalog configuration
    /// </summary>
    public partial class KalturaCatalogPartnerConfig : KalturaPartnerConfiguration
    {
        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Catalog; } }

        /// <summary>
        /// Single multilingual mode
        /// </summary>
        [DataMember(Name = "singleMultilingualMode")]
        [JsonProperty("singleMultilingualMode")]
        [XmlElement(ElementName = "singleMultilingualMode")]
        public bool? SingleMultilingualMode { get; set; }       

        internal override bool Update(int groupId)
        {
            Func<CatalogPartnerConfig, Status> partnerConfigFunc =
                (CatalogPartnerConfig catalogPartnerConfig) => PartnerConfigurationManager.UpdateCatalogConfig(groupId, catalogPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, this);

            return true;
        }

        internal override void ValidateForUpdate()
        {
        }
    }
}