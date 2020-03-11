using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODBCWrapper
{
    public class SqlQueryInfo
    {
        public KLogMonitor.KLogEnums.eDBQueryType QueryType { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
    }
}
