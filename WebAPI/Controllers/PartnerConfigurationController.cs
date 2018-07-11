using ApiObjects.Response;
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
    [Service("partnerConfiguration")]
    public class PartnerConfigurationController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get the list of PartnerConfiguration
        /// </summary>
        /// <param name="filter">filter by PartnerConfiguration type</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPartnerConfigurationListResponse List(KalturaPartnerConfigurationFilter filter)
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
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, 
                                                  "filter.partnerConfigurationTypeEqual", 
                                                  KalturaPartnerConfigurationType.Concurrency.ToString());
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.EnablePaymentGatewaySelection)
                {
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE,
                                                  "filter.partnerConfigurationTypeEqual",
                                                  KalturaPartnerConfigurationType.Concurrency.ToString());
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.OSSAdapter)
                {
                    throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE,
                                                  "filter.partnerConfigurationTypeEqual",
                                                  KalturaPartnerConfigurationType.Concurrency.ToString());
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
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.NonExistingDeviceFamilyIds)]
        static public bool Update(KalturaPartnerConfiguration configuration)
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
                else if (configuration is KalturaConcurrencyPartnerConfig)
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