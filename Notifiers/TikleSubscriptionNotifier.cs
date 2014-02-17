using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public class TikleSubscriptionNotifier: BaseSubscriptionNotifier
    {
        public TikleSubscriptionNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSubscriptionID)
        {
            NotifyChange(sSubscriptionID, true);
        }

        override public void NotifyChange(string sSubscriptionID, bool update = true)
        {
            tikle_ws.Service t = new Notifiers.tikle_ws.Service();
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            t.Url = sTikleWSURL;
            tikle_ws.Response resp = t.NotifySubscription(sSubscriptionID, m_nGroupID);
            Logger.Logger.Log("Notify", sSubscriptionID + " : "  +resp.ResultDetail, "subscriptions_notifier");
            
        }

    }
}
