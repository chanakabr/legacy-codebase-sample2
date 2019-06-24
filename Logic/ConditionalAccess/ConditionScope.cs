using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Core.Api;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess
{
    public class ConditionScope : IConditionScope, IBusinessModuleConditionScope, ISegmentsConditionScope, IDateConditionScope, IHeaderConditionScope, IIpRangeConditionScope, IAssetConditionScope
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public long BusinessModuleId { get; set; }

        public eTransactionType? BusinessModuleType { get; set; }

        public bool FilterByDate { get; set; }

        public bool FilterBySegments { get; set; }

        public List<long> SegmentIds { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public long Ip { get; set; }

        public long MediaId { get; set; }

        public int GroupId { get; set; }

        public long RuleId { get; set; }
        
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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

            if (Headers != null && Headers.Count > 0)
            {
                sb.AppendFormat("Headers: {0}; ", string.Join(",", Headers));
            }

            if (Ip > 0)
            {
                sb.AppendFormat("Ip: {0}; ", Ip);
            }

            return sb.ToString();
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

                            log.Debug("GetAssetRulesByAsset - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BusinessModuleRulesByAsset = null;
                log.Error(string.Format("GetAssetRulesByAsset faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BusinessModuleRule>, bool>(BusinessModuleRulesByAsset, BusinessModuleRulesByAsset != null);
        }
    }
}
