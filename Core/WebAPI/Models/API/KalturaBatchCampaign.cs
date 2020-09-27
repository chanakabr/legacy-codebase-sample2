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

        internal override void ValidateForAdd()
        {
            base.ValidateForAdd();

            if (this.PopulationConditions == null || this.PopulationConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "populationConditions");
            }

            ValidateConditions();
        }

        internal override GenericResponse<Campaign> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<BatchCampaign>(this);
            return CampaignManager.Instance.AddBatchCampaign(contextData, coreObject);
        }

        internal override void ValidateForUpdate()
        {
            base.ValidateForUpdate();

            if (this.PopulationConditions != null)
            {
                ValidateConditions();
            }
        }

        internal override GenericResponse<Campaign> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<BatchCampaign>(this);
            return CampaignManager.Instance.UpdateBatchCampaign(contextData, coreObject);
        }

        private void ValidateConditions()
        {
            if (PopulationConditions.Count > 50)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "populationConditions", 50);
            }

            foreach (var condition in this.PopulationConditions)
            {
                if (condition.Type != KalturaRuleConditionType.OR && condition.Type != KalturaRuleConditionType.SEGMENTS)
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "populationConditions", condition.objectType);
                }

                condition.Validate();
            }
        }
    }
}