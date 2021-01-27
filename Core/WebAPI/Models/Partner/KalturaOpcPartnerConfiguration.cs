using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial  class KalturaOpcPartnerConfiguration : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Reset Password
        /// </summary>
        [DataMember(Name = "resetPassword")]
        [JsonProperty("resetPassword")]
        [XmlElement(ElementName = "resetPassword")]
        public KalturaResetPasswordPartnerConfig ResetPassword { get; set; }

        internal override bool Update(int groupId)
        {
            return ClientsManager.ApiClient().UpdateOpcPartnerConfiguration(groupId, this);
        }

        public override void ValidateForUpdate()
        {
            base.ValidateForUpdate();

            if (ResetPassword == null || ResetPassword.Templates == null)
            {
                return;
            }

            //REQ-03.3
            if (ResetPassword.Templates.Count() == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaResetPasswordPartnerConfigTemplate");
            }

            //REQ-03.2.1 + REQ-03.2.2
            if (CheckDuplications(ResetPassword.Templates))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, 
                    "KalturaResetPasswordPartnerConfigTemplate.Id OR KalturaResetPasswordPartnerConfigTemplate.Label");
            }

            //REQ-03.2.3
            if (ResetPassword.Templates.Where(t => t.IsDefault).Count() != 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaResetPasswordPartnerConfigTemplate.IsDefault", "Defaults");
            }
        }

        private bool CheckDuplications(List<KalturaResetPasswordPartnerConfigTemplate> templates)
        {
            var gb = templates.GroupBy(x => x.Id)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            if (gb.Count > 0)
                return true;

            gb = templates.GroupBy(x => x.Label)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            return gb.Count > 0;
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Opc; } }
    }
}
