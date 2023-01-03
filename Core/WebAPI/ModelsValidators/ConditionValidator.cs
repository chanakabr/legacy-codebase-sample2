using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.ModelsValidators
{
    public static class ConditionValidator
    {
        public static void Validate(this KalturaCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            switch (model)
            {
                case KalturaAssetCondition c: ValidateCondition(c, types); break;
                case KalturaCountryCondition c: ValidateCondition(c, types); break;
                case KalturaIpRangeCondition c: ValidateCondition(c, types); break;
                case KalturaBusinessModuleCondition c: ValidateCondition(c, types); break;
                case KalturaSegmentsCondition c: ValidateCondition(c, types); break;
                case KalturaDateCondition c: ValidateCondition(c, types); break;
                case KalturaOrCondition c: ValidateCondition(c, types); break;
                case KalturaHeaderCondition c: ValidateCondition(c, types); break;
                case KalturaSubscriptionCondition c: ValidateCondition(c, types); break;
                case KalturaUserRoleCondition c: ValidateCondition(c, types); break;
                case KalturaDeviceBrandCondition c: ValidateCondition(c, types); break;
                case KalturaDeviceFamilyCondition c: ValidateCondition(c, types); break;
                case KalturaDeviceManufacturerCondition c: ValidateCondition(c, types); break;
                case KalturaDeviceModelCondition c: ValidateCondition(c, types); break;
                case KalturaUdidDynamicListCondition c: ValidateCondition(c, types); break;
                case KalturaDynamicKeysCondition c: ValidateCondition(c, types); break;
                case KalturaUserSessionProfileCondition c: ValidateCondition(c, types); break;
                case KalturaDeviceDynamicDataCondition c: ValidateCondition(c, types); break;
                case KalturaIpV6RangeCondition c: ValidateCondition(c, types); break;
                case KalturaAssetShopCondition c: ValidateCondition(c, types); break;
                case KalturaFileTypeCondition c: ValidateCondition(c, types); break;
                case KalturaChannelCondition c: ValidateCondition(c, types); break;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }

        public static int ConditionsCount(this KalturaCondition model)
        {
            switch (model)
            {
                case KalturaOrCondition c: return c.ConditionsCount();
                default: return 1;
            }
        }

        private static void ValidateCondition(KalturaOrCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            foreach (var condition in model.Conditions)
            {
                if (types != null && !types.Contains(condition.Type))
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaOrCondition.conditions", condition.objectType);
                }

                condition.Validate();
            }
        }

        private static int ConditionsCount(this KalturaOrCondition model)
        {
            return model.Conditions.Sum(_ => _.ConditionsCount());
        }

        private static void ValidateCondition(KalturaCountryCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.Countries))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaCountryCondition.countries");
            }
        }

        private static void ValidateCondition(KalturaAssetCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.Ksql) || string.IsNullOrWhiteSpace(model.Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
            }
        }

        private static void ValidateCondition(KalturaIpRangeCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            string ipRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

            if (string.IsNullOrEmpty(model.FromIP) || !Regex.IsMatch(model.FromIP, ipRegex))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fromIP");
            }

            if (string.IsNullOrEmpty(model.ToIP) || !Regex.IsMatch(model.ToIP, ipRegex))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "toIP");
            }

            // check for range IP
            string[] fromIPSplited = model.FromIP.Split('.');
            Int64 ipFrom = Int64.Parse(fromIPSplited[3]) + Int64.Parse(fromIPSplited[2]) * 256 + Int64.Parse(fromIPSplited[1]) * 256 * 256 + Int64.Parse(fromIPSplited[0]) * 256 * 256 * 256;

            string[] toIPSplited = model.ToIP.Split('.');
            Int64 ipTo = Int64.Parse(toIPSplited[3]) + Int64.Parse(toIPSplited[2]) * 256 + Int64.Parse(toIPSplited[1]) * 256 * 256 + Int64.Parse(toIPSplited[0]) * 256 * 256 * 256;

            if (ipTo < ipFrom)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "fromIP", "toIP");
            }
        }

        private static void ValidateCondition(KalturaBusinessModuleCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if ((!model.BusinessModuleId.HasValue || model.BusinessModuleId == 0) && !model.BusinessModuleType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition.businessModuleType");
            }
        }

        private static void ValidateCondition(KalturaSegmentsCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.SegmentsIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition.segmentsIds");
            }
        }

        private static void ValidateCondition(KalturaDateCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (model.StartDate == 0 && model.EndDate == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "condition.startDate", "condition.endDate");
            }
        }

        private static void ValidateCondition(KalturaHeaderCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.Key))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaHeaderCondition.key");
            }

            if (string.IsNullOrEmpty(model.Value))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaHeaderCondition.value");
            }
        }

        private static void ValidateCondition(KalturaSubscriptionCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSubscriptionCondition.idIn");
            }
        }

        private static void ValidateCondition(KalturaUserRoleCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserRoleCondition.idIn");
            }
        }

        private static void ValidateCondition(KalturaDeviceBrandCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceBrandCondition.idIn");
            }

            var items = model.GetDeviceBrandIds();
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceBrandCondition.idIn", 10);
            }
        }

        private static void ValidateCondition(KalturaDeviceFamilyCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceFamilyCondition.idIn");
            }

            var items = model.GetDeviceFamilyIds();
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceFamilyCondition.idIn", 10);
            }
        }

        private static void ValidateCondition(KalturaDeviceManufacturerCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceManufacturerCondition.idIn");
            }

            var items = model.GetDeviceManufacturerIds();
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceManufacturerCondition.idIn", 10);
            }
        }

        private static void ValidateCondition(KalturaDeviceModelCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.RegexEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceModelCondition.regexEqual");
            }

            if (!StringUtils.IsValidRegex(model.RegexEqual))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaDeviceModelCondition.regexEqual");
            }
        }

        private static void ValidateCondition(KalturaUdidDynamicListCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (model.Id < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUdidDynamicListCondition.id");
            }
        }

        private const int DYNAMIC_KEYS_VALUES_ARRAY_LIMIT = 16;

        private static void ValidateCondition(KalturaDynamicKeysCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrWhiteSpace(model.Key))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    "KalturaDynamicKeysCondition.key");
            }

            var values = model.GetValues();

            if (values == null || values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    $"KalturaDynamicKeysCondition.values");
            }

            if (values.Count > DYNAMIC_KEYS_VALUES_ARRAY_LIMIT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED,
                    $"KalturaDynamicKeysCondition.values", DYNAMIC_KEYS_VALUES_ARRAY_LIMIT);
            }

            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value == null || string.IsNullOrWhiteSpace(value))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                        $"KalturaDynamicKeysCondition.values[{i}]");
                }

                SchemeInputAttribute.ValidatePattern(SchemeInputAttribute.ASCII_ONLY_PATTERN,
                    $"KalturaDynamicKeysCondition.values[{i}]", value);
            }
        }

        private static void ValidateCondition(KalturaUserSessionProfileCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
        }

        private static void ValidateCondition(KalturaDeviceDynamicDataCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
        }

        private const string IPV6_REGEX = @"(?:^|(?<=\s))(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$)";

        private static void ValidateCondition(KalturaIpV6RangeCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(model.FromIP) || !Regex.IsMatch(model.FromIP, IPV6_REGEX))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fromIP");
            }

            if (string.IsNullOrEmpty(model.ToIP) || !Regex.IsMatch(model.ToIP, IPV6_REGEX))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "toIP");
            }

            var rangeValidator = new TVinciShared.IPAddressRange().Init(model.FromIP, model.ToIP);
            if (!rangeValidator.IsInRange(model.FromIP))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "fromIP", "toIP");
            }
        }

        private static void ValidateCondition(KalturaAssetShopCondition model, HashSet<KalturaRuleConditionType> types = null)
        {
            if (model.Values == null || model.Values.Objects == null || model.Values.Objects.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "values");
            }
        }

        private static void ValidateCondition(KalturaFileTypeCondition model, HashSet<KalturaRuleConditionType> types = null) { }

        private static void ValidateCondition(KalturaChannelCondition model, HashSet<KalturaRuleConditionType> types = null) { }
    }
}
