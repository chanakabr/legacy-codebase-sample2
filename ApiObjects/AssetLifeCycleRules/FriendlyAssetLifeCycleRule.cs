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
        public long MetaDateValueInSeconds { get; set; }        

        public FriendlyAssetLifeCycleRule()
            : base()
        {
            this.FilterTagTypeName = string.Empty;
            this.FilterTagValues = new List<string>();            
            this.MetaDateName = string.Empty;
            this.MetaDateValueInSeconds = 0;
        }

        public FriendlyAssetLifeCycleRule(AssetLifeCycleRule alcr)
            : base(alcr)
        {
            this.FilterTagTypeName = string.Empty;
            this.FilterTagValues = new List<string>();
            this.MetaDateName = string.Empty;
            this.MetaDateValueInSeconds = 0;
            if (!BuildActionRuleDataFromKsql())
            {
                throw new Exception(string.Format("Failed FriendlyAssetLifeCycleRule constructor(AssetLifeCycleRule alcr), ruleId: {0}", this.Id));
            }
        }

        public FriendlyAssetLifeCycleRule(int groupId, string tagType, List<string> tagValues, eCutType operand, string dateMeta, long dateValue)
            : base()
        {
            this.GroupId = groupId;
            this.FilterTagTypeName = tagType;
            this.FilterTagValues = new List<string>(tagValues);
            this.FilterTagOperand = operand;
            this.MetaDateName = dateMeta;
            this.MetaDateValueInSeconds = dateValue;
            if (!BuildActionRuleKsqlFromData())
            {
                throw new Exception(string.Format(@"Failed FriendlyAssetLifeCycleRule constructor(int groupId, string tagType, List<string> tagValues, eCutType operand, string dateMeta, long dateValue), ruleId: {0}", this.Id));
            }
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
            this.MetaDateValueInSeconds = dateValue;
            if (BuildActionRuleKsqlFromData())
            {                
                throw new Exception(string.Format(@"Failed FriendlyAssetLifeCycleRule constructor(int groupId, string tagType, List<string> tagValues, eCutType operand, string dateMeta, long dateValue), ruleId: {0}", this.Id));
            }
        }

        private bool BuildActionRuleDataFromKsql()
        {
            bool result = false;
            BooleanPhraseNode phrase = null;
            if (string.IsNullOrEmpty(this.KsqlFilter))
            {
                return result;
            }

            // Parse the rule's KSQL
            var status = BooleanPhraseNode.ParseSearchExpression(this.KsqlFilter, ref phrase);
            //(and genre = 'a' genre='b' date=-360)
            //(and date>-360 (or genre='a' genre='b'))
            // Validate parse result
            if (status != null && status.Code == (int)ResponseStatus.OK && phrase != null)
            {
                // It should be a phrase, because it is (and ...)
                if (phrase is BooleanPhrase)
                {
                    var nodes = (phrase as BooleanPhrase).nodes;

                    // Validate there is at least one node
                    // First node should be a PHRASE, looking like: (or cycletag='A' cycletag='B')
                    if (nodes.Count > 0)
                    {
                        var firstNode = nodes[0] as BooleanPhrase;

                        if (firstNode != null)
                        {
                            this.FilterTagOperand = firstNode.operand;

                            var firstTag = firstNode.nodes[0] as BooleanLeaf;

                            // field name - we can take from the first
                            if (firstTag != null)
                            {
                                this.FilterTagTypeName = firstTag.field;
                            }

                            // add all values to list. all should be leafs
                            foreach (var item in firstNode.nodes)
                            {
                                var leaf = item as BooleanLeaf;

                                if (leaf != null)
                                {
                                    this.FilterTagValues.Add(Convert.ToString(leaf.value));
                                }
                            }
                        }
                    }

                    // Validate that date nodes actually exist - there should be two
                    // Second and third nodes should be LEAFs as well, looking like: cycledate<'-2days'
                    if (nodes.Count > 1)
                    {
                        var secondNode = nodes[1] as BooleanLeaf;

                        if (secondNode != null)
                        {
                            this.MetaDateName = secondNode.field;

                            this.MetaDateValueInSeconds = Math.Abs(Convert.ToInt64(secondNode.value));                            
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private bool BuildActionRuleKsqlFromData()
        {
            bool result = false;

            if (!string.IsNullOrEmpty(this.FilterTagTypeName) && this.FilterTagValues != null && this.FilterTagValues.Count > 0 && !string.IsNullOrEmpty(this.MetaDateName) && this.MetaDateValueInSeconds > 0)
            {                
                long date = -1 * this.MetaDateValueInSeconds;
                StringBuilder builder = new StringBuilder();
                foreach (var tagValue in this.FilterTagValues)
                {
                    builder.AppendFormat("{0}='{1}' ", this.FilterTagTypeName, tagValue);
                }

                this.KsqlFilter = string.Format("(and ({0} {1}) {2}<'{3}')", 
                    // 0
                    this.FilterTagOperand.ToString(), 
                    // 1
                    builder.ToString(), 
                    // 3
                    this.MetaDateName, 
                    // 4
                    date);

                result = true;
            }

            return result;
        }        

    }
}
