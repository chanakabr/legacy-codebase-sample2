using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaManualChannel : KalturaChannel
    {
        /// <summary>
        /// A list of comma separated media ids associated with this channel, according to the order of the medias in the channel.
        /// </summary>
        [DataMember(Name = "mediaIds")]
        [JsonProperty("mediaIds")]
        [XmlElement(ElementName = "mediaIds", IsNullable = true)]
        public string MediaIds { get; set; }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        public bool ValidateMediaIds()
        {
            if (!string.IsNullOrEmpty(MediaIds))
            {
                string[] stringValues = MediaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaManualChannel.mediaIds");
                    }
                }
            }

            return true;
        }

    }
}