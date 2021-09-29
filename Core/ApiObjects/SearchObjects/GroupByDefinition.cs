using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.SearchObjects
{
    public class GroupByDefinition
    {
        public string Key { get; set; }
        public eFieldType Type { get; set; }
        public string Value { get; set; }
    }

}
