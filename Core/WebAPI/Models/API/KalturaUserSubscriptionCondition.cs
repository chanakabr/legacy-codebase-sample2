namespace WebAPI.Models.API
{
    /// <summary>
    /// UserSubscription Condition - indicates which users this rule is applied on by their subscriptions
    /// </summary>
    public partial class KalturaUserSubscriptionCondition : KalturaSubscriptionCondition
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_SUBSCRIPTION;
        }
    }
}