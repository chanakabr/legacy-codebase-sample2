using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class RetrieveQuotaNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.GetNPVRQuota(siteGuid, Version);
        }
    }
}
