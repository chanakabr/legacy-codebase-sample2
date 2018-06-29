using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelManager
{
    public class ExcelAssetDefinitionColumn : ExcelDefinitionColumn
    {
        public Type Propertytype { get; set; }
        public bool IsKey { get; set; }
        public int LanguageId { get; set; }
    }
}
