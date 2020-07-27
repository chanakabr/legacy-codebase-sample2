using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects;
using WebAPI.Models.General;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using ApiObjects.Base;
using ApiObjects.Response;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Device Information
    /// </summary>
    [Serializable]
    public partial class KalturaDeviceInformation : KalturaCrudObject<DeviceInformation, long>
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        internal override ICrudHandler<DeviceInformation, long> Handler
        {
            get
            {
                return DeviceInformationManager.Instance;
            }
        }

        public KalturaDeviceInformation()
        {

        }

        internal override void SetId(long id)
        {
            Id = id;
        }

        internal override GenericResponse<DeviceInformation> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceInformation>(this);
            return DeviceInformationManager.Instance.Add<DeviceInformation>(contextData, coreObject);
        }

        internal override GenericResponse<DeviceInformation> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceInformation>(this);
            return DeviceInformationManager.Instance.Update(contextData, coreObject);
        }
    }

    /// <summary>
    /// Device Model Information
    /// </summary>
    [Serializable]
    public partial class KalturaDeviceModelInformation : KalturaDeviceInformation
    {
        internal override GenericResponse<DeviceInformation> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceModelInformation>(this);
            return DeviceInformationManager.Instance.Add<DeviceModelInformation>(contextData, coreObject);
        }
    }

    /// <summary>
    /// Device Manufacturer Information
    /// </summary>
    [Serializable]
    public partial class KalturaDeviceManufacturerInformation : KalturaDeviceInformation
    {
        internal override GenericResponse<DeviceInformation> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceManufacturerInformation>(this);
            return DeviceInformationManager.Instance.Add<DeviceManufacturerInformation>(contextData, coreObject);
        }
    }

    public partial class KalturaDeviceInformationListResponse : KalturaListResponse<KalturaDeviceInformation>
    {
        public KalturaDeviceInformationListResponse() : base() { }
    }
}
