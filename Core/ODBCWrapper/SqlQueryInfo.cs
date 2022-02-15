using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phx.Lib.Log;

namespace ODBCWrapper
{
    public class SqlQueryInfo
    {
        public KLogEnums.eDBQueryType QueryType { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
    }
}
