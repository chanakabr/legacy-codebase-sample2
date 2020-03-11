using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace Tvinci.Data.Loaders
{
    public enum eProviderResult
    {
        Success,
        TimeOut,
        Fail,
        SafeMode
    }

    [Serializable]
    public abstract class Provider
    {
        public abstract eProviderResult TryExecuteGetMediasByIDs(MediasProtocolRequest request, out MediaResponse response);
        public abstract eProviderResult TryExecuteGetBaseResponse(BaseRequest request, out BaseResponse response);

        public abstract eProviderResult TryExecuteGetProgramsByIDs(EpgProgramDetailsRequest request, out EpgProgramResponse oProgramResponse);
        
    }   
}
