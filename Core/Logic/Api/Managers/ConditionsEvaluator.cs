using ApiObjects.Rules;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiLogic.Api.Managers
{
    public class ConditionsEvaluator
    {
        public static bool Evaluate(IDeviceBrandConditionScope scope, DeviceBrandCondition condition)
        {
            if (!scope.BrandId.HasValue) { return true; }

            var isExist = condition.IdIn.Contains(scope.BrandId.Value);
            return isExist;
        }

        public static bool Evaluate(IDeviceFamilyConditionScope scope, DeviceFamilyCondition condition)
        {
            if (!scope.FamilyId.HasValue) { return true; }

            var isExist = condition.IdIn.Contains(scope.FamilyId.Value);
            return isExist;
        }

        public static bool Evaluate(ISegmentsConditionScope scope, SegmentsCondition condition)
        {
            if (!scope.FilterBySegments)
            {
                return true;
            }

            if (scope.SegmentIds != null)
            {
                var intersected = condition.SegmentIds.Intersect(scope.SegmentIds);
                return intersected.Count() == condition.SegmentIds.Count;
            }
            return false;
        }

        public static bool Evaluate(IDeviceManufacturerConditionScope scope, DeviceManufacturerCondition condition)
        {
            if (!scope.ManufacturerId.HasValue) { return true; }

            var isExist = condition.IdIn.Contains(scope.ManufacturerId.Value);
            return isExist;
        }

        public static bool Evaluate(IDeviceModelConditionScope scope, DeviceModelCondition condition)
        {
            if (string.IsNullOrEmpty(scope.Model)) { return true; }

            try
            {
                return Regex.IsMatch(scope.Model, condition.RegexEqual, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool Evaluate(IDynamicKeysConditionScope scope, DynamicKeysCondition condition)
        {
            if (scope.SessionCharacteristics == null || scope.SessionCharacteristics.Count == 0)
            {
                return false;
            }

            var match = scope.SessionCharacteristics.TryGetValue(condition.Key, out var values) 
                        && condition.Values.Intersect(values).Any();
            return match;
        }

        public static bool Evaluate(IConditionScope scope, OrCondition orCondition)
        {
            bool isOneConditionEvaluate = false;
            foreach (var condition in orCondition.Conditions)
            {
                if (scope.Evaluate(condition))
                {
                    isOneConditionEvaluate = true;
                    break;
                }
            }

            if (orCondition.Not)
            {
                isOneConditionEvaluate = !isOneConditionEvaluate;
            }

            return isOneConditionEvaluate;
        }

        public static bool Evaluate(IAssetConditionScope scope, AssetCondition condition)
        {
            if (scope.MediaId == 0)
            {
                return true;
            }

            var rules = scope.GetBusinessModuleRulesByMediaId(scope.GroupId, scope.MediaId);
            if (rules != null && rules.FirstOrDefault(r => r.Id == scope.RuleId) != null)
            {
                return true;
            }

            return false;
        }

        public static bool Evaluate(IIpRangeConditionScope scope, IpRangeCondition condition)
        {
            if (condition.IpFrom <= scope.Ip && scope.Ip <= condition.IpTo)
            {
                return true;
            }

            return false;
        }

        public static bool Evaluate(IBusinessModuleConditionScope scope, BusinessModuleCondition condition)
        {
            return !scope.BusinessModuleType.HasValue ||
                    (condition.BusinessModuleType == scope.BusinessModuleType.Value &&
                     (scope.BusinessModuleId == 0 || condition.BusinessModuleId == 0 || condition.BusinessModuleId == scope.BusinessModuleId));

        }

        public static bool Evaluate(IDateConditionScope scope, DateCondition condition)
        {
            if (!scope.FilterByDate)
            {
                return true;
            }

            long now = ODBCWrapper.Utils.GetUtcUnixTimestampNow();
            bool res = (!condition.StartDate.HasValue || condition.StartDate.Value < now) && (!condition.EndDate.HasValue || now < condition.EndDate.Value);
            if (condition.Not)
            {
                res = !res;
            }

            return res;
        }

        public static bool Evaluate(IHeaderConditionScope scope, HeaderCondition condition)
        {
            bool isInHeaders = false;
            if (scope.Headers.ContainsKey(condition.Key) && scope.Headers[condition.Key].Equals(condition.Value))
            {
                isInHeaders = true;
            }

            if (condition.Not)
            {
                isInHeaders = !isInHeaders;
            }

            return isInHeaders;
        }

        public static bool Evaluate(IUserSubscriptionConditionScope scope, UserSubscriptionCondition condition)
        {
            if (scope.UserSubscriptions == null) { return true; }

            return scope.UserSubscriptions.Any(x => condition.SubscriptionIds.Contains(x));
        }

        public static bool Evaluate(IAssetSubscriptionConditionScope scope, AssetSubscriptionCondition condition)
        {
            if (scope.MediaId == 0) { return true; }

            return scope.IsMediaIncludedInSubscription(scope.GroupId, scope.MediaId, condition.SubscriptionIds);
        }

        public static bool Evaluate(IUserRoleConditionScope scope, UserRoleCondition condition)
        {
            if (string.IsNullOrEmpty(scope.UserId)) { return true; }

            var userRoleIds = scope.GetUserRoleIds(scope.GroupId, scope.UserId);
            return userRoleIds != null && userRoleIds.Any(x => condition.RoleIds.Contains(x));
        }

        public static bool Evaluate(IUdidDynamicListConditionScope scope, UdidDynamicListCondition condition)
        {
            if (string.IsNullOrEmpty(scope.Udid)) { return true; }

            var isExist = scope.CheckDynamicList(condition.Id);
            return isExist;
        }

        public static bool Evaluate(IUserSessionProfileConditionScope scope, UserSessionProfileCondition condition)
        {
            return scope.UserSessionProfileIds.Contains(condition.Id);
        }
        
        public static bool Evaluate(IDeviceDynamicDataConditionScope scope, DeviceDynamicDataCondition condition)
        {
            if (scope.DeviceDynamicData == null) return false;
            var match = scope.DeviceDynamicData.Any(_ => _.key == condition.Key && _.value == condition.Value);
            return match;
        }
    }
}