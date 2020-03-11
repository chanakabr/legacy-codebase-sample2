using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    class KangurooConditionalAccess : TvinciConditionalAccess
    {

        public KangurooConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public KangurooConditionalAccess(Int32 nGroupID, string connKey)
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
                #region Old Code
                //// Check if user has a least one recurring subscription which means the user is subscribed
                //foreach (var subscription in subscriptionsItems)
                //{


                //    // User is subscribed only if (NOT iff!) he has a renewable subscription
                //    if (subscription.m_bRecurringStatus == true)
                //    {
                //        BillingTransactionsResponse oBTR = base.GetUserBillingHistory(sSiteGUID, 0, 2);

                //        // Check if subscription is not a gift (gift doesn't make a user subscribed) - if so, user is subscribed
                //        foreach (var transaction in oBTR.m_Transactions)
                //        {
                //            if (transaction.m_ePaymentMethod != PaymentMethod.Gift)
                //            {
                //                oUserCAStatus = UserCAStatus.CurrentSub;
                //                return true;
                //            }
                //        }

                //        break;
                //    }

                //}

                //// If we didn't return in the foreach loop, means all subscriptions are non-recurring and therefore the user is only registered
                //oUserCAStatus = UserCAStatus.NeverPurchased;
                //return true;
                #endregion
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
