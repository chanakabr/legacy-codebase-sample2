using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
            return this.GetItemsIn<List<long>, long>(IdIn, "KalturaPricePlanFilter.IdIn");
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