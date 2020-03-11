using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Core.Catalog.Response;

namespace Core.Catalog.Request
{
    [ServiceKnownType(typeof(IProgramsRequest))]
    public interface IProgramsRequest
    {
        [OperationContract]
        EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest);
    }
}
