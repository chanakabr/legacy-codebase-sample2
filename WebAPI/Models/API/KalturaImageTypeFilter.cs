using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaImageTypeFilter : KalturaFilter<KalturaImageTypeOrderBy>
    {
        /// <summary>
        /// IDs to filter by
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// Ration IDs to filter by
        /// </summary>
        [DataMember(Name = "ratioIdIn")]
        [JsonProperty("ratioIdIn")]
        [XmlElement(ElementName = "ratioIdIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string RatioIdIn { get; set; }

        public override KalturaImageTypeOrderBy GetDefaultOrderByValue()
        {
            return KalturaImageTypeOrderBy.NONE;
        }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageTypeFilter.idIn");
                    }
                }
            }

            return list;
        }

        public List<long> GetRatioIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = RatioIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaImageTypeFilter.ratioIdIn");
                    }
                }
            }

            return list;
        }
    }

    public enum KalturaImageTypeOrderBy
    {
        NONE
    }
}