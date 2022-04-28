using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using ApiObjects;
using Core.Api.Managers;
using Google.Protobuf.Collections;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;
using ConcurrencyRestrictionPolicy = phoenix.ConcurrencyRestrictionPolicy;
using RuleConditionType = ApiObjects.RuleConditionType;

namespace GrpcAPI.Services
{
    public interface IAssetRuleService
    {
        CheckNetworkRulesResponse CheckNetworkRules(CheckNetworkRulesRequest request);
        GetAssetMediaRuleIdsResponse GetAssetMediaRuleIds(GetAssetMediaRuleIdsRequest request);
        GetAssetEpgRuleIdsResponse GetAssetEpgRuleIds(GetAssetEpgRuleIdsRequest request);
        GetMediaConcurrencyRulesResponse GetMediaConcurrencyRules(GetMediaConcurrencyRulesRequest request);
        GetAssetRulesResponse GetAssetRules(GetAssetRulesRequest request);

        GetGroupMediaConcurrencyRulesResponse GetGroupMediaConcurrencyRules(
            GetGroupMediaConcurrencyRulesRequest request);

        GetMediaConcurrencyByIdResponse GetMediaConcurrencyRule(
            GetMediaConcurrencyByIdRequest request);
    }

    public class AssetRuleService : IAssetRuleService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public CheckNetworkRulesResponse CheckNetworkRules(CheckNetworkRulesRequest checkNetworkRulesRequest)
        {
            try
            {
                List<ApiObjects.Rules.SlimAsset> slimAsset =
                    GrpcMapping.Mapper.Map<List<ApiObjects.Rules.SlimAsset>>(checkNetworkRulesRequest.SlimAsset);
                var status = AssetRuleManager.CheckNetworkRules(slimAsset, checkNetworkRulesRequest.GroupId,
                    checkNetworkRulesRequest.Ip, out var assetRule);

                return new CheckNetworkRulesResponse()
                {
                    Status = status != null ? GrpcMapping.Mapper.Map<Status>(status) : null,
                    AssetRule = assetRule != null ? GrpcMapping.Mapper.Map<AssetRule>(assetRule) : null
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping CheckNetworkRules GRPC service {e.Message}");
                return null;
            }
        }

        public GetAssetMediaRuleIdsResponse GetAssetMediaRuleIds(GetAssetMediaRuleIdsRequest request)
        {
            var assetMediaRuleIds =
                Core.ConditionalAccess.Utils.GetAssetMediaRuleIds(request.GroupId, request.MediaId);
            if (assetMediaRuleIds != null)
            {
                return new GetAssetMediaRuleIdsResponse()
                {
                    Ids = {assetMediaRuleIds}
                };
            }

            return null;
        }

        public GetAssetEpgRuleIdsResponse GetAssetEpgRuleIds(GetAssetEpgRuleIdsRequest request)
        {
            long programId = 0;
            var assetEpgRuleIds = Core.ConditionalAccess.Utils.GetAssetEpgRuleIds(request.GroupId,
                request.MediaId, ref programId);

            if (assetEpgRuleIds != null)
            {
                return new GetAssetEpgRuleIdsResponse()
                {
                    Ids = {assetEpgRuleIds},
                    ProgramId = programId
                };
            }

            return null;
        }

        public GetMediaConcurrencyRulesResponse GetMediaConcurrencyRules(GetMediaConcurrencyRulesRequest request)
        {
            try
            {
                var concurrencyRules = Core.Api.Module.GetMediaConcurrencyRules(request.GroupId, request.MediaId,
                    request.BusinessModuleId, (eBusinessModule) request.BusinessModuleType);
                if (concurrencyRules != null)
                {
                    return new GetMediaConcurrencyRulesResponse()
                    {
                        MediaConcurrencyRules =
                        {
                            GrpcMapping.Mapper.Map<RepeatedField<mediaConcurrencyRule>>(concurrencyRules)
                        }
                    };
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetMediaConcurrencyRules GRPC service {e.Message}");
                return null;
            }
        }

        public GetAssetRulesResponse GetAssetRules(GetAssetRulesRequest request)
        {
            try
            {
                var slimAsset = GrpcMapping.Mapper.Map<ApiObjects.Rules.SlimAsset>(request.SlimAsset.Data);
                var assetRuleCondition = request.RuleActionType.Data != GetAssetRulesRequest.Types.RuleActionType.None
                    ? request.RuleActionType.Data
                    : (GetAssetRulesRequest.Types.RuleActionType?) null;
                var assetRules =
                    Core.Api.Module.GetAssetRules((RuleConditionType) request.AssetRuleConditionType, request.GroupId,
                        slimAsset, (RuleActionType?) assetRuleCondition);
                return new GetAssetRulesResponse()
                {
                    Status = GrpcMapping.Mapper.Map<Status>(assetRules.Status),
                    TotalCount = assetRules.TotalItems,
                    AssetRules =
                    {
                        GrpcMapping.Mapper.Map<RepeatedField<AssetRule>>(assetRules.Objects)
                    }
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetAssetRules GRPC service {e.Message}");
                return null;
            }
        }

        public GetGroupMediaConcurrencyRulesResponse GetGroupMediaConcurrencyRules(
            GetGroupMediaConcurrencyRulesRequest request)
        {
            try
            {
                var groupMediaConcurrencyRules = Core.Api.api.GetGroupMediaConcurrencyRules(request.GroupId);
                if (groupMediaConcurrencyRules != null)
                {
                    return new GetGroupMediaConcurrencyRulesResponse()
                    {
                        MediaConcurrencyRules =
                        {
                            GrpcMapping.Mapper.Map<RepeatedField<mediaConcurrencyRule>>(groupMediaConcurrencyRules)
                        }
                    };
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetGroupMediaConcurrencyRules GRPC service {e.Message}");
                return null;
            }
        }

        public GetMediaConcurrencyByIdResponse GetMediaConcurrencyRule(
            GetMediaConcurrencyByIdRequest request)
        {
            var rule = DAL.ApiDAL.GetMCRuleByID(request.RuleId);
            if (rule != null)
            {
                return new GetMediaConcurrencyByIdResponse
                {
                    MediaConcurrencyLimit = rule.Limitation,
                    Policy = (ConcurrencyRestrictionPolicy) rule.RestrictionPolicy
                };
            }

            return null;
        }
    }
}