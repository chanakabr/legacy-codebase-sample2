using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelManager
{
    
    public class ExcelDefinitionColumn
    {        
        public string SystemName { get; set; }
        public ExcelColumnType Type { get; set; }        
        public bool IsValueRequired { get; set; }
    }
}
