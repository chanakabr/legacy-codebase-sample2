using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelManager
{
    public class ExcelTopicDefinitionColumn : ExcelDefinitionColumn
    {
        public MetaType metaType { get; set; }
        public int LanguageId { get; set; }
    }
}