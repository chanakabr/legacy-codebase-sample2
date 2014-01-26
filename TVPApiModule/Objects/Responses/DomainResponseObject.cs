using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DomainResponseObject
    {
        public Domain domain { get; set; }
        
        public DomainResponseStatus domainResponseStatus { get; set; }
    }
}
