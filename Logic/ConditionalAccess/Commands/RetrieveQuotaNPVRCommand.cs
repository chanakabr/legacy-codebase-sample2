using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class RetrieveQuotaNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.GetNPVRQuota(siteGuid);
        }
    }
}
