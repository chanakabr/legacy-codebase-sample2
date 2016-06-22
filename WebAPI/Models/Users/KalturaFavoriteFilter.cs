using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public enum KalturaFavoriteOrderBy
    {
    }

    /// <summary>
    /// Favorite request filter 
    /// </summary>
    [OldStandard("mediaTypeIn", "media_type")]
    public class KalturaFavoriteFilter : KalturaFilter<KalturaFavoriteOrderBy?>
    {
        /// <summary>
        /// Media type to filter by the favorite assets
        /// </summary>
        [DataMember(Name = "mediaTypeIn")]
        [JsonProperty(PropertyName = "mediaTypeIn")]
        [XmlElement(ElementName = "mediaTypeIn")]
        public int? MediaTypeIn { get; set; }

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

        public override KalturaFavoriteOrderBy? GetDefaultOrderByValue()
        {
            return null;
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
                        throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.IdIn contains invalid id {0}", value));
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