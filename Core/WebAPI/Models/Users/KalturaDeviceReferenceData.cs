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
using System.Text.RegularExpressions;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Device Information
    /// </summary>
    [Serializable]
    public partial class KalturaDeviceReferenceData : KalturaCrudObject<DeviceReferenceData, long>
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
        [SchemeProperty(MinLength = 1, MaxLength = 50)]
        public string Name { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public bool? Status { get; set; }

        internal override ICrudHandler<DeviceReferenceData, long> Handler
        {
            get
            {
                return DeviceReferenceDataManager.Instance;
            }
        }

        public KalturaDeviceReferenceData()
        {

        }

        internal override void SetId(long id)
        {
            Id = id;
        }

        internal override void ValidateForAdd()
        {
            ValidateName();
        }

        internal override void ValidateForUpdate()
        {
            ValidateName();
        }

        internal override GenericResponse<DeviceReferenceData> Add(ContextData contextData)
        {
            throw new NotImplementedException();
        }

        internal override GenericResponse<DeviceReferenceData> Update(ContextData contextData)
        {
            throw new NotImplementedException();
        }

        internal void ValidateName()
        {
            //Numeric, words, underscore and spaces
            if (!string.IsNullOrEmpty(Name) && !Regex.IsMatch(Name, @"^[a-zA-Z0-9\_ ]+$", RegexOptions.IgnoreCase))
            {
                throw new ClientException((int)StatusCode.Error, "Field [Name] didn't passed validation");
            }
        }
    }

    /// <summary>
    /// Device Manufacturer Information
    /// </summary>
    [Serializable]
    public partial class KalturaDeviceManufacturerInformation : KalturaDeviceReferenceData
    {
        internal override GenericResponse<DeviceReferenceData> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceManufacturerInformation>(this);
            return DeviceReferenceDataManager.Instance.Add(contextData, coreObject);
        }
        internal override GenericResponse<DeviceReferenceData> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<DeviceManufacturerInformation>(this);
            return DeviceReferenceDataManager.Instance.Update(contextData, coreObject);
        }
    }

    public partial class KalturaDeviceReferenceDataListResponse : KalturaListResponse<KalturaDeviceReferenceData>
    {
        public KalturaDeviceReferenceDataListResponse() : base() { }
    }
}
