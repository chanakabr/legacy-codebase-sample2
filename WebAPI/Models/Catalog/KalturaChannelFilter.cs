using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaChannelFilter : KalturaAssetFilter
    {
        /// <summary>
        ///Channel Id
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        public int IdEqual { get; set; }

        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }


        public KalturaChannelFilter()
        {            
        }

        internal override void Validate()
        {
            if (IdEqual <= 0)
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be positive");
            }
                    
        }
    }
}