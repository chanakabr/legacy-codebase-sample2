using ApiObjects;
using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiObjects.Response;
using WebAPI.Models.General;
using ApiObjects.MediaMarks;

namespace WebAPI.Models.Catalog
{
    public enum KalturaStreamingDeviceOrderBy
    {
        NONE
    }

    /// <summary>
    /// Filtering streaming devices
    /// </summary>
    [Serializable]
    public partial class KalturaStreamingDeviceFilter : KalturaFilter<KalturaStreamingDeviceOrderBy>
    {
        /// <summary>
        /// filter by asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty(PropertyName = "assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual", IsNullable = true)]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        public override KalturaStreamingDeviceOrderBy GetDefaultOrderByValue()
        {
            return KalturaStreamingDeviceOrderBy.NONE;
        }

        internal KalturaStreamingDeviceListResponse GetStreamingDevices(int groupId, long householdId)
        {
            ePlayType playType = ePlayType.ALL;

            if (AssetTypeEqual.HasValue)
            {
                playType = Mapper.Map<ePlayType>(AssetTypeEqual.Value);
            }

            var result = new KalturaStreamingDeviceListResponse();

            Func<GenericListResponse<ApiObjects.MediaMarks.DevicePlayData>> getStreamingDevicesListFunc = () =>
            Core.Users.ConcurrencyManager.GetDevicePlayDataList(groupId, householdId, playType);

            KalturaGenericListResponse<KalturaStreamingDevice> response =
                Clients.ClientUtils.GetResponseListFromWS<KalturaStreamingDevice, DevicePlayData>(getStreamingDevicesListFunc);

            result.Objects = new System.Collections.Generic.List<KalturaStreamingDevice>(response.Objects);
            result.TotalCount = response.TotalCount;
            return result;

        }
    }
}