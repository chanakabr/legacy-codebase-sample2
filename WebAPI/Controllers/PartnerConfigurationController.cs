using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
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
            int groupId = KSManager.GetKSFromRequest().GroupId;
            
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
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.General)
                {
                    response = ClientsManager.ApiClient().GetGeneralPartnerConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.ObjectVirtualAsset)
                {
                    response = ClientsManager.ApiClient().GetObjectVirtualAssetPartnerConfiguration(groupId);
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
        /// 'configuration': { 'value': 0, 'partner_configuration_type': { 'type': 'OSSAdapter', 'objectType': 'KalturaPartnerConfigurationHolder' },
        /// 'objectType': 'KalturaBillingPartnerConfig'}
        /// </param>        
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.NonExistingDeviceFamilyIds)]
        [Throws(eResponseStatus.InvalidLanguage)]
        [Throws(eResponseStatus.InvalidCurrency)]
        [Throws(eResponseStatus.DlmNotExist)]
        static public bool Update(KalturaPartnerConfiguration configuration)
        {
            bool response = false;
            int groupId = KSManager.GetKSFromRequest().GroupId;

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
                else if (configuration is KalturaGeneralPartnerConfig)
                {
                    KalturaGeneralPartnerConfig partnerConfig = configuration as KalturaGeneralPartnerConfig;
                    response = ClientsManager.ApiClient().UpdateGeneralPartnerConfiguration(groupId, partnerConfig);
                }
                else if (configuration is KalturaObjectVirtualAssetPartnerConfig)
                {
                    KalturaObjectVirtualAssetPartnerConfig partnerConfig = configuration as KalturaObjectVirtualAssetPartnerConfig;
                    response = ClientsManager.ApiClient().UpdateObjectVirtualAssetPartnerConfiguration(groupId, partnerConfig);
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