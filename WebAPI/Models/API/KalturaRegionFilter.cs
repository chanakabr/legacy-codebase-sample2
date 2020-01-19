using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public abstract partial class KalturaBaseRegionFilter : KalturaFilter<KalturaRegionOrderBy>
    {
        internal abstract KalturaRegionListResponse GetRegions(int groupId);
        internal abstract void Validate();

        public override KalturaRegionOrderBy GetDefaultOrderByValue()
        {
            return KalturaRegionOrderBy.CREATE_DATE_ASC;
        }
    }

    public partial class KalturaRegionFilter : KalturaBaseRegionFilter
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


        internal override void Validate()
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

        internal override KalturaRegionListResponse GetRegions(int groupId)
        {
            return ClientsManager.ApiClient().GetRegions(groupId, this);
        }
    }

    public partial class KalturaDefaultRegionFilter : KalturaBaseRegionFilter
    {
        internal override KalturaRegionListResponse GetRegions(int groupId)
        {
            var response = ClientsManager.ApiClient().GetDefaultRegion(groupId);
            if (response?.Regions?.Count > 0)
            {
                response.Regions = response.Regions.Where(x => x.IsDefault).ToList();
                response.TotalCount = response.Regions.Count;
            }

            return response;
        }

        internal override void Validate()
        {
        }
    }
}