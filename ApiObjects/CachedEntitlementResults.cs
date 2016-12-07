using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class CachedEntitlementResults
    {
        public int ViewLifeCycle { get; set; }
        public int FullLifeCycle { get; set; }
        public DateTime CreditDownloadedDate { get; set; }
        public bool IsFree { get; set; }
        public bool IsOfflinePlayback { get; set; }

        public CachedEntitlementResults() { }

        public CachedEntitlementResults(int viewLifeCycle, int fullLifeCycle, DateTime lastUseDate, bool isFree, bool isOfflinePlayback)
        {
            ViewLifeCycle = viewLifeCycle;
            FullLifeCycle = fullLifeCycle;
            CreditDownloadedDate = lastUseDate;
            IsFree = isFree;
            IsOfflinePlayback = isOfflinePlayback;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("ViewLifeCycle: {0}, ", ViewLifeCycle));
            sb.AppendFormat("FullLifeCycle: {0}, ", FullLifeCycle);
            sb.AppendFormat("LastUseDate: {0}, ", CreditDownloadedDate != null ? CreditDownloadedDate.ToString() : "");
            sb.AppendFormat("IsFree: {0}, ", IsFree);
            sb.AppendFormat("IsOfflinePlayback: {0}", IsOfflinePlayback);            

            return sb.ToString();
        }
    }
}