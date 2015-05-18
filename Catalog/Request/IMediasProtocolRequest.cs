using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Catalog
{
    [ServiceKnownType(typeof(IMediasProtocolRequest))]
    public interface IMediasProtocolRequest
    {
        [OperationContract]
        MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest);
    }
}
