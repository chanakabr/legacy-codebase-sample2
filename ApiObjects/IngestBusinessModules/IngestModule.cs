using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
   
    public abstract class IngestModule
    {
        public string Code { get; set; }

        public eIngestAction Action { get; set; }
    }
}
