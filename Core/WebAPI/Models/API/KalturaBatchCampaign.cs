using ApiLogic.Users.Managers;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaBatchCampaign : KalturaCampaign
    {
        /// <summary>
        /// These conditions define the population that apply one the campaign
        /// </summary>
        [DataMember(Name = "populationConditions")]
        [JsonProperty("populationConditions")]
        [XmlElement(ElementName = "populationConditions")]
        [SchemeProperty(IsNullable = true, RequiresPermission = (int)RequestType.READ)]
        public List<KalturaCondition> PopulationConditions { get; set; }
    }
}