using ApiLogic.Users.Managers;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaBatchCampaign : KalturaCampaign
    {
        internal override void ValidateForAdd()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            base.ValidateForAdd();
        }

        internal override GenericResponse<Campaign> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<BatchCampaign>(this);
            return CampaignManager.Instance.AddBatchCampaign(contextData, coreObject);
        }

        internal override void ValidateForUpdate()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            base.ValidateForUpdate();
        }

        internal override GenericResponse<Campaign> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<BatchCampaign>(this);
            return CampaignManager.Instance.UpdateBatchCampaign(contextData, coreObject);
        }
    }
}