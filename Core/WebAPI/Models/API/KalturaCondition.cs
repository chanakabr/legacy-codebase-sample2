using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaRuleConditionType
    {
        ASSET,
        COUNTRY,
        CONCURRENCY,
        IP_RANGE,
        BUSINESS_MODULE,
        SEGMENTS,
        DATE,
        OR,
        HEADER,
        USER_SUBSCRIPTION,
        ASSET_SUBSCRIPTION,
        USER_ROLE,
        DEVICE_BRAND,
        DEVICE_FAMILY,
        DEVICE_MANUFACTURER,
        DEVICE_MODEL,
        DEVICE_UDID_DYNAMIC_LIST,
        DYNAMIC_KEYS,
        USER_SESSION_PROFILE,
        DEVICE_DYNAMIC_DATA,
        IP_V6_RANGE,
        ASSET_SHOP
    }

    /// <summary>
    /// Condition
    /// </summary>
    [Serializable]
    public abstract partial class KalturaCondition : KalturaOTTObject
    {
        /// <summary>
        /// The type of the condition
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaRuleConditionType Type { get; protected set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        public abstract void Validate(HashSet<KalturaRuleConditionType> types = null);

        public virtual int ConditionsCount() { return 1; }
    }

    /// <summary>
    /// Not condition
    /// </summary>
    public abstract partial class KalturaNotCondition : KalturaCondition
    {
        /// <summary>
        /// Indicates whether to apply not on the other properties in the condition
        /// </summary>
        [DataMember(Name = "not")]
        [JsonProperty("not")]
        [XmlElement(ElementName = "not")]
        public bool? Not
        {
            get { return not; }
            set { not = value.HasValue ? value.Value : false; }
        }
        private bool not;
    }

    [SchemeClass(Required = new string[] { "conditions" })]
    public partial class KalturaOrCondition : KalturaNotCondition
    {
        /// <summary>
        /// List of conditions with or between them  
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaCondition> Conditions { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.OR;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            foreach (var condition in this.Conditions)
            {
                if (types != null && !types.Contains(condition.Type))
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "KalturaOrCondition.conditions", condition.objectType);
                }

                condition.Validate();
            }
        }
 
        public override int ConditionsCount()
        {
            return this.Conditions.Sum(_ => _.ConditionsCount());
        }
    }

    /// <summary>
    /// Country condition
    /// </summary>
    public partial class KalturaCountryCondition : KalturaNotCondition
    {
        /// <summary>
        /// Comma separated countries IDs list
        /// </summary>
        [DataMember(Name = "countries")]
        [JsonProperty("countries")]
        [XmlElement(ElementName = "countries")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string Countries { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.COUNTRY;
        }

        public List<int> getCountries()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(Countries, "KalturaCountryCondition.countries");
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(Countries))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaCountryCondition.countries");
            }
        }
    }

    /// <summary>
    /// Asset Condition
    /// </summary>
    [Serializable]
    public partial class KalturaAssetCondition : KalturaAssetConditionBase
    {
        /// <summary>
        /// KSQL  
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty("ksql")]
        [XmlElement(ElementName = "ksql")]
        public string Ksql { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.ASSET;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(Ksql) || string.IsNullOrWhiteSpace(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
            }
        }
    }

    /// <summary>
    /// Asset Condition
    /// </summary>
    [Serializable]
    public partial class KalturaConcurrencyCondition : KalturaAssetCondition
    {
        /// <summary>
        /// Concurrency limitation  
        /// </summary>
        [DataMember(Name = "limit")]
        [JsonProperty("limit")]
        [XmlElement(ElementName = "limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Concurrency limitation type
        /// </summary>
        [DataMember(Name = "concurrencyLimitationType")]
        [JsonProperty("concurrencyLimitationType")]
        [XmlElement(ElementName = "concurrencyLimitationType")]
        public KalturaConcurrencyLimitationType ConcurrencyLimitationType { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.CONCURRENCY;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            base.Validate();

            if (Limit < 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "KalturaConcurrencyCondition.limit", "0");
            }
        }
    }

    public enum KalturaConcurrencyLimitationType
    {
        Single = 0,
        Group = 1
    }

    /// <summary>
    /// IP range condition
    /// </summary>
    [Serializable]
    public partial class KalturaIpRangeCondition : KalturaCondition
    {
        /// <summary>
        /// From IP address range
        /// </summary>
        [DataMember(Name = "fromIP")]
        [JsonProperty("fromIP")]
        [XmlElement(ElementName = "fromIP")]
        public string FromIP { get; set; }

        /// <summary>
        /// TO IP address range
        /// </summary>
        [DataMember(Name = "toIP")]
        [JsonProperty("toIP")]
        [XmlElement(ElementName = "toIP")]
        public string ToIP { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.IP_RANGE;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            string ipRegex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

            if (string.IsNullOrEmpty(FromIP) || !Regex.IsMatch(FromIP, ipRegex))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fromIP");
            }

            if (string.IsNullOrEmpty(ToIP) || !Regex.IsMatch(ToIP, ipRegex))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "toIP");
            }

            // check for range IP
            string[] fromIPSplited = FromIP.Split('.');
            Int64 ipFrom = Int64.Parse(fromIPSplited[3]) + Int64.Parse(fromIPSplited[2]) * 256 + Int64.Parse(fromIPSplited[1]) * 256 * 256 + Int64.Parse(fromIPSplited[0]) * 256 * 256 * 256;

            string[] toIPSplited = ToIP.Split('.');
            Int64 ipTo = Int64.Parse(toIPSplited[3]) + Int64.Parse(toIPSplited[2]) * 256 + Int64.Parse(toIPSplited[1]) * 256 * 256 + Int64.Parse(toIPSplited[0]) * 256 * 256 * 256;

            if (ipTo < ipFrom)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "fromIP", "toIP");
            }
        }
    }

    /// <summary>
    /// Business module condition
    /// </summary>
    public partial class KalturaBusinessModuleCondition : KalturaCondition
    {
        /// <summary>
        /// Business module type  
        /// </summary>
        [DataMember(Name = "businessModuleType")]
        [JsonProperty("businessModuleType")]
        [XmlElement(ElementName = "businessModuleType", IsNullable = true)]
        public KalturaTransactionType? BusinessModuleType { get; set; }

        /// <summary>
        /// Business module ID  
        /// </summary>
        [DataMember(Name = "businessModuleId")]
        [JsonProperty("businessModuleId")]
        [XmlElement(ElementName = "businessModuleId", IsNullable = true)]
        public long? BusinessModuleId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.BUSINESS_MODULE;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if ((!this.BusinessModuleId.HasValue || this.BusinessModuleId == 0) && !this.BusinessModuleType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition.businessModuleType");
            }
        }
    }

    /// <summary>
    /// Segments condition
    /// </summary>
    public partial class KalturaSegmentsCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated segments IDs list 
        /// </summary>
        [DataMember(Name = "segmentsIds")]
        [JsonProperty("segmentsIds")]
        [XmlElement(ElementName = "segmentsIds")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string SegmentsIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.SEGMENTS;
        }

        public List<int> getSegmentsIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(SegmentsIds, "segmentsIds", true);
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.SegmentsIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition.segmentsIds");
            }
        }
    }

    /// <summary>
    /// Date condition
    /// </summary>
    public partial class KalturaDateCondition : KalturaNotCondition
    {
        /// <summary>
        /// Start date 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }

        /// <summary>
        /// End date 
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long EndDate { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DATE;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (this.StartDate == 0 && this.EndDate == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "condition.startDate", "condition.endDate");
            }
        }
    }

    /// <summary>
    /// Header condition
    /// </summary>
    public partial class KalturaHeaderCondition : KalturaNotCondition
    {
        /// <summary>
        /// Header key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Header value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.HEADER;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaHeaderCondition.key");
            }

            if (string.IsNullOrEmpty(Value))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaHeaderCondition.value");
            }
        }
    }

    public abstract partial class KalturaSubscriptionCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated subscription IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSubscriptionCondition.idIn");
            }
        }
    }

    /// <summary>
    /// UserSubscription Condition - indicates which users this rule is applied on by their subscriptions
    /// </summary>
    public partial class KalturaUserSubscriptionCondition : KalturaSubscriptionCondition
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_SUBSCRIPTION;
        }
    }

    /// <summary>
    /// AssetSubscription Condition - indicates which assets this rule is applied on by their subscriptions
    /// </summary>
    public partial class KalturaAssetSubscriptionCondition : KalturaSubscriptionCondition
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.ASSET_SUBSCRIPTION;
        }
    }

    /// <summary>
    /// UserRole Condition - indicates which users this rule is applied on by their roles
    /// </summary>
    public partial class KalturaUserRoleCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated user role IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_ROLE;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserRoleCondition.idIn");
            }
        }
    }

    public partial class KalturaDeviceBrandCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated Device Brand IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_BRAND;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceBrandCondition.idIn");
            }

            var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceBrandCondition.idIn", 10);
            }
        }

        internal List<int>  GetDeviceBrandIds()
        {
            return !string.IsNullOrEmpty(this.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(this.IdIn, "KalturaDeviceBrandCondition.IdIn", true) : null;
        }
    }

    public partial class KalturaDeviceFamilyCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated Device Family IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_FAMILY;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceFamilyCondition.idIn");
            }

            var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceFamilyCondition.idIn", 10);
            }
        }

        internal List<int> GetDeviceFamilyIds()
        {
            return !string.IsNullOrEmpty(this.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(this.IdIn, "KalturaDeviceFamilyCondition.IdIn", true) : null;
        }
    }

    public partial class KalturaDeviceManufacturerCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated Device Manufacturer IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_MANUFACTURER;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceManufacturerCondition.idIn");
            }

            var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDeviceManufacturerCondition.idIn", 10);
            }
        }

        internal List<int> GetDeviceManufacturerIds()
        {
            return !string.IsNullOrEmpty(this.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(this.IdIn, "KalturaDeviceManufacturerCondition.IdIn", true) : null;
        }
    }

    public partial class KalturaDeviceModelCondition : KalturaCondition
    {
        /// <summary>
        /// regex of device model that is compared to
        /// </summary>
        [DataMember(Name = "regexEqual")]
        [JsonProperty("regexEqual")]
        [XmlElement(ElementName = "regexEqual")]
        public string RegexEqual { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_MODEL;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrEmpty(this.RegexEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceModelCondition.regexEqual");
            }

            if (!StringUtils.IsValidRegex(this.RegexEqual))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaDeviceModelCondition.regexEqual");
            }
        }
    }

    public partial class KalturaUdidDynamicListCondition : KalturaCondition
    {
        /// <summary>
        /// KalturaUdidDynamicList.id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(DynamicMinInt = 1)]
        public long Id { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_UDID_DYNAMIC_LIST;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (Id < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUdidDynamicListCondition.id");
            }
        }
    }

    public partial class KalturaDynamicKeysCondition : KalturaCondition
    {
        private const int VALUES_ARRAY_LIMIT = 16;

        /// <summary>
        /// key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        [SchemeProperty(MinLength = 1, Pattern = SchemeInputAttribute.ASCII_ONLY_PATTERN)]
        public string Key { get; set; }

        /// <summary>
        /// comma-separated values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlElement(ElementName = "values")]
        [SchemeProperty(MinLength = 1)]
        public string Values { get; set; }

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.DYNAMIC_KEYS;
        }

        internal List<string> GetValues()
        {
            return !string.IsNullOrWhiteSpace(Values)
                ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(Values, "KalturaDynamicKeysCondition.values", true)
                : null;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    "KalturaDynamicKeysCondition.key");
            }

            var values = GetValues();

            if (values == null || values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    $"KalturaDynamicKeysCondition.values");
            }

            if (values.Count > VALUES_ARRAY_LIMIT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED,
                    $"KalturaDynamicKeysCondition.values", VALUES_ARRAY_LIMIT);
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
    }
    
    public partial class KalturaDeviceDynamicDataCondition : KalturaCondition
    {
        /// <summary>
        /// key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        [SchemeProperty(Pattern = SchemeInputAttribute.NOT_EMPTY_PATTERN)]
        public string Key { get; set; }

        /// <summary>
        /// value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty(Pattern = SchemeInputAttribute.NOT_EMPTY_PATTERN)]
        public string Value { get; set; }

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.DEVICE_DYNAMIC_DATA;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
        }
    }

    /// <summary>
    /// UserSessionProfile Condition
    /// </summary>
    public partial class KalturaUserSessionProfileCondition : KalturaCondition
    {
        /// <summary>
        /// UserSessionProfile id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinLong = 1)]
        public long Id { get; set; }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_SESSION_PROFILE;
        }
    }
}