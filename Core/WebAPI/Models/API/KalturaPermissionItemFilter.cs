using APILogic.Api.Managers;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Models.API
{
    [Serializable]
    public partial class KalturaPermissionItemFilter : KalturaFilter<KalturaPermissionItemOrderBy>
    {
        public override KalturaPermissionItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaPermissionItemOrderBy.NONE;
        }

        public virtual void Validate()
        {
        }

        internal virtual KalturaPermissionItemListResponse GetPermissionItems(KalturaFilterPager pager)
        {
            var result = new KalturaPermissionItemListResponse();

            Func<GenericListResponse<PermissionItem>> getPermissionItemListFunc = () =>
                RolesPermissionsManager.GetPermissionItemList(AutoMapper.Mapper.Map<PermissionItemFilter>(this), AutoMapper.Mapper.Map<CorePager>(pager));

            KalturaGenericListResponse<KalturaPermissionItem> response =
                ClientUtils.GetResponseListFromWS<KalturaPermissionItem, PermissionItem>(getPermissionItemListFunc);

            result.Objects = new List<KalturaPermissionItem>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }

    public partial class KalturaPermissionItemByIdInFilter : KalturaPermissionItemFilter
    {
        /// <summary>
        /// Permission item identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }
        }

        internal override KalturaPermissionItemListResponse GetPermissionItems(KalturaFilterPager pager)
        {
            var result = new KalturaPermissionItemListResponse();

            Func<GenericListResponse<PermissionItem>> getPermissionItemListFunc = () =>
                RolesPermissionsManager.GetPermissionItemList(AutoMapper.Mapper.Map<PermissionItemByIdInFilter>(this), AutoMapper.Mapper.Map<CorePager>(pager));

            KalturaGenericListResponse<KalturaPermissionItem> response =
                ClientUtils.GetResponseListFromWS<KalturaPermissionItem, PermissionItem>(getPermissionItemListFunc);

            result.Objects = new List<KalturaPermissionItem>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }
    /// <summary>
    /// If filter properties are empty will return all API action type permission items
    /// </summary>
    public partial class KalturaPermissionItemByApiActionFilter : KalturaPermissionItemFilter
    {
        /// <summary>
        /// API service name
        /// </summary>
        [DataMember(Name = "serviceEqual")]
        [JsonProperty("serviceEqual")]
        [XmlElement(ElementName = "serviceEqual")]
        public string ServiceEqual { get; set; }

        /// <summary>
        /// API action name
        /// </summary>
        [DataMember(Name = "actionEqual")]
        [JsonProperty("actionEqual")]
        [XmlElement(ElementName = "actionEqual")]
        public string ActionEqual { get; set; }

        internal override KalturaPermissionItemListResponse GetPermissionItems(KalturaFilterPager pager)
        {
            var result = new KalturaPermissionItemListResponse();

            Func<GenericListResponse<PermissionItem>> getPermissionItemListFunc = () =>
                RolesPermissionsManager.GetPermissionItemList(AutoMapper.Mapper.Map<PermissionItemByApiActionFilter>(this), AutoMapper.Mapper.Map<CorePager>(pager));

            KalturaGenericListResponse<KalturaPermissionItem> response =
                ClientUtils.GetResponseListFromWS<KalturaPermissionItem, PermissionItem>(getPermissionItemListFunc);

            result.Objects = new List<KalturaPermissionItem>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }

    /// <summary>
    /// If filter properties are empty will return all API argument type permission items
    /// </summary>
    public partial class KalturaPermissionItemByArgumentFilter : KalturaPermissionItemByApiActionFilter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        [DataMember(Name = "parameterEqual")]
        [JsonProperty("parameterEqual")]
        [XmlElement(ElementName = "parameterEqual")]
        public string ParameterEqual { get; set; }

        internal override KalturaPermissionItemListResponse GetPermissionItems(KalturaFilterPager pager)
        {
            var result = new KalturaPermissionItemListResponse();

            Func<GenericListResponse<PermissionItem>> getPermissionItemListFunc = () =>
                RolesPermissionsManager.GetPermissionItemList(AutoMapper.Mapper.Map<PermissionItemByArgumentFilter>(this), AutoMapper.Mapper.Map<CorePager>(pager));

            KalturaGenericListResponse<KalturaPermissionItem> response =
                ClientUtils.GetResponseListFromWS<KalturaPermissionItem, PermissionItem>(getPermissionItemListFunc);

            result.Objects = new List<KalturaPermissionItem>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }

    /// <summary>
    /// If filter properties are empty will return all parameter type permission items
    /// </summary>
    public partial class KalturaPermissionItemByParameterFilter : KalturaPermissionItemFilter
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        [DataMember(Name = "parameterEqual")]
        [JsonProperty("parameterEqual")]
        [XmlElement(ElementName = "parameterEqual")]
        public string ParameterEqual { get; set; }

        /// <summary>
        /// Parameter name
        /// </summary>
        [DataMember(Name = "objectEqual")]
        [JsonProperty("objectEqual")]
        [XmlElement(ElementName = "objectEqual")]
        public string ObjectEqual { get; set; }

        internal override KalturaPermissionItemListResponse GetPermissionItems(KalturaFilterPager pager)
        {
            var result = new KalturaPermissionItemListResponse();

            Func<GenericListResponse<PermissionItem>> getPermissionItemListFunc = () =>
                RolesPermissionsManager.GetPermissionItemList(AutoMapper.Mapper.Map<PermissionItemByParameterFilter>(this), AutoMapper.Mapper.Map<CorePager>(pager));

            KalturaGenericListResponse<KalturaPermissionItem> response =
                ClientUtils.GetResponseListFromWS<KalturaPermissionItem, PermissionItem>(getPermissionItemListFunc);

            result.Objects = new List<KalturaPermissionItem>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }

    public enum KalturaPermissionItemOrderBy
    {
        NONE
    }

}