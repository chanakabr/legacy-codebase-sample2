using ApiLogic.Api.Managers;
using ApiObjects.Rules;
using System;
using System.Collections.Generic;

namespace ApiLogic.ConditionalAccess
{
    public interface IBatchCampaignConditionScope : ISegmentsConditionScope { }

    public class BatchCampaignConditionScope : IBatchCampaignConditionScope
    {
        public List<long> SegmentIds { get; set; }
        public bool FilterBySegments { get; set; }
        public long RuleId { get; set; }

        public bool Evaluate(RuleCondition condition)
        {
            switch (condition)
            {
                case SegmentsCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case OrCondition c: return ConditionsEvaluator.Evaluate(this, c);
                default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in BatchCampaignConditionScope");
            }
        }
    }
}
