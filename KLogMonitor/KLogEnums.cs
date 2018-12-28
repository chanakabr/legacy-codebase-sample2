using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KLogMonitor
{
    public class KLogEnums
    {
        public enum eDBQueryType
        {
            UNKNOWN,
            SELECT,
            UPDATE,
            INSERT,
            DELETE,
            EXECUTE,
            COMMAND
        }

        public enum AppType
        {
            WS,
            WCF,
            WindowsService
        }
    }
}
