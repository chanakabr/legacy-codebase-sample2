using System.Collections.Generic;
using ApiLogic.Api.Managers;
using ApiLogic.Users.Managers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using Core.Api.Managers;
using Jaeger.Thrift;
using phoenix;

namespace GrpcAPI.Services
{
    public interface ISegmentService
    {
        bool IsSegmentUsed(IsSegmentUsedRequest request);
    }

    public class SegmentService : ISegmentService
    {
        public bool IsSegmentUsed(IsSegmentUsedRequest request)
        {
            var isSegmentUsed = IsSegmentUsedByBusinessModuleRules(request) ||
                                IsSegmentUsedByCampaigns(request) ||
                                IsSegmentUsedByAssetRules(request);
            return isSegmentUsed;
        }

        private bool IsSegmentUsedByBusinessModuleRules(IsSegmentUsedRequest request)
        {
            var businessModuleRules = BusinessModuleRuleManager.GetAllBusinessModuleRules(request.GroupId);
            if (businessModuleRules != null && businessModuleRules.Count > 0)
            {
                foreach (var businessModuleRule in businessModuleRules)
                {
                    if (ConditionsContainsValidator.ValidateSegmentExist(businessModuleRule.Conditions, request.SegmentId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsSegmentUsedByCampaigns(IsSegmentUsedRequest request)
        {
            var contextData = new ContextData(request.GroupId);
            var filter = new CampaignSegmentFilter() 
            { 
                SegmentIdEqual = request.SegmentId, 
                StateIn = new List<CampaignState>{ CampaignState.INACTIVE, CampaignState.ACTIVE },
                IgnoreSetFilterByShop = true
            };
            var campaigns = CampaignManager.Instance.ListCampaignsBySegment(contextData, filter);
            return campaigns.HasObjects();
        }

        private bool IsSegmentUsedByAssetRules(IsSegmentUsedRequest request)
        {
            var assetRules = AssetRuleManager.Instance.GetAssetRules(ApiObjects.RuleConditionType.Segments, request.GroupId);
            if (assetRules.HasObjects())
            {
                foreach (var assetRule in assetRules.Objects)
                {
                    if (ConditionsContainsValidator.ValidateSegmentExist(assetRule.Conditions, request.SegmentId))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}