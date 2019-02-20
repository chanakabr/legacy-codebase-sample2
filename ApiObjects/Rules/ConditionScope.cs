using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    public interface IConditionScope
    {
        long RuleId { get; set; }
    }

    public interface ISegmentsConditionScope : IConditionScope
    {
        List<long> SegmentIds { get; set; }

        bool FilterBySegments { get; set; }
    }

    public interface IBusinessModuleConditionScope : IConditionScope
    {
        long BusinessModuleId { get; set; }

        eTransactionType? BusinessModuleType { get; set; }
    }

    public interface IDateConditionScope : IConditionScope
    {
        bool FilterByDate { get; set; }
    }

    public interface IHeaderConditionScope : IConditionScope
    {
        Dictionary<string, string> Headers { get; set; }
    }

    public interface IIpRangeConditionScope : IConditionScope
    {
        long Ip { get; set; }
    }

    public interface IAssetConditionScope : IConditionScope
    {
        long MediaId { get; set; }

        int GroupId { get; set; }

        List<BusinessModuleRule> GetBusinessModuleRulesByMediaId(int groupId, long mediaId);
    }
}