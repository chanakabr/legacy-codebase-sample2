using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.Api.Managers
{
    public interface IBusinessModuleRuleConditionScope :
        // BEO-5794 BusinessModuleRule
        IDateConditionScope,
        IAssetConditionScope,
        IBusinessModuleConditionScope,
        ISegmentsConditionScope,
        // BEO-6825 SeasonalPromotion
        IUserRoleConditionScope,
        IUserSubscriptionConditionScope,
        IAssetSubscriptionConditionScope,
        IChannelConditionScope,
        IFileTypeConditionScope
    { }

    public class BusinessModuleRuleConditionScope : IBusinessModuleRuleConditionScope
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public long BusinessModuleId { get; set; }
        public eTransactionType? BusinessModuleType { get; set; }
        public List<long> SegmentIds { get; set; }
        public bool FilterBySegments { get; set; }
        public bool FilterByDate { get; set; }
        public string UserId { get; set; }
        public List<int> UserSubscriptions { get; set; }
        public long RuleId { get; set; }
        public long MediaId { get; set; }
        public int GroupId { get; set; }
        public List<long> FileTypeIds { get; set; }

        public new bool Evaluate(RuleCondition condition)
        {
            switch (condition)
            {
                case DateCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case BusinessModuleCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case SegmentsCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case UserRoleCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case UserSubscriptionCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case AssetSubscriptionCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case AssetCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case OrCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case ChannelCondition c: return ConditionsEvaluator.Evaluate(this, c);
                case FileTypeCondition c: return ConditionsEvaluator.Evaluate(this, c);
                default: throw new NotImplementedException($"Evaluation for condition type {condition.Type} was not implemented in BusinessModuleRuleConditionScope");
            }
        }

        public List<BusinessModuleRule> GetBusinessModuleRulesByMediaId(int groupId, long mediaId)
        {
            List<BusinessModuleRule> allBusinessModuleRules = BusinessModuleRuleManager.GetAllBusinessModuleRules(groupId);
            List<BusinessModuleRule> businessRulesByMedia = new List<BusinessModuleRule>();

            string businessModuleRulesByMediaKey = LayeredCacheKeys.GetBusinessModuleRulesRulesByMediaKey(mediaId);
            string mediaInvalidationKey = LayeredCacheKeys.GetMediaInvalidationKey(groupId, mediaId);
            string allBusinessModuleRulesInvalidationKey = LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId);

            if (!LayeredCache.Instance.Get<List<BusinessModuleRule>>(businessModuleRulesByMediaKey,
                                                            ref businessRulesByMedia,
                                                            GetBusinessModuleRulesByMedia,
                                                            new Dictionary<string, object>()
                                                            {
                                                                        { "allBusinessModuleRules", allBusinessModuleRules },
                                                                        { "mediaId", mediaId },
                                                                        { "groupId", groupId }
                                                            },
                                                            groupId,
                                                            LayeredCacheConfigNames.GET_ASSET_RULES_BY_ASSET,
                                                            new List<string>()
                                                            {
                                                                        allBusinessModuleRulesInvalidationKey,
                                                                        mediaInvalidationKey
                                                            }))
            {
                log.ErrorFormat("GetBusinessModuleRulesByMediaId - Failed get data from cache. groupId: {0}", groupId);
            }

            return businessRulesByMedia;
        }

        private static Tuple<List<BusinessModuleRule>, bool> GetBusinessModuleRulesByMedia(Dictionary<string, object> funcParams)
        {
            List<BusinessModuleRule> BusinessModuleRulesByAsset = new List<BusinessModuleRule>();

            try
            {
                if (funcParams != null && funcParams.Count == 3)
                {
                    if (funcParams.ContainsKey("allBusinessModuleRules") && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        long? mediaId = funcParams["mediaId"] as long?;
                        List<BusinessModuleRule> allBusinessModuleRules = funcParams["allBusinessModuleRules"] as List<BusinessModuleRule>;

                        if (mediaId != null && allBusinessModuleRules != null && allBusinessModuleRules.Count > 0 && groupId.HasValue)
                        {
                            var assetRulesWithKsql = allBusinessModuleRules.Where(x => x.Conditions.Any(y => y is AssetCondition));

                            Parallel.ForEach(assetRulesWithKsql, (currRuleWithKsql) =>
                            {
                                StringBuilder ksqlFilter = new StringBuilder();

                                ksqlFilter.AppendFormat(string.Format("(and asset_type='media' media_id = '{0}' (or", mediaId.Value));

                                foreach (var condition in currRuleWithKsql.Conditions)
                                {
                                    if (condition is AssetCondition)
                                    {
                                        AssetCondition assetCondition = condition as AssetCondition;
                                        ksqlFilter.AppendFormat(" {0}", assetCondition.Ksql);
                                    }
                                }

                                ksqlFilter.Append("))");

                                var assets = api.SearchAssets(groupId.Value, ksqlFilter.ToString(), 0, 0, true, 0, true, string.Empty, string.Empty, string.Empty, 0, 0, true);

                                // If there is a match, add rule to list
                                if (assets != null && assets.Count() > 0)
                                {
                                    BusinessModuleRulesByAsset.Add(currRuleWithKsql);
                                }
                            });

                            log.Debug("GetBusinessModuleRulesByMedia - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BusinessModuleRulesByAsset = null;
                log.Error(string.Format("GetBusinessModuleRulesByMedia faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BusinessModuleRule>, bool>(BusinessModuleRulesByAsset, BusinessModuleRulesByAsset != null);
        }

        public List<long> GetUserRoleIds(int groupId, string userId)
        {
            var longIdsResponse = Core.Users.Module.GetUserRoleIds(groupId, userId);
            if (longIdsResponse != null) { return longIdsResponse.Ids; }

            return null;
        }

        public bool IsMediaIncludedInSubscription(int groupId, long mediaId, HashSet<long> subscriptionIds)
        {
            var subscriptionsChannels = Core.Pricing.Module.Instance.GetSubscriptions(groupId, subscriptionIds, string.Empty, string.Empty, string.Empty, null, 
                SubscriptionOrderBy.StartDateAsc, 0, 30, true, null, false, null, null, null, null);
            if (subscriptionsChannels == null || subscriptionsChannels.Subscriptions == null) { return false; }

            HashSet<int> channelsIds = new HashSet<int>();
            foreach (var channelSubscription in subscriptionsChannels.Subscriptions)
            {
                foreach (var item in channelSubscription.m_sCodes)
                {
                    if (int.TryParse(item.m_sCode, out int subscriptionId) && !channelsIds.Contains(subscriptionId))
                    {
                        channelsIds.Add(subscriptionId);
                    }
                }
            }

            if (channelsIds.Count == 0) { return false; }

            List<int> validChannelIds = Core.ConditionalAccess.Utils.ValidateMediaContainedInChannels((int)mediaId, groupId, channelsIds);
            return validChannelIds != null && validChannelIds.Count > 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (BusinessModuleId > 0)
            {
                sb.AppendFormat("BusinessModuleId: {0}; ", BusinessModuleId);
            }

            if (BusinessModuleType.HasValue)
            {
                sb.AppendFormat("BusinessModuleType: {0}; ", BusinessModuleType.Value);
            }

            if (FilterByDate)
            {
                sb.AppendFormat("FilterByDate: {0}; ", FilterByDate);
            }

            if (FilterBySegments)
            {
                sb.AppendFormat("FilterBySegments: {0}; ", FilterBySegments);
            }

            if (SegmentIds != null && SegmentIds.Count > 0)
            {
                sb.AppendFormat("SegmentIds: {0}; ", string.Join(",", SegmentIds));
            }

            if (MediaId > 0)
            {
                sb.AppendFormat("MediaId: {0}; ", MediaId);
            }

            if (GroupId > 0)
            {
                sb.AppendFormat("GroupId: {0}; ", GroupId);
            }

            if (!string.IsNullOrEmpty(UserId))
            {
                sb.AppendFormat("UserId: {0}; ", UserId);
            }

            if (UserSubscriptions != null)
            {
                sb.AppendFormat("UserSubscriptions: {0}; ", string.Join(", ", UserSubscriptions));
            }

            return sb.ToString();
        }

        public List<long> GetChannelsByMediald(int groupId, long mediaId)
        {
            var channels = ChannelManager.Instance.GetChannelsContainingMedia(groupId, mediaId, 0, 0, ChannelOrderBy.Id, OrderDir.NONE, true, 0);
            if (channels.Objects?.Count > 0)
            {
                return channels.Objects.Select(x => (long)x.m_nChannelID).ToList();
            }

            return null;
        }
    }
}