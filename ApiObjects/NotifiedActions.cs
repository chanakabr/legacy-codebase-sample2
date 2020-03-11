using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    /// <summary>
    /// List of actions that PS need to be notified on.
    /// Actions can be added easily.
    /// </summary>
    public enum NotifiedAction
    {
        CancelDomainServiceNow = 1,
        CancelDomainSubscriptionRenewal = 2,
        ChargedMediaFile = 3,
        ChargedSubscription = 4,
        ChargedCollection = 5,
        ChargedSubscriptionRenewal = 6,
        ChangedSubscription = 7,
        FailedSubscriptionRenewal = 8
    }
}
