using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumProject;

namespace GracenoteFeeder
{
    public class FieldTypeEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<string> XmlReffName = new List<string>();
        public enums.FieldTypes FieldType { get; set; }
        public List<string> Value = new List<string>();
    }
}
   