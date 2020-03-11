using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using KLogMonitor;

namespace TVinciShared
{
    public class ScheualerManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ScheualerManager()
        {

        }
        public void Start()
        {
            Thread.Sleep(10000);
            log.Debug("message - start end");
        }
    }
}
