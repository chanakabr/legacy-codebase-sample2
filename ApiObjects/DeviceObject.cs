using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class DeviceObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int FamilyID { get; set; }
        public string FamilyName { get; set; }
    }
}
