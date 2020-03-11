using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;

namespace TVPPro.Configuration.Online
{
	public class TVPProOnlineConfiguration : ConfigurationManager<OnlineConfiguration>
	{
		public static TVPProOnlineConfiguration Instance = new TVPProOnlineConfiguration();
		private Dictionary<string, string> m_AlternativeServers = new Dictionary<string, string>();

		ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

		private TVPProOnlineConfiguration()
		{
			base.Behaivor = ConfigurationManager<OnlineConfiguration>.eBehaivor.AllowWrite;
			base.DataModified = dataModified;
			base.SyncFromFile(System.Configuration.ConfigurationManager.AppSettings["TVPPro.Configuration.Online"], true);
		}
        
        // No Need
        //public string GetActiveAlternativeTVMUrl()
        //{
        //    m_locker.EnterReadLock();

        //    try
        //    {
        //        string result = string.Empty;
        //        if (m_AlternativeServers.TryGetValue(Data.TVM.AlternativeServer.ActiveID, out result))
        //        {
        //            return LinkHelper.ParseURL(result);
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //    finally
        //    {
        //        m_locker.ExitReadLock();
        //    }
        //}

		private void dataModified(OnlineConfiguration data)
		{
			m_locker.EnterWriteLock();
			try
			{
				m_AlternativeServers.Clear();

				foreach (Server server in data.TVM.AlternativeServer)
				{
					m_AlternativeServers.Add(server.ID, server.URL);
				}
			}
			finally
			{
				m_locker.ExitWriteLock();
			}
		}
	}
}
