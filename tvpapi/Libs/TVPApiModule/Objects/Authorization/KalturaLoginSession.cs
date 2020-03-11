using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Authorization
{
    public class KalturaLoginSession
    {
        /// <summary>
        /// Access token in a KS format
        /// </summary>
        public string KS { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration
        /// </summary>
        public long Expiry { get; set; }

        public long RefreshTokenExpiry { get; set; }
    }
}
