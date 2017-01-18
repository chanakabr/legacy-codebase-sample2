using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class ProtectNPVRCommand : BaseNPVRCommand
    {
        public bool isProtect;

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.SetNPVRProtectionStatus(siteGuid, assetID, false, isProtect);
        }
    }
}
