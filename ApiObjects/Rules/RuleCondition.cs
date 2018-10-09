using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class AssetCondition : AssetRuleCondition
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
    public class IpRangeCondition : AssetCondition
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
        public long StartDate { get; set; }

        [JsonProperty("EndDate")]
        public long EndDate { get; set; }

        public DateCondition()
        {
            this.Type = RuleConditionType.Date;
        }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class SegmentsCondition : NotRuleCondition
    {
        [JsonProperty("SegmentIds")]
        public List<long> SegmentIds { get; set; }

        public SegmentsCondition()
        {
            this.Type = RuleConditionType.Segments;
        }
    }
}
