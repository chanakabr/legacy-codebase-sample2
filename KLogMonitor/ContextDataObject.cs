using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace KLogMonitor
{
    // Shared data object has to implement ILogicalThreadAffinative
    public class ContextDataObject : ILogicalThreadAffinative
    {
        public Dictionary<string, string> data { get; set; }
    }
}
