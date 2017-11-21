using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class RecordSeriesByProgramIdNPVRCommand : BaseNPVRCommand
    {
        public bool NewVersion {get;set;}

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.RecordSeriesByProgramID(siteGuid, assetID);
        }
    }
}
