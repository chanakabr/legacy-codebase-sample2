using System.Reflection;
using log4net;

namespace ps_notifiers
{
	internal static class Logger
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Namespace);

		internal static void Log(string method, string message, string logFileName = "")
		{
			if (!string.IsNullOrEmpty(message) && message.ToLower().Contains("error>"))
			{
				_log.Error(method + "> " + message);
			}
			else
			{
				_log.Info(method + "> " + message);
			}
		}
	}
}
