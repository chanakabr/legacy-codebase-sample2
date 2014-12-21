using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class DomainResponseObject
    {
        public Domain domain { get; set; }
        
        public DomainResponseStatus domain_response_status { get; set; }
    }
}
