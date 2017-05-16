using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Pricing
{
    [Serializable]
    public enum RelationTypes
    {
        And = 1,
        Or = 2
    }
    [Serializable]
    public enum WhenAlgoType
    {
        N_FIRST_TIMES = 1,
        EVERY_N_TIMES = 2
    }
    [Serializable]
    public enum DiscountTypes
    {
        Permanent = 1,
        UsageBased = 2,
        Coupon = 3
    }
    [Serializable]
    public enum CouponsStatus
    {
        Valid = 0,
        NotExists = 1,
        AllreadyUsed = 2,
        IrrelevantCode = 3,
        Expired = 4,
        NotActive = 5
    }

    [Serializable]
    public enum CouponType
    {
        Unknown = 0,
        Coupon = 1,
        Voucher = 2
    }

    [Serializable]
    public enum CampaignTrigger
    {
        Unknown = 0,
        Purchase = 1,
        SocialInvite = 3
        
    }

    [Serializable]
    public enum CampaignResult
    {
        Unknown = 0,
        Voucher = 2,
        ExtendSubscription = 4
    }

    [Serializable]
    public enum CampaignType
    {
        Unknown = 0,
        Time = 1,
        Event = 2

    }

    [Serializable]
    public enum ePricingModules
    {
        Pricing = 1,
        Discount = 2,
        Coupons = 3,
        UsageModule = 4,
        Campaign = 5,
        PPV = 5,
        Subscription = 6,
        PrePaid = 7,
        Preview = 7,
        Collection = 8,

    }

    [Serializable]
    public enum SubscriptionOrderBy
    {
        StartDateAsc = 0,
        StartDateDesc = 1
    }

    [Serializable]
    public enum VerificationPaymentGateway
    {
        Google = 1,
        Apple = 2,
        Roku = 3
    }
}
