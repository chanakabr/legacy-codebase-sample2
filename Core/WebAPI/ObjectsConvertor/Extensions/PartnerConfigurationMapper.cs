using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Partner;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class PartnerConfigurationMapper
    {
        public static KalturaPartnerConfigurationType GetConfigurationType(this KalturaPartnerConfiguration model)
        {
            switch (model)
            {
                case KalturaBillingPartnerConfig c: return c.ConfigurationType();
                case KalturaConcurrencyPartnerConfig c: return KalturaPartnerConfigurationType.Concurrency;
                case KalturaGeneralPartnerConfig c: return KalturaPartnerConfigurationType.General;
                case KalturaObjectVirtualAssetPartnerConfig c: return KalturaPartnerConfigurationType.ObjectVirtualAsset;
                case KalturaCommercePartnerConfig c: return KalturaPartnerConfigurationType.Commerce;
                case KalturaPlaybackPartnerConfig c: return KalturaPartnerConfigurationType.Playback;
                case KalturaPaymentPartnerConfig c: return KalturaPartnerConfigurationType.Payment;
                case KalturaCatalogPartnerConfig c: return KalturaPartnerConfigurationType.Catalog;
                case KalturaSecurityPartnerConfig c: return KalturaPartnerConfigurationType.Security;
                case KalturaOpcPartnerConfiguration c: return KalturaPartnerConfigurationType.Opc;
                case KalturaBasePartnerConfiguration c: return KalturaPartnerConfigurationType.Base;
                case KalturaCustomFieldsPartnerConfiguration c: return KalturaPartnerConfigurationType.CustomFields;
                case KalturaDefaultParentalSettingsPartnerConfig c: return KalturaPartnerConfigurationType.DefaultParentalSettings;
                default: throw new NotImplementedException($"GetConfigurationType for {model.objectType} is not implemented");
            }
        }

        // KalturaBillingPartnerConfig
        public static KalturaPartnerConfigurationType ConfigurationType(this KalturaBillingPartnerConfig model) 
        {
            if (model.PartnerConfigurationType != null)
                return model.PartnerConfigurationType.type;

            return model.Type;
        }

        internal static HashSet<int> GetDeviceFamilyIds(this KalturaConcurrencyPartnerConfig model)
        {
            HashSet<int> values = null;

            if (model.DeviceFamilyIds == null)
            {
                return values;
            }

            values = new HashSet<int>();

            string[] stringValues = model.DeviceFamilyIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value) && value != 0)
                {
                    if (!values.Add(value))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "deviceFamilyIds");
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "deviceFamilyIds");
                }
            }

            return values;
        }

        internal static List<int> GetSecondaryLanguagesIds(this KalturaGeneralPartnerConfig model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>
                (model.SecondaryLanguages, "KalturaGeneralPartnerConfig.secondaryLanguages", false, false);
        }

        internal static List<int> GetSecondaryCurrenciesIds(this KalturaGeneralPartnerConfig model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>
                (model.SecondaryCurrencies, "KalturaGeneralPartnerConfig.secondaryCurrencies", false, false);
        }

        internal static List<int> GetDowngradePriorityFamilyIds(this KalturaGeneralPartnerConfig model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>
                (model.DowngradePriorityFamilyIds, "KalturaRollingDeviceRemovalData.DowngradePriorityFamilyIds", false, false);
        }

        internal static Dictionary<KalturaTransactionType, int> GetBookmarkEventThresholds(this KalturaCommercePartnerConfig model)
        {
            Dictionary<KalturaTransactionType, int> bookmarkEventThresholds = null;
            if (model.BookmarkEventThresholds != null)
            {
                bookmarkEventThresholds = new Dictionary<KalturaTransactionType, int>();
                foreach (var bookmarkEventThreshold in model.BookmarkEventThresholds)
                {
                    if (bookmarkEventThresholds.ContainsKey(bookmarkEventThreshold.TransactionType))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, bookmarkEventThreshold.TransactionType);
                    }

                    bookmarkEventThresholds.Add(bookmarkEventThreshold.TransactionType, bookmarkEventThreshold.Threshold);
                }
            }

            return bookmarkEventThresholds;
        }

        internal static List<string> GetMetaSystemNameInsteadOfAliasList(this KalturaCustomFieldsPartnerConfiguration model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<string>, string>
                (model.MetaSystemNameInsteadOfAliasList, "KalturaCustomFieldsPartnerConfiguration.metaSystemNameInsteadOfAliasList", false, false);
        }
    }
}