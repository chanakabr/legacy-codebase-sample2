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
        public int TransitionIntervalInDays { get; set; }
        public LifeCycleTransitions Actions { get; set; }

        public AssetLifeCycleRule()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.KsqlFilter = string.Empty;
            this.TransitionIntervalInDays = 0;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string description, string filter, int transitionIntervalInDays)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.TransitionIntervalInDays = transitionIntervalInDays;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string description, string filter, int transitionIntervalInDays, List<int> tagIdsToAdd, List<int> tagIdsToRemove,
                                    LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove, int? geoBlockRuleToSet = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.TransitionIntervalInDays = transitionIntervalInDays;
            this.Actions = new LifeCycleTransitions(tagIdsToAdd, tagIdsToRemove, fileTypesAndPpvsToAdd, fileTypesAndPpvsToRemove, geoBlockRuleToSet);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("Name: {0}, ", string.IsNullOrEmpty(Name) ? string.Empty : Name));
            sb.Append(string.Format("Description: {0}, ", string.IsNullOrEmpty(Description) ? string.Empty : Description));
            sb.Append(string.Format("KsqlFilter: {0}, ", string.IsNullOrEmpty(KsqlFilter) ? string.Empty : KsqlFilter));
            sb.Append(string.Format("TransitionIntervalInDays: {0}, ", TransitionIntervalInDays));
            sb.Append(string.Format("Actions: {0}, ", Actions != null ? Actions.ToString() : string.Empty));

            return sb.ToString();
        }

    }    
}
