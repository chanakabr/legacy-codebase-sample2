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

namespace WebAPI.Models.Catalog
{
    public class KalturaPersonalListSearchFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// Comma separated list of partner list types to search within. 
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "partnerListTypeIn")]
        [JsonProperty("partnerListTypeIn")]
        [XmlElement(ElementName = "partnerListTypeIn", IsNullable = true)]
        public string PartnerListTypeIn { get; set; }

        internal HashSet<int> GetPartnerListTypeIn()
        {
            if (string.IsNullOrEmpty(PartnerListTypeIn))
                return null;

            HashSet<int> values = new HashSet<int>();
            string[] stringValues = PartnerListTypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPersonalListSearchFilter.PartnerListTypeIn");
                }
            }

            return values;
        }
    }
}