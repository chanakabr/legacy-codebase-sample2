using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public enum NetworkResponseStatus
    {
        OK = 0,
        QuantityLimitation = 1,
        FrequencyLimitation = 2,
        NetworkExists = 3,
        NetworkDoesNotExist = 4,
        InvalidInput = 5,
        Error = 6
    }
}
