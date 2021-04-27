using System;
using System.Collections.Generic;
using System.Text;

namespace ODBCWrapper
{
    public class DbPrimarySecondaryRouting
    {
        /// <summary>
        /// true - should go to primary, false - should go to secondary
        /// </summary>
        public Dictionary<string, bool> QueryNameToShouldRouteToPrimaryMapping;

        public DbPrimarySecondaryRouting()
        {
            this.QueryNameToShouldRouteToPrimaryMapping = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
