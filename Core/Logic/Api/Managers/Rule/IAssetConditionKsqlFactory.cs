using ApiObjects.Rules;

namespace ApiLogic.Api.Managers.Rule
{
    public interface IAssetConditionKsqlFactory
    {
        string GetKsql(long groupId, AssetConditionBase condition);
    }
}