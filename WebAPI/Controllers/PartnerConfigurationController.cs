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
        /// Get the list of PartnerConfiguration
        /// </summary>
        /// <param name="filter">filter by PartnerConfiguration type</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPartnerConfigurationListResponse List(KalturaPartnerConfigurationFilter filter)
        {
            KalturaPartnerConfigurationListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Concurrency)
                {
                    response = ClientsManager.ApiClient().GetConcurrencyPartner(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.DefaultPaymentGateway)
                {
                    throw new NotImplementedException();
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.EnablePaymentGatewaySelection)
                {
                    throw new NotImplementedException();
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.OSSAdapter)
                {
                    throw new NotImplementedException();
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
                if (configuration is KalturaBillingPartnerConfig)
                {
                    KalturaBillingPartnerConfig partnerConfig = configuration as KalturaBillingPartnerConfig;
                    response = ClientsManager.BillingClient().SetPartnerConfiguration(groupId, partnerConfig);
                }
                if (configuration is KalturaConcurrencyPartnerConfig)
                {
                    KalturaConcurrencyPartnerConfig partnerConfig = configuration as KalturaConcurrencyPartnerConfig;
                    response = ClientsManager.ApiClient().UpdateConcurrencyPartner(groupId, partnerConfig);
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