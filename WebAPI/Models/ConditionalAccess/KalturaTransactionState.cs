using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaTransactionState
    {
        failed = 0,
        completed = 1,
        pending = 2,
        canceled = 3,
    }
}