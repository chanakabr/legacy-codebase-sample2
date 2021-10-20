using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Exceptions;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Custom Fields Partner Configuration
    /// </summary>
    public partial class KalturaCustomFieldsPartnerConfiguration : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Array of clientTag values
        /// </summary>
        [DataMember(Name = "metaSystemNameInsteadOfAliasList")]
        [JsonProperty("metaSystemNameInsteadOfAliasList")]
        [XmlElement(ElementName = "metaSystemNameInsteadOfAliasList")]
        [Managers.Scheme.SchemeProperty(IsNullable = false)]
        public string MetaSystemNameInsteadOfAliasList { get; set; }

        internal override bool Update(int groupId)
        {
            Func<CustomFieldsPartnerConfig, Status> partnerConfigFunc =
                (CustomFieldsPartnerConfig partnerConfig) => 
                    CustomFieldsPartnerConfigManager.Instance.UpdateConfig(groupId, partnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, this);

            return true;
        }

        internal List<string> GetMetaSystemNameInsteadOfAliasList()
        {
            return GetItemsIn<List<string>, string>(MetaSystemNameInsteadOfAliasList, "KalturaCustomFieldsPartnerConfiguration.metaSystemNameInsteadOfAliasList", false, false);
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.CustomFields; } }
    }
}
