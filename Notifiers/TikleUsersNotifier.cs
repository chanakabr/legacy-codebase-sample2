using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public class TikleUsersNotifier: BaseUsersNotifier
    {
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
            Logger.Logger.Log("Notify", sSiteGUID + " : " + resp.ResultDetail, "users_notifier");
            
        }
    }
}
