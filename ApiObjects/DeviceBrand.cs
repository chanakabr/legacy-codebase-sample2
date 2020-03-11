using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DeviceBrand
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public int DeviceFamilyId { get; set; }

        public DeviceBrand() { }

        public DeviceBrand(int id, string name, int deviceFamilyId)
        {
            this.Id = id;
            this.Name = name;
            this.DeviceFamilyId = deviceFamilyId;
        }
    }
}
