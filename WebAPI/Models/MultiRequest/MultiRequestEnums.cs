using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.MultiRequest
{
    public enum KalturaSkipOptions
    {
        No = 0,
        // Skip current request if previous Request has an error
        Previous = 1,
        // Skip current request if any of previous Requests had an error
        Any = 2
    }

    public enum KalturaSkipOperators
    {
        Equal,
        UnEqual,
        LessThan,
        GreaterThan
    }

    public enum KalturaAggregationType
    {
        Count,
        Sum,
        Avg
    }
}