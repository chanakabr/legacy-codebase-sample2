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
        public string MetaDateName { get; set; }
        public LifeCycleTransitions Actions { get; set; }

        public AssetLifeCycleRule()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.KsqlFilter = string.Empty;
            this.MetaDateName = string.Empty;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string description, string filter, string metaDateName)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.MetaDateName = metaDateName;
            this.Actions = new LifeCycleTransitions();
        }

        public AssetLifeCycleRule(long id, string name, string description, string filter, string metaDateName, List<int> tagIdsToAdd, List<int> tagIdsToRemove,
                                  LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove, int? geoBlockRuleToSet = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.KsqlFilter = filter;
            this.MetaDateName = metaDateName;
            this.Actions = new LifeCycleTransitions(tagIdsToAdd, tagIdsToRemove, fileTypesAndPpvsToAdd, fileTypesAndPpvsToRemove, geoBlockRuleToSet);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();            
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("Name: {0}, ", string.IsNullOrEmpty(Name) ? string.Empty : Name));
            sb.Append(string.Format("Description: {0}, ", string.IsNullOrEmpty(Description) ? string.Empty : Description));
            sb.Append(string.Format("KsqlFilter: {0}, ", string.IsNullOrEmpty(KsqlFilter) ? string.Empty : KsqlFilter));
            sb.Append(string.Format("MetaDateName: {0}, ", string.IsNullOrEmpty(MetaDateName) ? string.Empty : MetaDateName));
            sb.Append(string.Format("Actions: {0}, ", Actions != null ? Actions.ToString() : string.Empty));

            return sb.ToString();
        }

    }    
}
