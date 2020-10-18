namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaBillingAction
    {
        unknown = 0,
        purchase = 1,
        renew_payment = 2,
        renew_canceled_subscription = 3,
        cancel_subscription_order = 4,
        subscription_date_changed = 5,
        pending = 6
    }
}