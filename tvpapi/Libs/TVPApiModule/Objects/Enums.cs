using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TVPApiModule.Objects
{
    public class Enums
    {
        public enum eCutWith
        {
            OR = 0,
            AND = 1
        }

        public enum ProgramIdType
        {
            EXTERNAL = 0,
            INTERNAL = 1
        }

        public enum eCode
        {
            Success = 0,
            Failure = 1,
            BadArguments = 2
        }

        [Serializable]
        public enum eAssetFilterTypes
        {
            [EnumMember]
            EPG = 0,

            [EnumMember]
            NPVR = 1
        }
    }

    public enum BillingItemsType
    {
        Unknown = 0,
        PPV = 1,
        Subscription = 2,
        PrePaid = 3,
        PrePaidExpired = 4,
        Collection = 5
    }

    public enum PaymentMethod
    {
        Unknown = 0,
        CreditCard = 1,
        SMS = 2,
        PayPal = 3,
        DebitCard = 4,
        Ideal = 5,
        Incaso = 6,
        Gift = 7,
        Visa = 20,
        MasterCard = 21,
        InApp = 200,
        M1 = 60,
        ChangeSubscription = 8,
        Offline = 50
        // PS takes care of billing in the payment day. We still don't know the method
    }

    public enum BillingAction
    {
        Unknown = 0,
        Purchase = 1,
        RenewPayment = 2,
        RenewCancledSubscription = 3,
        CancelSubscriptionOrder = 4,
        SubscriptionDateChanged = 5
    }

    public enum PlayContextType
    {
        Playback,
        Trailer,
        CatchUp,
        StartOver,
        Download
    }
}
