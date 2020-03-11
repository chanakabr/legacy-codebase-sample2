using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader
{    
    public interface ILoaderAdapter
    {
        bool IsPersist();
        object Execute();
        object Execute(eExecuteBehaivor behaivor);        
        [Obsolete()]
        object LastExecuteResult { get; }
    }
}
