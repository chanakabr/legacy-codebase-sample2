using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.AssetLifeCycleRules
{
    public class AssetLifeCycleRule
    {

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string KsqlFilter { get; set; }
        public LifeCycleTransitions Actions { get; set; }

        public AssetLifeCycleRule()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.KsqlFilter = string.Empty;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string filter, string description)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string filter, string description, List<int> tagIdsToAdd, List<int> tagIdsToRemove, 
                                    Dictionary<int, List<int>> fileTypesToPpvsMapToAdd,Dictionary<int, List<int>> fileTypesToPpvsMapToRemove, int? geoBlockRuleToSet = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.Actions = new LifeCycleTransitions(tagIdsToAdd, tagIdsToRemove, fileTypesToPpvsMapToAdd, fileTypesToPpvsMapToRemove, geoBlockRuleToSet);
        }

    }    
}
