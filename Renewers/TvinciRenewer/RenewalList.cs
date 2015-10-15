using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace TvinciRenewer
{
    public class RenewalList
    {
        private Dictionary<long, int> purchaseIDToIndexDict;
        private List<long> mppPurchaseIDs;
        private List<DateTime> endDates;

        public RenewalList(DataTable dt)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                InitializeCollections(length);
                for (int i = 0; i < length; i++)
                {
                    long l = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["ID"]);
                    DateTime ed = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["END_DATE"]);
                    mppPurchaseIDs.Add(l);
                    endDates.Add(ed);
                    purchaseIDToIndexDict.Add(l, i);
                }
            }
            else
            {
                InitializeCollections(0);
            }
        }

        private void InitializeCollections(int lengthOfLists)
        {
            mppPurchaseIDs = new List<long>(lengthOfLists);
            endDates = new List<DateTime>(lengthOfLists);
            purchaseIDToIndexDict = new Dictionary<long, int>();
        }

        public List<long> GetMPPPurchasesList()
        {
            return mppPurchaseIDs;
        }

        public List<DateTime> GetEndDates()
        {
            return endDates;
        }

        public DateTime GetEndDateByPurchaseID(long mppPurchaseID)
        {
            if (purchaseIDToIndexDict.ContainsKey(mppPurchaseID))
                return endDates[purchaseIDToIndexDict[mppPurchaseID]];
            return new DateTime(2000, 1, 1); // same as ODBCWrapper.Utils.GetDateSafeVal value when it fails to parse
        }
    }
}
