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
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string KsqlFilter { get; set; }
        public AssetLifeCycleRuleTransitionIntervalUnits TransitionIntervalUnits { get; set; } // from Tvinci.dbo.lu_alcr_transition_interval_units
        public LifeCycleTransitions Actions { get; set; }

        public AssetLifeCycleRule()
        {
            this.Id = 0;
            this.GroupId = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.KsqlFilter = string.Empty;                        
            this.TransitionIntervalUnits = AssetLifeCycleRuleTransitionIntervalUnits.Unknown;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, int groupId, string name, string description, string filter, AssetLifeCycleRuleTransitionIntervalUnits transitionIntervalUnits)
        {
            this.Id = id;
            this.GroupId = groupId;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.TransitionIntervalUnits = transitionIntervalUnits;
            this.Actions = new LifeCycleTransitions();            
        }

        public AssetLifeCycleRule(long id, int groupId, string name, string description, string filter, AssetLifeCycleRuleTransitionIntervalUnits transitionIntervalUnits, List<int> tagIdsToAdd, List<int> tagIdsToRemove,
                                  LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove, int? geoBlockRuleToSet = null)
        {
            this.Id = id;
            this.GroupId = groupId;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.TransitionIntervalUnits = transitionIntervalUnits;
            this.Actions = new LifeCycleTransitions(tagIdsToAdd, tagIdsToRemove, fileTypesAndPpvsToAdd, fileTypesAndPpvsToRemove, geoBlockRuleToSet);
        }

        public AssetLifeCycleRule(AssetLifeCycleRule alcr)            
        {
            this.Actions = alcr.Actions;
            this.Description = alcr.Description;
            this.GroupId = alcr.GroupId;
            this.Id = alcr.Id;
            this.KsqlFilter = alcr.KsqlFilter;
            this.Name = alcr.Name;
            this.TransitionIntervalUnits = alcr.TransitionIntervalUnits;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));
            sb.Append(string.Format("Name: {0}, ", string.IsNullOrEmpty(Name) ? string.Empty : Name));
            sb.Append(string.Format("Description: {0}, ", string.IsNullOrEmpty(Description) ? string.Empty : Description));
            sb.Append(string.Format("KsqlFilter: {0}, ", string.IsNullOrEmpty(KsqlFilter) ? string.Empty : KsqlFilter));
            sb.Append(string.Format("TransitionIntervalUnits: {0}, ", TransitionIntervalUnits.ToString()));
            sb.Append(string.Format("Actions: {0}, ", Actions != null ? Actions.ToString() : string.Empty));

            return sb.ToString();
        }

    }    
}
