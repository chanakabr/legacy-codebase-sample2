using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    class ElisaConditionalAccess : TvinciConditionalAccess
    {

        public ElisaConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public ElisaConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        protected override bool GetUserCASubStatus(string sSiteGUID, ref UserCAStatus oUserCAStatus)
        {
            PermittedSubscriptionContainer[] subscriptionsItems = GetUserPermittedSubscriptions(sSiteGUID);



            if (subscriptionsItems != null && subscriptionsItems.Length > 0)
            {
                oUserCAStatus = UserCAStatus.CurrentSub;
                return true;
            }
            else
            {
                // No subscriptions for the user and therefore the user is only registered
                oUserCAStatus = UserCAStatus.NeverPurchased;
                return true;
            }
        }

        public override UserCAStatus GetUserCAStatus(string sSiteGUID)
        {
            return base.GetUserCAStatus(sSiteGUID);
        }
    }
}
