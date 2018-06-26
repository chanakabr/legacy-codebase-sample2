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

namespace WebAPI.Models.Pricing
{
    public partial class KalturaPriceDetailsFilter : KalturaFilter<KalturaPriceDetailsOrderBy>
    {
        /// <summary>
        /// Comma separated price identifiers 
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }
                
        internal List<long> GetIdIn()
        {
            List<long> list = null;
            if (!string.IsNullOrEmpty(IdIn))
            {
                list = new List<long>();
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                long longValue;
                foreach (string stringValue in stringValues)
                {
                    if (long.TryParse(stringValue, out longValue))
                    {
                        list.Add(longValue);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPriceFilter.idIn");
                    }
                }
            }

            return list;
        }
        public override KalturaPriceDetailsOrderBy GetDefaultOrderByValue()
        {
            return KalturaPriceDetailsOrderBy.NAME_ASC;
        }
    }


    public enum KalturaPriceDetailsOrderBy
    {
        NAME_ASC   
    }
    
}