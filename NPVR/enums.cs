using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{


    // this enum must correspond to the table lu_npvr_providers in TVinci DB.
    public enum NPVRProvider
    {
        None = 0,
        AlcatelLucent = 1,
        Kaltura = 2,
        Harmonic = 3
    }

    public enum ProtectStatus
    {
        Protected = 0,
        NotProtected = 1,
        RecordingDoesNotExist = 2,
        Error = 3
    }

    public enum RecordStatus
    {
        OK = 0,
        AlreadyRecorded = 1,
        Error = 2
    }
}
