using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public class EutelsatUsersNotifier: BaseUsersNotifier
    {
        public EutelsatUsersNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sSiteGUID)
        {
            //tikle_ws.Service t = new Notifiers.tikle_ws.Service();
            //string sTikleWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("tikle_ws"); //TCM not relevant anymore 
            //t.Url = sTikleWSURL;
            //tikle_ws.Response resp = t.NotifyCustomer(sSiteGUID);

        }
    }
}
