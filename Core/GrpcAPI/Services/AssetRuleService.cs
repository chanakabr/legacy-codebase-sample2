using AutoMapper;
using Core.Api.Managers;
using Google.Protobuf.Collections;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssetRuleOrderBy = ApiObjects.AssetRuleOrderBy;
using ConcurrencyRestrictionPolicy = phoenix.ConcurrencyRestrictionPolicy;
using RuleActionType = ApiObjects.RuleActionType;
using RuleConditionType = ApiObjects.RuleConditionType;

namespace GrpcAPI.Services
{
    public interface IAssetRuleService
    {
        bool HasAssetRules(HasAssetRulesRequest request);
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

        public bool HasAssetRules(HasAssetRulesRequest request)
        {
            return AssetRuleManager.HasAssetRules(request.GroupId, (RuleActionType)request.ActionType);
        }
        
        public CheckNetworkRulesResponse CheckNetworkRules(CheckNetworkRulesRequest checkNetworkRulesRequest)
        {
            try
            {
                List<ApiObjects.Rules.SlimAsset> slimAsset = checkNetworkRulesRequest.SlimAsset != null ? 
                    Mapper.Map<List<ApiObjects.Rules.SlimAsset>>(checkNetworkRulesRequest.SlimAsset) : null;
                var status = AssetRuleManager.CheckNetworkRules(slimAsset, checkNetworkRulesRequest.GroupId,
                    checkNetworkRulesRequest.Ip, out var assetRule);
                
                return new CheckNetworkRulesResponse()
                {
                    Status = status != null ? Mapper.Map<Status>(status) : null,
                    AssetRule = assetRule != null ? Mapper.Map<AssetRule>(assetRule) : null
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

            return new GetAssetMediaRuleIdsResponse();
        }

        public GetAssetEpgRuleIdsResponse GetAssetEpgRuleIds(GetAssetEpgRuleIdsRequest request)
        {
            long programId = 0;
            DateTime programEndDate = default;
            var assetEpgRuleIds = Core.ConditionalAccess.Utils.GetAssetEpgRuleIds(request.GroupId,
                request.MediaId, ref programId, ref programEndDate);

            return new GetAssetEpgRuleIdsResponse()
            {
                Ids = {assetEpgRuleIds ?? new List<long>()},
                ProgramId = programId,
                ProgramIdEndDate = programEndDate != default ? new DateTimeOffset(programEndDate).ToUnixTimeMilliseconds() : 0
            };
        }

        public GetMediaConcurrencyRulesResponse GetMediaConcurrencyRules(GetMediaConcurrencyRulesRequest request)
        {
            try
            {
                var concurrencyRules = Core.Api.Module.GetMediaConcurrencyRules(request.GroupId, request.MediaId,
                    request.BusinessModuleId, (ApiObjects.eBusinessModule) request.BusinessModuleType);
                if (concurrencyRules != null)
                {
                    return new GetMediaConcurrencyRulesResponse()
                    {
                        MediaConcurrencyRules =
                        {
                            new RepeatedField<phoenix.MediaConcurrencyRule> {concurrencyRules.Select(x => phoenix.MediaConcurrencyRule.Parser.ParseFrom(GrpcSerialize.ProtoSerialize(x)))}   
                        }
                    };
                }

                return new GetMediaConcurrencyRulesResponse();
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
                var slimAsset = Mapper.Map<ApiObjects.Rules.SlimAsset>(request.SlimAsset);
                var assetRuleCondition = request.RuleActionType == phoenix.RuleActionType.None ? (phoenix.RuleActionType?) null : request.RuleActionType;
                var assetRules = Core.Api.Module.GetAssetRules(
                        (RuleConditionType)request.AssetRuleConditionType,
                        request.GroupId,
                        slimAsset,
                        (RuleActionType?)assetRuleCondition,
                        request.NameContains,
                        (AssetRuleOrderBy)request.OrderBy);
                return new GetAssetRulesResponse()
                {
                    Status = Mapper.Map<Status>(assetRules.Status),
                    TotalCount = assetRules.TotalItems,
                    AssetRules =
                    {
                        Mapper.Map<RepeatedField<AssetRule>>(assetRules.Objects)   
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
                            new RepeatedField<phoenix.MediaConcurrencyRule> {groupMediaConcurrencyRules.Select(x => phoenix.MediaConcurrencyRule.Parser.ParseFrom(GrpcSerialize.ProtoSerialize(x)))}
                        }
                    };
                }

                return new GetGroupMediaConcurrencyRulesResponse();
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

            return new GetMediaConcurrencyByIdResponse();
        }
    }
}