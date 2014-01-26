using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Scope
    {
        public string name { get; set; }

        public string loginUrl { get; set; }

        public string logoutUrl { get; set; }
    }
}
