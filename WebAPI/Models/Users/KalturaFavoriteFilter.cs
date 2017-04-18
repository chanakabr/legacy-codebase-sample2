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

namespace WebAPI.Models.Users
{
    public enum KalturaFavoriteOrderBy
    {
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }

    /// <summary>
    /// Favorite request filter 
    /// </summary>
    public class KalturaFavoriteFilter : KalturaFilter<KalturaFavoriteOrderBy>
    {
        /// <summary>
        /// Media type to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "mediaTypeIn")]
        [JsonProperty(PropertyName = "mediaTypeIn")]
        [XmlElement(ElementName = "mediaTypeIn")]
        [OldStandardProperty("media_type")]
        [Obsolete]
        public int? MediaTypeIn
        {
            get
            {
                return MediaTypeEqual;
            }
            set
            {
                MediaTypeEqual = value;
            }
        }

        /// <summary>
        /// Media type to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "mediaTypeEqual")]
        [JsonProperty(PropertyName = "mediaTypeEqual")]
        [XmlElement(ElementName = "mediaTypeEqual")]
        public int? MediaTypeEqual { get; set; }

        /// <summary>
        /// Device UDID to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        [Obsolete]
        public string UDID { get; set; }

        /// <summary>
        /// Media identifiers from which to filter the favorite assets
        /// </summary>
        [DataMember(Name = "media_ids")]
        [JsonProperty(PropertyName = "media_ids")]
        [XmlArray(ElementName = "media_ids", IsNullable = true)]
        [XmlArrayItem("item")]
        [Obsolete]
        public List<KalturaIntegerValue> MediaIds { get; set; }

        /// <summary>
        /// Media identifiers from which to filter the favorite assets
        /// </summary>
        [DataMember(Name = "mediaIdIn")]
        [JsonProperty(PropertyName = "mediaIdIn")]
        [XmlArray(ElementName = "mediaIdIn", IsNullable = true)]
        public string MediaIdIn { get; set; }

        public override KalturaFavoriteOrderBy GetDefaultOrderByValue()
        {
            return KalturaFavoriteOrderBy.CREATE_DATE_DESC;
        }

        public List<int> getMediaIdIn() 
        {
            List<int> list = null;
            if (!string.IsNullOrEmpty(MediaIdIn))
            {
                list = new List<int>();
                string[] stringValues = MediaIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    int value;
                    if (int.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaFavoriteFilter.idIn");
                    }
                }
            }
            else if (MediaIds != null)
            {
                list = MediaIds.Select(id => id.value).ToList();
            }

            return list;
        }
    }
}