using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaRegionFilter : KalturaFilter<KalturaRegionOrderBy>
    {
        /// <summary>
        /// List of comma separated regions external identifiers
        /// </summary>
        [DataMember(Name = "externalIdIn")]
        [JsonProperty("externalIdIn")]
        [XmlElement(ElementName = "externalIdIn")]
        public string ExternalIdIn { get; set; }

        public List<string> GetExternalIdIn()
        {
            List<string> list = null;
            if (!string.IsNullOrEmpty(ExternalIdIn))
            {
                string[] stringValues = ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (stringValues != null && stringValues.Length > 0)
                {
                    list = stringValues.ToList();
                }
            }

            return list;
        }

        public override KalturaRegionOrderBy GetDefaultOrderByValue()
        {
            return KalturaRegionOrderBy.CREATE_DATE_ASC;
        }
    }
}