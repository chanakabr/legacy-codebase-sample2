using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.SearchObjects
{
    public class BooleanLeafFieldDefinitions
    {
        public string Field { get; set; }
        public eFieldType FieldType { get; set; }
        public Type ValueType { get; set; }
    }
}
