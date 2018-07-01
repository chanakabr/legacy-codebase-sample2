using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaRuleConditionType
    {
        ASSET,
        COUNTRY,
        CONCURRENCY
    }
    
    /// <summary>
    /// Condition
    /// </summary>
    public abstract class KalturaCondition : KalturaOTTObject
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
    /// Country condition
    /// </summary>
    public class KalturaCountryCondition : KalturaCondition
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

        /// <summary>
        /// Comma separated countries IDs list
        /// </summary>
        [DataMember(Name = "countries")]
        [JsonProperty("countries")]
        [XmlElement(ElementName = "countries")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string Countries { get; set; }

        public KalturaCountryCondition()
        {
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
    public class KalturaAssetCondition : KalturaCondition
    {
        /// <summary>
        /// KSQL  
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty("ksql")]
        [XmlElement(ElementName = "ksql")]
        public string Ksql { get; set; }

        public KalturaAssetCondition()
        {
            this.Type = KalturaRuleConditionType.ASSET;
        }
    }

    /// <summary>
    /// Asset Condition
    /// </summary>
    public class KalturaConcurrencyCondition : KalturaAssetCondition
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

        public KalturaConcurrencyCondition()
        {
            this.Type = KalturaRuleConditionType.CONCURRENCY;
        }
    }

    public enum KalturaConcurrencyLimitationType
    {
        Single = 0,
        Group = 1
    }
}