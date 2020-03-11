using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers
{
    /// <summary>
    /// Enumeration of all supported HMAC algorithms
    /// </summary>
    public enum Algorithm
    {
        HMACMD5 = 0,
        HMACSHA1,
        HMACSHA256
    }
}
