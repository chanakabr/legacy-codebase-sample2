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
            string response = "";
            NotifyChange(sSubscriptionID, ref response);
        }

        override public void NotifyChange(string sSubscriptionID, bool update = true)
        {
            string response = "";
            NotifyChange(sSubscriptionID, ref response);
        }

        override public void NotifyChange(string sSubscriptionID, ref string response, bool update = true)
        {
            tikle_ws.Service t = new Notifiers.tikle_ws.Service();
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            t.Url = sTikleWSURL;
            tikle_ws.Response resp = t.NotifySubscription(sSubscriptionID, m_nGroupID);

            response = resp.ResultDetail;
            Logger.Logger.Log("Notify", sSubscriptionID + " : "  + response, "subscriptions_notifier");   
        }

    }
}
