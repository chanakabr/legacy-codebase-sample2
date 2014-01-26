using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Scope
    {
        public string name { get; set; }

        public string login_url { get; set; }

        public string logout_url { get; set; }
    }
}
