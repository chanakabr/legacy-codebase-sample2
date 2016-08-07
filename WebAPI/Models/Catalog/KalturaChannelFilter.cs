using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

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

        /// <summary>
        ///Query
        /// </summary>
        [DataMember(Name = "queryEqual")]
        [JsonProperty("queryEqual")]
        [XmlElement(ElementName = "queryEqual", IsNullable = true)]
        public string QueryEqual { get; set; }


        public KalturaChannelFilter(KalturaChannelFilter k)
        {
            this.IdEqual = k.IdEqual;
            this.QueryEqual = k.QueryEqual;
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