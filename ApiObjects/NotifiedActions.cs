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
        CancelDomainSubscriptionRenewal = 2
    }
}
