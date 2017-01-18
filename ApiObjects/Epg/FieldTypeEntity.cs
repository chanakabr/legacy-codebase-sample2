using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    public class FieldTypeEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<string> XmlReffName = new List<string>();
        public FieldTypes FieldType { get; set; }
        public List<string> Value = new List<string>();
        // add this in storm version
        public string Alias { get; set; }
        public string RegexExpression { get; set; }
    }
}
