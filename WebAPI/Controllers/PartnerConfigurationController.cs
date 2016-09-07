using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models;
using WebAPI.Models.Partner;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/partnerConfiguration/action")]
    public class PartnerConfigurationController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Update Partner Configuration
        /// </summary>
        /// <param name="configuration">Partner Configuration
        /// possible configuration type: 
        /// "configuration": { "value": 0, "partner_configuration_type": { "type": "OSSAdapter", "objectType": "KalturaPartnerConfigurationHolder" },
        /// "objectType": "KalturaBillingPartnerConfig"}
        /// </param>        
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public bool Update(KalturaPartnerConfiguration configuration)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                KalturaBillingPartnerConfig partnerConfig = configuration as KalturaBillingPartnerConfig;
                if (partnerConfig != null)
                {
                    response = ClientsManager.BillingClient().SetPartnerConfiguration(groupId, partnerConfig);
                }
                else
                {
                    throw new InternalServerErrorException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }

}