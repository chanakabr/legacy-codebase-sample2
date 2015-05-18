using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Catalog
{
    [ServiceKnownType(typeof(IProgramsRequest))]
    public interface IProgramsRequest
    {
        [OperationContract]
        EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest);
    }
}
