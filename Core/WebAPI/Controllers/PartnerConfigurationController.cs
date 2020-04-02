using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("partnerConfiguration")]
    public class PartnerConfigurationController : IKalturaController
    {
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
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.General)
                {
                    response = ClientsManager.ApiClient().GetGeneralPartnerConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.ObjectVirtualAsset)
                {
                    response = ClientsManager.ApiClient().GetObjectVirtualAssetPartnerConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Commerce)
                {
                    response = ClientsManager.ApiClient().GetCommerceConfigList(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Playback)
                {
                    response = ClientsManager.ApiClient().GetPlaybackAdapterConfiguration(groupId);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "filter.partnerConfigurationTypeEqual", filter.PartnerConfigurationTypeEqual);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update/set Partner Configuration
        /// </summary>
        /// <param name="configuration">Partner Configuration to update</param>
        /// <returns></returns>
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
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = configuration.Update(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}