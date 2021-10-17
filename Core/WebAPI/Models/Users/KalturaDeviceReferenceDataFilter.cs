using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Device Reference Data Filter
    /// </summary>
    public partial class KalturaDeviceReferenceDataFilter : KalturaCrudFilter<KalturaDeviceReferenceDataOrderBy, DeviceReferenceData>
    {
        /// <summary>
        /// IdIn
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(MinLength = 1)]
        public string IdIn { get; set; }

        public KalturaDeviceReferenceDataFilter() : base()
        {
        }

        public override KalturaDeviceReferenceDataOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceReferenceDataOrderBy.NONE;
        }

        public override void Validate(ContextData contextData)
        {
        }

        public override GenericListResponse<DeviceReferenceData> List(ContextData contextData, CorePager pager)
        {
            throw new WebAPI.Exceptions.ClientException((int)eResponseStatus.NotAllowed, "Filter error");
        }
    }

    public partial class KalturaDeviceManufacturersReferenceDataFilter : KalturaDeviceReferenceDataFilter
    {
        /// <summary>
        /// name equal
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual")]
        public string NameEqual { get; set; }

        public override GenericListResponse<DeviceReferenceData> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<DeviceManufacturersReferenceDataFilter>(this);
            var deviceReferenceDatas = DeviceReferenceDataManager.Instance.ListByManufacturer(contextData, coreFilter, pager);
            return deviceReferenceDatas;
        }
    }

    public enum KalturaDeviceReferenceDataOrderBy
    {
        NONE
    }
}