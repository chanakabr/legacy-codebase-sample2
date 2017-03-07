using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.AssetLifeCycleRules
{
    public class LifeCycleTransitions
    {

        public List<int> TagIdsToAdd { get; set; } // from Tvinci.dbo.tags
        public List<int> TagIdsToRemove { get; set; } // from Tvinci.dbo.tags
        public LifeCycleFileTypesAndPpvsTransitions FileTypesAndPpvsToAdd { get; set; }
        public LifeCycleFileTypesAndPpvsTransitions FileTypesAndPpvsToRemove { get; set; }
        public int? GeoBlockRuleToSet { get; set; }

        public LifeCycleTransitions()
        {
            this.TagIdsToAdd = new List<int>();
            this.TagIdsToRemove = new List<int>();
            this.FileTypesAndPpvsToAdd = new LifeCycleFileTypesAndPpvsTransitions();
            this.FileTypesAndPpvsToRemove = new LifeCycleFileTypesAndPpvsTransitions();
            this.GeoBlockRuleToSet = null;
        }

        public LifeCycleTransitions(List<int> tagIdsToAdd, List<int> tagIdsToRemove, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove, int? geoBlockRuleToSet = null)
        {
            this.TagIdsToAdd = tagIdsToAdd;
            this.TagIdsToRemove = tagIdsToRemove;
            this.FileTypesAndPpvsToAdd = fileTypesAndPpvsToAdd;
            this.FileTypesAndPpvsToRemove = fileTypesAndPpvsToRemove;
            this.GeoBlockRuleToSet = geoBlockRuleToSet;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("TagIdsToAdd: {0}, ", TagIdsToAdd != null ? string.Join(",", TagIdsToAdd) : string.Empty));
            sb.Append(string.Format("TagIdsToRemove: {0}, ", TagIdsToRemove != null ? string.Join(",", TagIdsToRemove) : string.Empty));
            sb.Append(string.Format("FileTypesAndPpvsToAdd: {0}, ", FileTypesAndPpvsToAdd != null ? FileTypesAndPpvsToAdd.ToString() : string.Empty));
            sb.Append(string.Format("FileTypesAndPpvsToRemove: {0}, ", FileTypesAndPpvsToRemove != null ? FileTypesAndPpvsToRemove.ToString() : string.Empty));
            sb.Append(string.Format("GeoBlockRuleToSet: {0}, ", GeoBlockRuleToSet.HasValue ? GeoBlockRuleToSet.Value.ToString() : string.Empty));            

            return sb.ToString();
        }

    }
}
