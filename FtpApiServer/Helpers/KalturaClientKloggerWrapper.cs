using Kaltura;
using KLogMonitor;
using System.Reflection;

namespace FtpApiServer.Helpers
{
    internal class KalturaClientKloggerWrapper : ILogger
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public void Log(string msg)
        {
            _Logger.Debug(msg);
        }
    }
}