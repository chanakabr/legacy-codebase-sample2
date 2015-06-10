using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Notifiers
{
    public class TikleUsersNotifier : BaseUsersNotifier
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TikleUsersNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSiteGUID)
        {
            tikle_ws.Service t = new Notifiers.tikle_ws.Service();
            string sTikleWSURL = Utils.GetWSURL("tikle_ws");
            t.Url = sTikleWSURL;
            tikle_ws.Response resp = t.NotifyCustomer(sSiteGUID);
            log.Debug("Notify sSiteGUID: " + sSiteGUID + " : " + resp.ResultDetail);
        }
    }
}
