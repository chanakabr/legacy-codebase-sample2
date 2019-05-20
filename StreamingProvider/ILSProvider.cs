using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamingProvider
{
    public interface ILSProvider
    {
        string GenerateEPGLink(Dictionary<string, object> dParams);
        string GenerateVODLink(string vodUrl);
    }
}
