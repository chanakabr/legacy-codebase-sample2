using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public interface INPVRProvider
    {
        bool CreateAccount(NPVRParamsObj args);

        bool DeleteAccount(NPVRParamsObj args);


    }
}
