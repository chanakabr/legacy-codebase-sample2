namespace WebAPI.Models.API
{
    /// <summary>
    /// AssetSubscription Condition - indicates which assets this rule is applied on by their subscriptions
    /// </summary>
    public partial class KalturaAssetSubscriptionCondition : KalturaSubscriptionCondition
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.ASSET_SUBSCRIPTION;
        }
    }
}