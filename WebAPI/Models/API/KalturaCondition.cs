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
    /// <summary>
    /// Condition
    /// </summary>
    public class KalturaCondition : KalturaOTTObject
    {
        /// <summary>
        /// Indicates whether to apply not on the other properties in the condition
        /// </summary>
        [DataMember(Name = "not")]
        [JsonProperty("not")]
        [XmlElement(ElementName = "not")]
        public bool Not { get; set; }
    }

    /// <summary>
    /// Asset Condition
    /// </summary>
    public class KalturaAssetCondition : KalturaOTTObject
    {
        /// <summary>
        /// KSQL  
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty("ksql")]
        [XmlElement(ElementName = "ksql")]
        public string Ksql { get; set; }
    }

    /// <summary>
    /// Country condition
    /// </summary>
    public class KalturaCountryCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated countries IDs list
        /// </summary>
        [DataMember(Name = "countries")]
        [JsonProperty("countries")]
        [XmlElement(ElementName = "countries")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string Countries { get; set; }

        public List<long> getCountries()
        {
            List<long> countries = new List<long>();

            if (!string.IsNullOrEmpty(Countries))
            {
                string[] splitted = Countries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var country in splitted)
                {
                    countries.Add(long.Parse(country));
                }
            }

            return countries;
        }
    }
}