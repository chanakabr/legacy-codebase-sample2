using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Pricing
{
    [Serializable]
    public class ServiceObject
    {
        public long ID;
        public string Name;

        public ServiceObject()
        {
        }

        public ServiceObject(long id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
