using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Notifiers
{
    public class TikleSubscriptionNotifier : BaseSubscriptionNotifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TikleSubscriptionNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSubscriptionID)
        {
            string response = "";
            NotifyChange(sSubscriptionID, ref response);
        }

        override public void NotifyChange(string sSubscriptionID, int create0update1assign2)
        {
            string response = "";
            NotifyChange(sSubscriptionID, ref response, create0update1assign2);
        }

        //override public void NotifyChange(string sSubscriptionID, ref string response, bool update = true)
        override public void NotifyChange(string sSubscriptionID, ref string response, int create0update1assign2 = 1)
        {
            tikle_ws.Service t = new Notifiers.tikle_ws.Service();
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            t.Url = sTikleWSURL;
            tikle_ws.Response resp = t.NotifySubscription(sSubscriptionID, m_nGroupID);

            response = resp.ResultDetail;
            log.Debug("Notify sSubscriptionID + " + sSubscriptionID + " : " + response);
        }

    }
}
