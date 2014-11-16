using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class enums
    {

        // this enum must correspond to the table lu_npvr_providers in TVinci DB.
        public enum NPVRProvider : int
        {
            None = 0,
            AlcatelLucent = 1
        }
    }
}
