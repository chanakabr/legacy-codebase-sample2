using phoenix;
using Phx.Lib.Log;
using System.Reflection;

namespace GrpcAPI.Services
{
    public interface IAssetUserRuleService
    {
        long GetAssetUserRuleByUserId(GetAssetUserRuleByUserIdRequest request);
        bool IsAssetUserRuleIdValid(IsAssetUserRuleIdValidRequest request);
    }

    public class AssetUserRuleService : IAssetUserRuleService
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public long GetAssetUserRuleByUserId(GetAssetUserRuleByUserIdRequest request)
        {
            long result = 0;
            var genericResponse = Core.Api.Module.GetAssetUserRuleList((int)request.GroupId, request.UserId, ApiObjects.RuleActionType.UserFilter, ApiObjects.RuleConditionType.AssetShop, false);

            if (genericResponse != null && genericResponse.HasObjects())
            {
                result = genericResponse.Objects[0].Id;
            }

            return result;
        }

        public bool IsAssetUserRuleIdValid(IsAssetUserRuleIdValidRequest request)
        {
            bool isValid = false;
            var genericResponse = Core.Api.Module.GetAssetUserRuleList((int)request.GroupId, null, ApiObjects.RuleActionType.UserFilter, ApiObjects.RuleConditionType.AssetShop, false);

            if (genericResponse != null && genericResponse.HasObjects())
            {
                isValid = genericResponse.Objects.Exists(rule => rule.Id == request.AssetUserRuleId);
            }

            return isValid;
        }
    }
}