using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
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
        DEVICE_UDID
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

        internal abstract void Validate();
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

    public partial class KalturaOrCondition : KalturaNotCondition
    {
        /// <summary>
        /// List of conditions with or between them  
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaCondition> Conditions { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.OR;
        }

        internal override void Validate()
        {
            if (this.Conditions == null || this.Conditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition.conditions");
            }

            foreach (var condition in this.Conditions)
            {
                condition.Validate();
            }
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
            return this.GetItemsIn<List<int>, int>(Countries, "KalturaCountryCondition.countries");
        }

        internal override void Validate()
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
    public partial class KalturaAssetCondition : KalturaCondition
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

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
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
            List<int> segments = new List<int>();

            if (!string.IsNullOrEmpty(SegmentsIds))
            {
                string[] splitted = SegmentsIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int segmentId = 0;

                foreach (var segment in splitted)
                {
                    if (int.TryParse(segment, out segmentId) && segmentId > 0)
                    {
                        segments.Add(segmentId);
                    }
                }
            }

            return segments;
        }

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
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

        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceBrandCondition.idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaDeviceBrandCondition.idIn", 10);
            }
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

        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceFamilyCondition.idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaDeviceFamilyCondition.idIn", 10);
            }
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

        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceManufacturerCondition.idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaDeviceManufacturerCondition.idIn", 10);
            }
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

        internal override void Validate()
        {
            if (string.IsNullOrEmpty(this.RegexEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceModelCondition.idIn");
            }
        }
    }

    public partial class KalturaDeviceUdidCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated Device Udid IDs list
        /// </summary>
        [DataMember(Name = "udidIn")]
        [JsonProperty("udidIn")]
        [XmlElement(ElementName = "udidIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string UdidIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_UDID;
        }

        internal override void Validate()
        {
            //TODO - Matan or Shir, fix udid list ctrl
            throw new NotImplementedException();

            if (string.IsNullOrEmpty(this.UdidIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaDeviceUdidCondition.udidIn");
            }

            var items = GetItemsIn<List<long>, long>(this.UdidIn, "udidIn", true);
            if (items.Count > 10)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaDeviceUdidCondition.idIn", 10);
            }
        }
    }
}