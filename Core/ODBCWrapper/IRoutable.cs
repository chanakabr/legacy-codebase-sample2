using System;
using System.Collections.Generic;
using System.Text;

namespace ODBCWrapper
{
    public interface IRoutable
    {
        bool ShouldRouteToPrimary();
        string GetName();
    }
}
