using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.AssetLifeCycleRules
{
    public class FriendlyAssetLifeCycleRuleResponse
    {

        public ApiObjects.Response.Status Status { get; set; }
        public FriendlyAssetLifeCycleRule Rule { get; set; }

        public FriendlyAssetLifeCycleRuleResponse()
        { 
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Rule = new FriendlyAssetLifeCycleRule();
        }

        public FriendlyAssetLifeCycleRuleResponse(FriendlyAssetLifeCycleRule rule)
        {
            this.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            this.Rule = rule;
        }

    }
}
