using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaRegionFilter : KalturaFilter<KalturaRegionOrderBy>
    {
        /// <summary>
        /// List of comma separated regions external IDs
        /// </summary>
        [DataMember(Name = "externalIdIn")]
        [JsonProperty("externalIdIn")]
        [XmlElement(ElementName = "externalIdIn")]
        public string ExternalIdIn { get; set; }

        /// <summary>
        /// List of comma separated regions Ids
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        /// <summary>
        /// Region parent ID to filter by
        /// </summary>
        [DataMember(Name = "parentIdEqual")]
        [JsonProperty("parentIdEqual")]
        [XmlElement(ElementName = "parentIdEqual")]
        public int ParentIdEqual { get; set; }

        /// <summary>
        /// Region parent ID to filter by
        /// </summary>
        [DataMember(Name = "liveAssetIdEqual")]
        [JsonProperty("liveAssetIdEqual")]
        [XmlElement(ElementName = "liveAssetIdEqual")]
        public int LiveAssetIdEqual { get; set; }


        public void Validate()
        {
            if ((!string.IsNullOrEmpty(ExternalIdIn) && (!string.IsNullOrEmpty(IdIn) || ParentIdEqual > 0)) ||
                (!string.IsNullOrEmpty(IdIn) && (!string.IsNullOrEmpty(ExternalIdIn) || ParentIdEqual > 0)) ||
                (ParentIdEqual > 0 && (!string.IsNullOrEmpty(IdIn) || !string.IsNullOrEmpty(ExternalIdIn))))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaRegionFilter.externalIdIn, KalturaRegionFilter.idIn", "KalturaRegionFilter.parentIdEqual");
            }
        }

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