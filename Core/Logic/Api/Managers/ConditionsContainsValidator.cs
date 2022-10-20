using APILogic.ConditionalAccess;
using ApiObjects.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiLogic.Api.Managers
{
    public class ConditionsContainsValidator
    {
        public static bool ValidateSegmentExist(List<RuleCondition> conditions, long segmentId)
        {
            if (conditions != null && conditions.Count > 0)
            {
                foreach (var condition in conditions)
                {
                    // at least one condition applyed on scope
                    switch (condition)
                    {
                        case SegmentsCondition c:
                            {
                                if (ContainsSegment(c, segmentId)) { return true; }
                                break;
                            }
                        case OrCondition c:
                            {
                                if (ValidateSegmentExist(c.Conditions, segmentId)) { return true; }
                                break;
                            }
                        default: continue;
                    }
                }
            }
            return false;
        }

        private static bool ContainsSegment(SegmentsCondition condition, long segmentId)
        {
            return condition.SegmentIds.Contains(segmentId);
        }
    }
}