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
    public partial class KalturaMediaFileFilter : KalturaFilter<KalturaMediaFileOrderBy>
    {
        /// <summary>
        /// Asset identifier to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long AssetIdEqual { get; set; }

        /// <summary>
        /// Asset file identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        public long IdEqual { get; set; }

        internal virtual void Validate()
        {
            if (AssetIdEqual > 0 && IdEqual > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMediaFileFilter.idEqual", "KalturaMediaFileFilter.assetIdEqual");
            }
        }

        public override KalturaMediaFileOrderBy GetDefaultOrderByValue()
        {
            return KalturaMediaFileOrderBy.NONE;
        }
    }

    public enum KalturaMediaFileOrderBy
    {
        NONE
    }
}