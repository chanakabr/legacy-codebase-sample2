using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class ExcelTemplateAttribute : Attribute
    {

        public bool PropertyValueRequired { get; set; }        

        public bool IgnoreWhenGeneratingTemplate { get; set; }

        public bool IsKeyProperty { get; set; }

        public string SystemName { get; set; }
    }
}
