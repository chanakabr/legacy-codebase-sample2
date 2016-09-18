using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODBCWrapper
{
    public class DatabaseStoredProceduresMapping
    {

        public Dictionary<string, string> routing { get; set; }

        public DatabaseStoredProceduresMapping()
        {
            routing = new Dictionary<string, string>();
        }
    }
}
