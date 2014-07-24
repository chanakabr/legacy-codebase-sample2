using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers
{
    public interface ICDNTokenizer
    {
        string GenerateToken(Dictionary<string, string> dParams);
    }
}
