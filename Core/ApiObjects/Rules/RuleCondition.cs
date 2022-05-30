using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class RuleCondition
    {
        [JsonProperty("Type")]
        public RuleConditionType Type { get; protected set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        public virtual bool IsRuleConditionEquals(RuleConditionType type)
        {
            return Type.Equals(type);
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class NotRuleCondition : RuleCondition
    {
        [JsonProperty("Not")]
        public bool Not { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class AssetRuleCondition : RuleCondition
    {
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class OrCondition : AssetRuleCondition
    {
        [JsonProperty("Conditions", ItemTypeNameHandling = TypeNameHandling.All)]
        public List<RuleCondition> Conditions { get; set; }

        [JsonProperty("Not")]
        public bool Not { get; set; }

        public OrCondition()
        {
            Type = RuleConditionType.Or;
        }

        public override bool IsRuleConditionEquals(RuleConditionType type)
        {
            return base.IsRuleConditionEquals(type) || Conditions.Any(x => x.IsRuleConditionEquals(type));
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetCondition : AssetConditionBase
    {
        [JsonProperty("Ksql")]
        public string Ksql { get; set; }

        public AssetCondition()
        {
            Type = RuleConditionType.Asset;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class CountryCondition : AssetRuleCondition
    {
        [JsonProperty("Not")]
        public bool Not { get; set; }

        [JsonProperty("Countries")]
        public List<int> Countries { get; set; }

        public CountryCondition()
        {
            this.Type = RuleConditionType.Country;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class ConcurrencyCondition : AssetCondition
    {
        [JsonProperty("Limit")]
        public int Limit { get; set; }

        [JsonProperty("RestrictionPolicy")]
        public ConcurrencyRestrictionPolicy RestrictionPolicy { get; set; }

        public ConcurrencyCondition()
        {
            this.Type = RuleConditionType.Concurrency;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class IpRangeCondition : AssetRuleCondition
    {
        [JsonProperty("fromIp")]
        public string FromIp { get; set; }

        [JsonProperty("toIp")]
        public string ToIp { get; set; }

        [JsonProperty("ipFrom")]
        public long IpFrom { get; set; }

        [JsonProperty("ipTo")]
        public long IpTo { get; set; }

        public IpRangeCondition()
        {
            this.Type = RuleConditionType.IP_RANGE;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BusinessModuleCondition : RuleCondition
    {
        [JsonProperty("BusinessModuleId")]
        public long BusinessModuleId { get; set; }

        [JsonProperty("BusinessModuleType")]
        public eTransactionType BusinessModuleType { get; set; }

        public BusinessModuleCondition()
        {
            this.Type = RuleConditionType.BusinessModule;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class DateCondition : NotRuleCondition
    {
        [JsonProperty("StartDate")]
        public long? StartDate { get; set; }

        [JsonProperty("EndDate")]
        public long? EndDate { get; set; }

        public DateCondition()
        {
            this.Type = RuleConditionType.Date;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class HeaderCondition : AssetRuleCondition
    {
        [JsonProperty("Not")]
        public bool Not { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public HeaderCondition()
        {
            this.Type = RuleConditionType.Header;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class SubscriptionCondition : RuleCondition
    {
        [JsonProperty("SubscriptionIds")]
        public HashSet<long> SubscriptionIds { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UserSubscriptionCondition : SubscriptionCondition
    {
        public UserSubscriptionCondition()
        {
            this.Type = RuleConditionType.UserSubscription;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class AssetSubscriptionCondition : SubscriptionCondition
    {
        public AssetSubscriptionCondition()
        {
            this.Type = RuleConditionType.AssetSubscription;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UserRoleCondition : RuleCondition
    {
        [JsonProperty("RoleIds")]
        public HashSet<long> RoleIds { get; set; }

        public UserRoleCondition()
        {
            this.Type = RuleConditionType.UserRole;
        }
    }

    public interface IUdidDynamicListConditionScope : IConditionScope
    {
        string Udid { get; set; }
        bool CheckDynamicList(long id);
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UdidDynamicListCondition : RuleCondition
    {
        public long Id { get; set; }

        public UdidDynamicListCondition()
        {
            Type = RuleConditionType.DeviceUdidDynamicList;
        }
    }
    
    [Serializable]
    public class UserSessionProfileCondition : RuleCondition
    {
        public long Id { get; set; }

        public UserSessionProfileCondition()
        {
            Type = RuleConditionType.UserSessionProfile;
        }
    }
    
    public interface IUserSessionProfileConditionScope : IConditionScope
    {
        IReadOnlyCollection<long> UserSessionProfileIds { get; }
    }
}