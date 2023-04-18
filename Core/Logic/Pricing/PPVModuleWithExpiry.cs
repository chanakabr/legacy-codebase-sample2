using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Pricing
{
    
    /*
     * This class is used as a container for a PPVModule with a start and end date to denote if it has already expired
     */ 
    [Serializable]
    [DataContract]
    public class PPVModuleWithExpiry
    {
        [DataMember]
        public PPVModule PPVModule{ get; set; }
        [DataMember] private DateTime _startDate;

        [DataMember] private DateTime _endDate;
        public bool IsValidForPurchase => IsValidDate(_startDate, _endDate);
        
        public PPVModuleWithExpiry()
        {
            PPVModule = new PPVModule();
        }

        public PPVModuleWithExpiry(PPVModule oPPVModule, DateTime dtStartDate, DateTime dtEndDate)
        {
            PPVModule = oPPVModule;
            _startDate = dtStartDate;
            _endDate = dtEndDate;
        }

        private bool IsValidDate(DateTime dtStartDate, DateTime dtEndDate)
        {
            return dtStartDate <= DateTime.UtcNow && dtEndDate >= DateTime.UtcNow;
        }
    }
}
