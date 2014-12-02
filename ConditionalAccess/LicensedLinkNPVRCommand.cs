using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class LicensedLinkNPVRCommand : BaseNPVRCommand
    {

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            LicensedLinkNPVRResponse res = new LicensedLinkNPVRResponse();
            throw new NotImplementedException("Add here the correct params.");

            return res;
        }
    }
}
