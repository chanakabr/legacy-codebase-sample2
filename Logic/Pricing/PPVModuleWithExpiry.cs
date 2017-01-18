using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    
    /*
     * This class is used as a container for a PPVModule with a start and end date to denote if it has already expired
     */ 
    [Serializable]
    public class PPVModuleWithExpiry
    {
        public PPVModule PPVModule{ get; set; }
        public bool IsValidForPurchase { get; set; }

        public PPVModuleWithExpiry()
        {
            PPVModule = new PPVModule();
            IsValidForPurchase = false;
        }

        public PPVModuleWithExpiry(PPVModule oPPVModule, DateTime dtStartDate, DateTime dtEndDate)
        {
            PPVModule = oPPVModule;
            IsValidForPurchase = IsValidDate(dtStartDate, dtEndDate);
        }

        private bool IsValidDate(DateTime dtStartDate, DateTime dtEndDate)
        {
            return dtStartDate <= DateTime.UtcNow && dtEndDate >= DateTime.UtcNow;
        }
    }
}
