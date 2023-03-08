using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using AutoMapper;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public abstract partial class KalturaChannelsBaseFilter : KalturaFilter<KalturaChannelsOrderBy>
    {
        internal abstract KalturaChannelListResponse GetChannels(ContextData contextData, bool isAllowedToViewInactiveAssets, KalturaFilterPager pager);

        public override KalturaChannelsOrderBy GetDefaultOrderByValue()
        {
            return KalturaChannelsOrderBy.NONE;
        }

        public virtual void Validate() { }

        public KalturaChannelsBaseFilter() : base() { }

        protected void ConvertChannelsByType(List<GroupsCacheManager.Channel> objects, ref KalturaChannelListResponse result)
        {
            result.Channels = new List<KalturaChannel>();
            // convert channels
            foreach (GroupsCacheManager.Channel channel in objects)
            {
                if (channel.m_nChannelTypeID == (int)GroupsCacheManager.ChannelType.KSQL)
                {
                    result.Channels.Add(Mapper.Map<KalturaDynamicChannel>(channel));
                }
                else if (channel.m_nChannelTypeID == (int)GroupsCacheManager.ChannelType.Manual)
                {
                    result.Channels.Add(Mapper.Map<KalturaManualChannel>(channel));
                }
                else
                {
                    result.TotalCount--;
                }
            }
        }
    }

    public partial class KalturaChannelsFilter : KalturaChannelsBaseFilter
    {
        /// <summary>
        /// channel identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        /// media identifier to filter by
        /// </summary>
        [DataMember(Name = "mediaIdEqual")]
        [JsonProperty("mediaIdEqual")]
        [XmlElement(ElementName = "mediaIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, MinLong = 1)]
        public long MediaIdEqual { get; set; }

        /// <summary>
        /// Exact channel name to filter by
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual")]
        public string NameEqual { get; set; }

        /// <summary>
        /// Channel name starts with (auto-complete)
        /// </summary>
        [DataMember(Name = "nameStartsWith")]
        [JsonProperty("nameStartsWith")]
        [XmlElement(ElementName = "nameStartsWith")]
        public string NameStartsWith { get; set; }

        /// <summary>
        /// Comma separated channel ids 
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }

        public override void Validate()
        {
            List<string> message = new List<string>();
            int inputCount = 0;

            if (MediaIdEqual > 0)
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.mediaIdEqual");
            }

            if (IdEqual > 0)
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.idEqual");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(NameEqual))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.nameEqual");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(NameStartsWith))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.nameStartsWith");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(IdIn))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.idIn");
                ValidateCheck(message, inputCount);
            }
        }

        private static void ValidateCheck(List<string> message, int inputCount)
        {
            if (inputCount > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, message[0], message[1]);
            }
        }

        public override KalturaChannelsOrderBy GetDefaultOrderByValue()
        {
            return KalturaChannelsOrderBy.NONE;
        }

        internal List<int> GetIdIn()
        {
            List<int> list = null;

            if (!string.IsNullOrEmpty(IdIn))
            {
                list = new List<int>();
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    if (int.TryParse(stringValue, out int value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaChannelsFilter.idIn");
                    }
                }
            }

            return list;
        }

        internal override KalturaChannelListResponse GetChannels(ContextData contextData, bool isAllowedToViewInactiveAssets, KalturaFilterPager pager)
        {
            KalturaChannelListResponse result = new KalturaChannelListResponse();
            GenericListResponse<GroupsCacheManager.Channel> response = null;
            Func<GenericListResponse<GroupsCacheManager.Channel>> getListFunc = null;

            CatalogMappings.ConvertChannelsOrderBy(OrderBy, out ChannelOrderBy orderBy, out OrderDir orderDirection);

            if (MediaIdEqual > 0)
            {
                getListFunc = () => ChannelManager.Instance.GetChannelsContainingMedia(contextData, MediaIdEqual, pager.GetRealPageIndex(),
                                                            pager.PageSize.Value, orderBy, orderDirection, isAllowedToViewInactiveAssets);
            }
            else if (IdEqual > 0)
            {
                Func<GenericResponse<GroupsCacheManager.Channel>> getFunc = () =>
                  ChannelManager.Instance.GetChannel(contextData, IdEqual, isAllowedToViewInactiveAssets, true);
                var channel = ClientUtils.GetResponseFromWS<KalturaChannel, GroupsCacheManager.Channel>(getFunc);
                return new KalturaChannelListResponse() { TotalCount = channel != null ? 1 : 0, Channels = new List<KalturaChannel>() { channel } };
            }
            else if (!string.IsNullOrEmpty(NameEqual))
            {
                getListFunc = () => ChannelManager.Instance.SearchChannels(contextData, true, NameEqual, null, pager.GetRealPageIndex(),
                                                            pager.PageSize.Value, orderBy, orderDirection, isAllowedToViewInactiveAssets);
            }
            else if (!string.IsNullOrEmpty(IdIn))
            {
                getListFunc = () => ChannelManager.Instance.GetChannelsListResponseByChannelIds(contextData, GetIdIn(), isAllowedToViewInactiveAssets, null, true);
            }
            else
            {
                //search using ChannelLike
                getListFunc = () => ChannelManager.Instance.SearchChannels(contextData, false, NameStartsWith, null, pager.GetRealPageIndex(),
                                                            pager.PageSize.Value, orderBy, orderDirection, isAllowedToViewInactiveAssets);
            }

            response = ClientUtils.GetGenericListResponseFromWS<GroupsCacheManager.Channel>(getListFunc);

            if (response.TotalItems > 0)
            {
                result.TotalCount = response.TotalItems;
            }

            if (response.Objects != null && response.Objects.Count > 0)
            {
                ConvertChannelsByType(response.Objects, ref result);
            }

            return result;
        }
    }

    public partial class KalturaChannelSearchByKsqlFilter : KalturaChannelsBaseFilter
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        /// channel struct 
        /// </summary>
        [DataMember(Name = "channelStructEqual")]
        [JsonProperty("channelStructEqual")]
        [XmlElement(ElementName = "channelStructEqual")]
        public KalturaChannelStruct? ChannelStructEqual { get; set; }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaChannelSearchByKsqlFilter.kSql");
            }
        }

        internal override KalturaChannelListResponse GetChannels(ContextData contextData, bool isAllowedToViewInactiveAssets, KalturaFilterPager pager)
        {
            KalturaChannelListResponse result = new KalturaChannelListResponse();             

            AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition() { Filter = Ksql, UserId = contextData.UserId.Value, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets };

            var channelType = CatalogMappings.ConvertChannelType(ChannelStructEqual);
            CatalogMappings.ConvertChannelsOrderBy(OrderBy, out ChannelOrderBy orderBy, out ApiObjects.SearchObjects.OrderDir orderDirection);

            Func<GenericListResponse<GroupsCacheManager.Channel>> getListFunc = () =>
              ChannelManager.Instance.GetChannels(contextData, assetSearchDefinition, channelType, pager.GetRealPageIndex(), pager.PageSize.Value, orderBy, orderDirection);
            GenericListResponse<GroupsCacheManager.Channel> response = ClientUtils.GetGenericListResponseFromWS<GroupsCacheManager.Channel>(getListFunc);

            if (response.TotalItems > 0)
            {
                result.TotalCount = response.TotalItems;
            }

            if (response.Objects != null && response.Objects.Count > 0)
            {
                ConvertChannelsByType(response.Objects, ref result);
            }

            return result;
        }
    }
}