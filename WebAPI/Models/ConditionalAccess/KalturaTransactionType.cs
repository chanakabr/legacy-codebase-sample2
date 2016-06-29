using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaTransactionType
    {  
        PPV,
        SUBSCRIPTION,
        COLLECTION
    }
}