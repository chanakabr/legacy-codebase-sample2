using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    [Serializable]
    public partial class KalturaPreviewModuleFilter : KalturaFilter<KalturaPreviewModuleOrderBy>
    {
        /// <summary>
        /// Comma separated discount codes
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1, IsNullable = true)]
        public string IdIn { get; set; }

        public override KalturaPreviewModuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaPreviewModuleOrderBy.NONE;
        }

        internal List<long> GetIdIn()
        {
            return this.GetItemsIn<List<long>, long>(IdIn, "KalturaPreviewModuleFilter.IdIn", true);
        }
    }

    public enum KalturaPreviewModuleOrderBy
    {
        NONE
    }
}
