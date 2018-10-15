using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
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
        OR
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

    public partial class KalturaOrCondition : KalturaCondition
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
            List<int> countries = new List<int>();

            if (!string.IsNullOrEmpty(Countries))
            {
                string[] splitted = Countries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int countryId = 0;

                foreach (var country in splitted)
                {
                    if (int.TryParse(country, out countryId) && countryId > 0)
                    {
                        countries.Add(countryId);
                    }
                }
            }

            return countries;
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
        public string FromIP{ get; set; }

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
        [XmlElement(ElementName = "businessModuleType")]
        public KalturaTransactionType BusinessModuleType { get; set; }

        /// <summary>
        /// Business module ID  
        /// </summary>
        [DataMember(Name = "businessModuleType")]
        [JsonProperty("businessModuleType")]
        [XmlElement(ElementName = "businessModuleType")]
        public long BusinessModuleId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.BUSINESS_MODULE;
        }
    }

    /// <summary>
    /// Segments condition
    /// </summary>
    public partial class KalturaSegmentsCondition : KalturaNotCondition
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
    }

    /// <summary>
    /// Segments condition
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
    }
}