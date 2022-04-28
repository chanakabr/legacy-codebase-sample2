using ApiLogic.Api.Managers;
using ApiObjects.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Api.Managers
{
    public interface INetworkConditionScope: IHeaderConditionScope, IIpRangeConditionScope, IIpV6RangeConditionScope { }
    
    public class NetworkConditionScope : INetworkConditionScope
    {
        public Dictionary<string, string> Headers { get; set; }
        public long RuleId { get; set; }
        public long Ip { get; set; }
        public string IpV6 { get; set; }

        public bool Evaluate(RuleCondition condition)
        {
            switch (condition)
            {
                case HeaderCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case IpRangeCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case IpV6RangeCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case OrCondition c: return ConditionsEvaluator.Evaluate(this, c);
                default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in NetworkConditionScope");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Headers != null && Headers.Count > 0)
            {
                sb.AppendFormat("Headers: {0}; ", string.Join(",", Headers));
            }

            var isV6 = !string.IsNullOrEmpty(this.IpV6);
            if (!isV6 && Ip > 0)
            {
                sb.AppendFormat("Ip: {0}; ", Ip);
            }
            else if (isV6)
            {
                sb.AppendFormat("Ip: {0}; ", IpV6);
            }
            return sb.ToString();
        }
    }
}
