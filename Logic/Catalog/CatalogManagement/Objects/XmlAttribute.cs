using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    // TODO SHIR - FINISH 
    public class XmlAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public bool IsKeyProperty { get; set; }

        public bool PropertyValueRequired { get; set; }        

        public bool IgnoreWhenGeneratingTemplate { get; set; }        
    }
}
