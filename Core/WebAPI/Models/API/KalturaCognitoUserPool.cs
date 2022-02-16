using ApiLogic.Base;
using ApiObjects;
using WebAPI.Models.General;
using ApiObjects.Response;
using ApiObjects.Base;
using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;

namespace WebAPI.Models.API
{
    public partial class KalturaCognitoUserPool : KalturaOTTObject
    {
        /// <summary>
        /// Default
        /// </summary>
        [DataMember(Name = "iotDefault")]
        [JsonProperty(PropertyName = "iotDefault")]
        [XmlElement(ElementName = "iotDefault")]
        public KalturaIotDefault IotDefault { get; set; }
    }
}
