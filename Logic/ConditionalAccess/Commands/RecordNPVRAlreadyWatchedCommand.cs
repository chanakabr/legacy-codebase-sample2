using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess
{    
    public class RecordNPVRAlreadyWatchedCommand : BaseNPVRCommand
    {
        public int alreadyWatched { get; set; }

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.SetAssetAlreadyWatchedStatus(siteGuid, assetID, alreadyWatched);
        }
    }   
}
