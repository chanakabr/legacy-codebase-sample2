using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    public class LogWithSeverity
    {
        #region Members

        public string FullLogMessage { get; set; }
        public string LogSeverity { get; set; }

        #endregion

        #region CTOR

        public LogWithSeverity(string sFullLogMessage, string sSeverity)
        {
            this.FullLogMessage = sFullLogMessage;
            this.LogSeverity = sSeverity;
        }

        #endregion
    }
}
