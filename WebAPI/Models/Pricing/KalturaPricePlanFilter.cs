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
    public partial class KalturaPricePlanFilter : KalturaFilter<KalturaPricePlanOrderBy>
    {
        /// <summary>
        /// Comma separated price plans identifiers 
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPricePlanFilter.idIn");
                    }
                }
            }

            return list;
        }
        public override KalturaPricePlanOrderBy GetDefaultOrderByValue()
        {
            return KalturaPricePlanOrderBy.CREATE_DATE_DESC;
        }
    }


    public enum KalturaPricePlanOrderBy
    {
        CREATE_DATE_DESC   
    }
    
}