using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class NetworkResponseObject
    {
        public bool IsSuccess { get; set; }

        public NetworkResponseStatus Reason { get; set; }
    }
}
