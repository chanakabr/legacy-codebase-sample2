using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;

namespace TVPPro.Configuration.Admin
{
    public class AdminConfiguration : ConfigurationManager<AdminData>
    {
        

        #region Public Properties
        private static AdminConfiguration m_Instance = new AdminConfiguration();
        public static AdminConfiguration Instance
        {
            get
            {
                return m_Instance;
            }
        }

        public string BrowseToSiteBaseURL
        {
            get
            {
                string url = Data.Sites.BrowseToSiteBaseURL;

                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }

                return url.EndsWith("/") ? url : string.Concat(url, "/");   
            }
        }
        #endregion

        #region Constructor
        private AdminConfiguration()
        {
            TCMClient.Settings.Instance.Init();
            base.SyncFromFile(ConfigurationManager.AppSettings["Configuration.Admin"], true);
        } 
        #endregion
    }
}
