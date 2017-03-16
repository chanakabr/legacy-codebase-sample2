using ApiObjects.SearchObjects;
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
        public long MetaDateValue { get; set; }
        public List<string> TagNamesToAdd { get; set; }
        public List<string> TagNamesToRemove { get; set; }

        public FriendlyAssetLifeCycleRule()
            : base()
        {
            this.FilterTagTypeName = string.Empty;
            this.FilterTagValues = new List<string>();            
            this.MetaDateName = string.Empty;
            this.MetaDateValue = 0;
            this.TagNamesToAdd = new List<string>();
            this.TagNamesToRemove = new List<string>();
        }

        public FriendlyAssetLifeCycleRule(AssetLifeCycleRule alcr)
            : base(alcr)
        {
            this.FilterTagTypeName = string.Empty;
            this.FilterTagValues = new List<string>();
            this.MetaDateName = string.Empty;
            this.MetaDateValue = 0;
            this.TagNamesToAdd = new List<string>();
            this.TagNamesToRemove = new List<string>();
        }

        public FriendlyAssetLifeCycleRule(long id, int groupId, string name, string description, AssetLifeCycleRuleTransitionIntervalUnits transitionIntervalUnits, string tagType,
                                            List<string> tagValues, eCutType operand, string dateMeta, long dateValue, List<int> tagIdsToAdd, List<int> tagIdsToRemove)
            : base(id, groupId, name, description, string.Empty, transitionIntervalUnits)
        {
            this.Actions.TagIdsToAdd = new List<int>(tagIdsToAdd);
            this.Actions.TagIdsToRemove = new List<int>(tagIdsToRemove);
            this.FilterTagTypeName = tagType;
            this.FilterTagValues = new List<string>(tagValues);
            this.FilterTagOperand = operand;
            this.MetaDateName = dateMeta;
            this.MetaDateValue = dateValue;
            this.TagNamesToAdd = new List<string>();
            this.TagNamesToRemove = new List<string>();
        }

    }
}
