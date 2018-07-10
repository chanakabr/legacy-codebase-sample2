using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    public class KalturaBulkFilter : KalturaPersistedFilter<KalturaBulkOrderBy>
    {

        public override KalturaBulkOrderBy GetDefaultOrderByValue()
        {
            return KalturaBulkOrderBy.NONE;
        }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "statusEqual")]
        [JsonProperty("statusEqual")]
        [XmlElement(ElementName = "statusEqual", IsNullable = true)]        
        public KalturaBatchJobStatus? StatusEqual { get; set; }

    }

    public enum KalturaBulkOrderBy
    {
        NONE
    }

}



