using ApiObjects.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaSearchAssetFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// (Deprecated - use KalturaBaseSearchAssetFilter.kSql)
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries; 1 - Recordings; Any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// Comma separated list of EPG channel ids to search within. *****Deprecated, please use linear_media_id inside kSql instead*****
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [Deprecated("5.0.1.0")]
        public string IdIn { get; set; }       

        internal List<int> getTypeIn()
        {
            if (string.IsNullOrEmpty(TypeIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchAssetFilter.typeIn");
                }
            }

            return values;
        }

        internal List<int> getEpgChannelIdIn()
        {
            if (string.IsNullOrEmpty(IdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchAssetFilter.idIn");
                }
            }

            return values;
        }

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var userId = contextData.UserId.ToString();
            int domainId = (int)(contextData.DomainId ?? 0);
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);
            
            var response = ClientsManager.CatalogClient().SearchAssets(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(), pager.PageSize, this.Ksql,
                this.OrderBy, this.getTypeIn(), this.getEpgChannelIdIn(), contextData.ManagementData, this.DynamicOrderBy,
                this.getGroupByValue(), responseProfile, isAllowedToViewInactiveAssets, this.GroupByOrder);

            return response;
        }
    }
}