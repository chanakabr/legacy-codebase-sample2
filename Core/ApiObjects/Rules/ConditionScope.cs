using ApiObjects.Response;
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

    public interface IUserSubscriptionConditionScope : IConditionScope
    {
        List<int> UserSubscriptions { get; set; }
    }

    public interface IAssetSubscriptionConditionScope : IConditionScope
    {
        int GroupId { get; set; }
        long MediaId { get; set; }

        bool IsMediaIncludedInSubscription(int groupId, long mediaId, HashSet<long> subscriptionIds);
    }

    public interface IUserRoleConditionScope : IConditionScope
    {
        string UserId { get; set; }
        int GroupId { get; set; }

        List<long> GetUserRoleIds(int groupId, string userId);
    }

    public interface ITriggerCampaignConditionScope : IConditionScope
    {
        int GroupId { get; set; }
        string UserId { get; set; }
        long CampaignId { get; set; }
        int? BrandId { get; set; }
        int? Family { get; set; }
        int? ManufacturerId { get; set; }
        string Model { get; set; }
        string Udid { get; set; }
    }
}