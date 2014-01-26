using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PreviewModule
    {
        public long id { get; set; }

        public string name { get; set; }

        public int fullLifeCycle { get; set; }

        public int nonRenewPeriod { get; set; }
    }
}
