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
        public Dictionary<int, List<int>> FileTypesToPpvsMapToAdd { get; set; }
        public Dictionary<int, List<int>> FileTypesToPpvsMapToRemove { get; set; }
        public int? GeoBlockRuleToSet { get; set; }

        public LifeCycleTransitions()
        {
            this.TagIdsToAdd = new List<int>();
            this.TagIdsToRemove = new List<int>();
            this.FileTypesToPpvsMapToAdd = new Dictionary<int, List<int>>();
            this.FileTypesToPpvsMapToRemove = new Dictionary<int, List<int>>();
            this.GeoBlockRuleToSet = null;
        }

        public LifeCycleTransitions(List<int> tagIdsToAdd, List<int> tagIdsToRemove, Dictionary<int, List<int>> fileTypesToPpvsMapToAdd, Dictionary<int, List<int>> fileTypesToPpvsMapToRemove, int? geoBlockRuleToSet = null)
        {
            this.TagIdsToAdd = tagIdsToAdd;
            this.TagIdsToRemove = tagIdsToRemove;
            this.FileTypesToPpvsMapToAdd = fileTypesToPpvsMapToAdd;
            this.FileTypesToPpvsMapToRemove = fileTypesToPpvsMapToRemove;
            this.GeoBlockRuleToSet = geoBlockRuleToSet;
        }

    }
}
