using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    /// <summary>
    /// Object that defines performaing a request vis adapters controller
    /// </summary>
    public class HouseholdBillingRequest
    {
        public OSSAdapter OSSAdapter { get; set; }
        public int GroupId { get; set; }
        public long HouseholdId { get; set; }
        public string UserIP { get; set; }        
    }
}
