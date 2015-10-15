using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ExternalChannelBase
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public ExternalChannelBase()
        {
        }

        public ExternalChannelBase(ExternalChannelBase externalChannelBase)
        {
            this.ID = externalChannelBase.ID;
            this.Name = externalChannelBase.Name;
        }

        public ExternalChannelBase(int id, string name, bool isDefault)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
