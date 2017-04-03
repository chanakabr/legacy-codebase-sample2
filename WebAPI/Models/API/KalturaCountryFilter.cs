using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Country filter
    /// </summary>
    public class KalturaCountryFilter : KalturaFilter<KalturaCountryOrderBy>
    {

        /// <summary>
        /// Country identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

        /// <summary>
        /// Ip to identify the country
        /// </summary>
        [DataMember(Name = "ipEqual")]
        [JsonProperty("ipEqual")]
        [XmlElement(ElementName = "ipEqual", IsNullable = true)]
        public string IpEqual { get; set; }

        /// <summary>
        /// Indicates if to get the IP from the request
        /// </summary>
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [DataMember(Name = "ipEqualCurrent")]
        [JsonProperty("ipEqualCurrent")]
        [XmlElement(ElementName = "ipEqualCurrent", IsNullable = true)]
        public bool? IpEqualCurrent { get; set; }

        public override KalturaCountryOrderBy GetDefaultOrderByValue()
        {
            return KalturaCountryOrderBy.NAME_ASC;
        }

        public List<int> GetIdIn()
        {
            List<int> list = new List<int>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    int value;
                    if (int.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaCountryFilter.idIn");
                    }
                }
            }

            return list;
        }

    }
}