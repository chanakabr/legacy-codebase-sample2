using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Business module rule
    /// </summary>
    public partial class KalturaBusinessModuleRule : KalturaRule
    {
        /// <summary>
        /// List of conditions for the rule
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaCondition> Conditions { get; set; }

        /// <summary>
        /// List of actions for the rule
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        public List<KalturaBusinessModuleRuleAction> Actions { get; set; }

        /// <summary>
        /// Create date of the rule
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }
        
        /// <summary>
        /// Update date of the rule
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (this.Conditions != null || this.Conditions.Count > 0)
            {
                if (Conditions.Count(c => c.Type == KalturaRuleConditionType.BUSINESS_MODULE) > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "KalturaBusinessModuleCondition not in KalturaOrCondition");
                }

                foreach (var condition in this.Conditions)
                {
                    condition.Validate();
                }
            }

            ValidateActions();
        }
        
        private void ValidateActions()
        {
            if (this.Actions == null || this.Actions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
            }

            var duplicates = this.Actions.GroupBy(x => x.Type).Count(t => t.Count() >= 2);

            if (duplicates > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
            }
        }
        
        /// <summary>
        /// Fill current AssetRule data members with given assetRule only if they are empty\null
        /// </summary>
        /// <param name="oldRule">given assetRule to fill with</param>
        internal void FillEmpty(KalturaBusinessModuleRule oldRule)
        {
            if (oldRule != null)
            {
                this.CreateDate = oldRule.CreateDate;

                if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
                {
                    this.Name = oldRule.Name;
                }

                if (this.Description == null)
                {
                    this.Description = oldRule.Description;
                }

                if (this.Actions == null)
                {
                    this.Actions = oldRule.Actions;
                }

                if (this.Conditions == null)
                {
                    this.Conditions = oldRule.Conditions;
                }
            }
        }
    }

    public partial class KalturaBusinessModuleRuleListResponse : KalturaListResponse
    {
        /// <summary>
        /// Asset rules
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBusinessModuleRule> Objects { get; set; }
    }
}