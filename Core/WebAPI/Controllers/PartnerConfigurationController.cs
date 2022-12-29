using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;
using WebAPI.Utils;
using WebAPI.ModelsValidators;

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
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Payment)
                {
                    response = ClientsManager.ApiClient().GetPaymentConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Catalog)
                {
                    response = ClientsManager.ApiClient().GetCatalogConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Security)
                {
                    response = ClientsManager.ApiClient().GetSecurityConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Opc)
                {
                    response = ClientsManager.ApiClient().GetOpcPartnerConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.Base)
                {
                    response = GroupsManager.Instance.GetBaseConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.DefaultParentalSettings)
                {
                    response = ClientsManager.ApiClient().GetParentalDefaultPartnerConfiguration(groupId);
                }
                else if (filter.PartnerConfigurationTypeEqual == KalturaPartnerConfigurationType.CustomFields)
                {
                    response = ClientsManager.ApiClient().GetCustomFieldsConfiguration(groupId);
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
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [Throws(eResponseStatus.ExtendedTypeValueCannotBeChanged)]
        [Throws(eResponseStatus.NoPartnerConfigurationToUpdate)]
        [Throws(eResponseStatus.NoConfigurationValueToUpdate)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.OSSAdapterNotExist)]
        [Throws(eResponseStatus.CategoryTreeDoesNotExist)]
        [Throws(eResponseStatus.RegionDoesNotExist)]
        [Throws(eResponseStatus.PaymentGatewayIdRequired)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.NotAllowed)]
        static public bool Update(KalturaPartnerConfiguration configuration)
        {
            bool response = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                configuration.ValidateForUpdate();

                switch (configuration)
                {
                    case KalturaBillingPartnerConfig c: response = UpdateBillingPartnerConfiguration(groupId, c); break;
                    case KalturaConcurrencyPartnerConfig c: response = UpdateConcurrencyPartnerConfig(groupId, c); break;
                    case KalturaGeneralPartnerConfig c: response = UpdateGeneralPartnerConfiguration(groupId, c); break;
                    case KalturaObjectVirtualAssetPartnerConfig c: response = UpdateObjectVirtualAssetPartnerConfig(groupId, c); break;
                    case KalturaCommercePartnerConfig c: response = UpdateCommercePartnerConfiguration(groupId, c); break;
                    case KalturaPlaybackPartnerConfig c: response = UpdatePlaybackPartnerConfig(groupId, c); break;
                    case KalturaPaymentPartnerConfig c: response = UpdatePaymentPartnerConfig(groupId, c); break;
                    case KalturaCatalogPartnerConfig c: response = UpdateCatalogPartnerConfiguration(groupId, c); break;
                    case KalturaSecurityPartnerConfig c: response = UpdateSecurityPartnerConfig(groupId, c); break;
                    case KalturaOpcPartnerConfiguration c: response = UpdateOpcPartnerConfiguration(groupId, c); break;
                    case KalturaCustomFieldsPartnerConfiguration c: response = UpdateCustomFieldsPartnerConfiguration(groupId, c); break;
                    case KalturaDefaultParentalSettingsPartnerConfig c: response = UpdateDefaultParentalSettingsPartnerConfig(groupId, c); break;
                    default: throw new NotImplementedException($"Update for {configuration.objectType} is not implemented");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        internal static bool UpdateCommercePartnerConfiguration(int groupId, KalturaCommercePartnerConfig model)
        {
            Func<CommercePartnerConfig, Status> commercePartnerConfigFunc =
                (CommercePartnerConfig commercePartnerConfig) =>
                    PartnerConfigurationManager.UpdateCommerceConfig(groupId, commercePartnerConfig);

            ClientUtils.GetResponseStatusFromWS(commercePartnerConfigFunc, model);

            return true;
        }

        internal static bool UpdateCustomFieldsPartnerConfiguration(int groupId, KalturaCustomFieldsPartnerConfiguration model)
        {
            Func<CustomFieldsPartnerConfig, Status> partnerConfigFunc =
                (CustomFieldsPartnerConfig partnerConfig) =>
                    CustomFieldsPartnerConfigManager.Instance.UpdateConfig(groupId, partnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, model);

            return true;
        }

        internal static bool UpdateOpcPartnerConfiguration(int groupId, KalturaOpcPartnerConfiguration model)
        {
            return ClientsManager.ApiClient().UpdateOpcPartnerConfiguration(groupId, model);
        }

        internal static bool UpdateBillingPartnerConfiguration(int groupId, KalturaBillingPartnerConfig model)
        {
            return ClientsManager.BillingClient().SetPartnerConfiguration(groupId, model);
        }

        internal static bool UpdateCatalogPartnerConfiguration(int groupId, KalturaCatalogPartnerConfig model)
        {
            Func<CatalogPartnerConfig, Status> partnerConfigFunc =
                (CatalogPartnerConfig catalogPartnerConfig) => CatalogPartnerConfigManager.Instance.UpdateCatalogConfig(groupId, catalogPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, model);

            return true;
        }

        internal static bool UpdateSecurityPartnerConfig(int groupId, KalturaSecurityPartnerConfig model)
        {
            if (model.Encryption?.Username == null) throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "encryption.username");

            var updaterId = KS.GetContextData().UserId.Value; // never null actually

            ClientUtils.GetResponseStatusFromWS((SecurityPartnerConfig partnerConfig) =>
                PartnerConfigurationManager.UpdateSecurityConfig(groupId, partnerConfig, updaterId), model);

            return true;
        }

        internal static bool UpdateDefaultParentalSettingsPartnerConfig(int groupId, KalturaDefaultParentalSettingsPartnerConfig model)
        {
            Func<DefaultParentalSettingsPartnerConfig, Status> partnerConfigFunc =
                (DefaultParentalSettingsPartnerConfig parentalPartnerConfig) =>
                    DefaultParentalSettingsPartnerConfigManager.Instance.UpsertParentalDefaultConfig
                    (groupId, Utils.Utils.GetUserIdFromKs(), parentalPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, model);

            return true;
        }

        internal static bool UpdatePlaybackPartnerConfig(int groupId, KalturaPlaybackPartnerConfig model)
        {
            Func<PlaybackPartnerConfig, Status> partnerConfigFunc =
                           (PlaybackPartnerConfig playbackPartnerConfig) =>
                               PartnerConfigurationManager.UpdatePlaybackConfig(groupId, playbackPartnerConfig);

            ClientUtils.GetResponseStatusFromWS<KalturaPlaybackPartnerConfig, PlaybackPartnerConfig>(partnerConfigFunc, model);

            return true;
        }

        internal static bool UpdateGeneralPartnerConfiguration(int groupId, KalturaGeneralPartnerConfig model)
        {
            return ClientsManager.ApiClient().UpdateGeneralPartnerConfiguration(groupId, model);
        }

        internal static bool UpdateObjectVirtualAssetPartnerConfig(int groupId, KalturaObjectVirtualAssetPartnerConfig model)
        {
            return ClientsManager.ApiClient().UpdateObjectVirtualAssetPartnerConfiguration(groupId, model);
        }

        internal static bool UpdatePaymentPartnerConfig(int groupId, KalturaPaymentPartnerConfig model)
        {
            Func<PaymentPartnerConfig, Status> partnerConfigFunc =
                (PaymentPartnerConfig paymentPartnerConfig) => PartnerConfigurationManager.UpdatePaymentConfig(groupId, paymentPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, model);

            return true;
        }


        internal static bool UpdateConcurrencyPartnerConfig(int groupId, KalturaConcurrencyPartnerConfig model)
        {
            return ClientsManager.ApiClient().UpdateConcurrencyPartner(groupId, model);
        }
    }
}