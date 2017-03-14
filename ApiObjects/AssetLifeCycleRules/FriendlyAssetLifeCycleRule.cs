using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.AssetLifeCycleRules
{
    public class FriendlyAssetLifeCycleRule : AssetLifeCycleRule
    {
        public string FilterTagTypeName { get; set; }
        public List<string> FilterTagValues { get; set; } // from Tvinci.dbo.tags
        public eCutType FilterTagOperand { get; set; }
        public string MetaDateName { get; set; }
        public long MetaDateValueInSeconds { get; set; }

        public FriendlyAssetLifeCycleRule() : base()
        {
        }

        public FriendlyAssetLifeCycleRule(AssetLifeCycleRule alcr) : base()
        {
            this.Actions = alcr.Actions;
            this.Description = alcr.Description;
            this.GroupId = alcr.GroupId;
            this.Id = alcr.Id;
            this.KsqlFilter = alcr.KsqlFilter;
            this.Name = alcr.Name;
            this.TransitionIntervalUnitsId = alcr.TransitionIntervalUnitsId;
        }

    }
}
