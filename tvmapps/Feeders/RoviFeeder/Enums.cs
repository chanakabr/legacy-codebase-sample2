using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoviFeeder
{
    public enum FeederImplEnum
    {
        MOVIE = 1,
        CMT = 2,
        EPISODE = 3,
        SERIES = 4
    }

    public enum IngestNotificationStatus
    {
        SUCCESS = 1,
        ERROR
    }
}
