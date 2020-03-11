using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESIndexUpdateHandler.Updaters
{
    public interface IUpdateable
    {
        List<int> IDs { get; set; }
        ApiObjects.eAction Action { get; set; }

        bool Start();
    }
}
