using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.Loaders
{
    public class CacheManager
    {
        private static Cache m_oCache = new Cache();

        private CacheManager()
        {
        }

        public static Cache Cache { get { return m_oCache; } }
    }
}
