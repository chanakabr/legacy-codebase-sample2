using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ODBCWrapper;

namespace Notifiers
{
    public class TikleMediaNotifier: BaseMediaNotifier
    {
        public TikleMediaNotifier(Int32 nGroupID)
            : base(nGroupID)
        {
        }


        override public void NotifyChange(string sMediaID)
        {
            if (IsNotifyProduct(sMediaID))
            {
                tikle_ws.Service t = new Notifiers.tikle_ws.Service();
                string sTikleWSURL = Utils.GetWSURL("tikle_ws");
                t.Url = sTikleWSURL;
                tikle_ws.Response resp = t.NotifyProduct(sMediaID, m_nGroupID);
                Logger.Logger.Log("Notify", sMediaID + " : " + resp.ResultDetail, "media_notifier");
            }
            else
            {
                Logger.Logger.Log("Notify", sMediaID + " : " + "No need to notify - media is off or expired", "media_notifier");
            }
        }

        private bool IsNotifyProduct(string sMediaID)
        {
            bool retVal = false;
            int nMediaID;
            if (int.TryParse(sMediaID, out nMediaID))
            {
                DataSetSelectQuery selectQuery = new DataSetSelectQuery();
                selectQuery += " select * from media where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nMediaID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", 1);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", 1);
                selectQuery += " and (end_date > getdate() or end_date IS NULL)";
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = true;
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            return retVal;
        }
    }
}
