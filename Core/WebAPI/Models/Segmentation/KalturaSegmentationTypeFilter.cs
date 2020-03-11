using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public abstract partial class KalturaBaseSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrder>
    {
        internal abstract KalturaSegmentationTypeListResponse GetSegmentationTypes(int groupId, long userId, KalturaFilterPager pager);

        internal abstract bool Validate();

        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
        }
    }

    /// <summary>
    /// Filter for segmentation types
    /// </summary>
    public partial class KalturaSegmentationTypeFilter : KalturaBaseSegmentationTypeFilter
    {
        /// <summary>
        /// Comma separated segmentation types identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public HashSet<long> GetIdIn()
        {
            HashSet<long> hashSet = new HashSet<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !hashSet.Contains(value))
                    {
                        hashSet.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSegmentationTypeFilter.idIn");
                    }
                }
            }

            return hashSet;
        }

        internal override bool Validate()
        {
            if (string.IsNullOrEmpty(IdIn) && string.IsNullOrEmpty(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            if (!string.IsNullOrEmpty(IdIn) && !string.IsNullOrEmpty(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            return true;
        }

        internal override KalturaSegmentationTypeListResponse GetSegmentationTypes(int groupId, long userId, KalturaFilterPager pager)
        {
            HashSet<long> ids = null;
            bool isAllowedToViewInactiveAssets = false;

            if (string.IsNullOrEmpty(Ksql))
            {
                ids = GetIdIn();
            }
            else
            {
                isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            }

            return ClientsManager.ApiClient().ListSegmentationTypes(groupId, ids, pager.getPageIndex(), pager.getPageSize(),
                new AssetSearchDefinition() { Filter = Ksql, UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets });
        }
    }

    public partial class KalturaSegmentValueFilter : KalturaBaseSegmentationTypeFilter
    {
        /// <summary>
        /// Comma separated segmentation identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        public List<long> GetIdIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "idIn");
                    }
                }
            }

            return list;
        }

        internal override KalturaSegmentationTypeListResponse GetSegmentationTypes(int groupId, long userId, KalturaFilterPager pager)
        {
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);

            return ClientsManager.ApiClient().GetSegmentationTypesBySegmentIds(groupId, GetIdIn(),  
                new AssetSearchDefinition() { UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets },
                pager.getPageIndex(), pager.getPageSize());
        }

        internal override bool Validate()
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            return true;
        }
    }

    /// <summary>
    /// Segmentation types order
    /// </summary>
    public enum KalturaSegmentationTypeOrder
    {
        NONE
    }
}
