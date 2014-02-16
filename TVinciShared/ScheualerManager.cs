using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TVinciShared
{
    public class ScheualerManager
    {
        public ScheualerManager()
        {

        }
        public void Start()
        {
            Thread.Sleep(10000);
            Logger.Logger.Log("message", "start end", "Scheduler");
        }
    }
}
