using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.Configuration.Technical;
using TVPPro.Configuration.PlatformServices;
using System.Configuration;
using System.IO;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Site;

/// <summary>
/// Summary description for ConfigurationManager
/// </summary>
/// 

namespace TVPApi
{
    public class ConfigManager
    {

        private TechnicalConfiguration m_technicalConfig;
        private PlatformServicesConfiguration m_platformConfig;
        private MediaConfiguration m_mediaConfig;
        private SiteConfiguration m_siteConfig;
        private static Dictionary<string, ConfigManager> m_instances = null;

        //Init technichal and platform config (sync from file)
        private ConfigManager(int groupID, string platform)
        {
            string parentDirectoryStr = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[groupID.ToString()]);
            string mediaConfigFile = string.Concat(parentDirectoryStr, "MediaConfiguration.config");
            string directoryStr = string.Concat(parentDirectoryStr, platform);
            m_mediaConfig = TVPPro.Configuration.Media.MediaConfiguration.GetInstance(mediaConfigFile);
            if (!string.IsNullOrEmpty(directoryStr))
            {
                string[] fileNames = Directory.GetFiles(directoryStr);
                foreach (string file in fileNames)
                {
                    if (file.Contains("Technical"))
                    {
                        m_technicalConfig = TVPPro.Configuration.Technical.TechnicalConfiguration.GetInstance(file);
                    }
                    else if (file.Contains("Platform"))
                    {
                        m_platformConfig = TVPPro.Configuration.PlatformServices.PlatformServicesConfiguration.GetInstance(file);
                    }
                    else if (file.Contains("Site"))
                    {
                        m_siteConfig = TVPPro.Configuration.Site.SiteConfiguration.GetInstance(file);
                    }
                    //else if (file.Contains("Media"))
                    //{
                    //    m_mediaConfig = TVPPro.Configuration.Media.MediaConfiguration.GetInstance(file);
                    //}
                }
            }
        }

     



        //Get specific config manager by group ID. If this is a new group ID - create new instances of platform configuration and Technichal c
        //configuration (sync from different files)
        public static ConfigManager GetInstance(int groupID, string platform)
        {
            if (m_instances == null)
            {
                m_instances = new Dictionary<string, ConfigManager>();
            }
            string keyStr = string.Concat(groupID.ToString(), platform);
            if (!m_instances.ContainsKey(keyStr))
            {
                m_instances.Add(keyStr, new ConfigManager(groupID, platform));
            }

            return m_instances[keyStr];

        }

        public TechnicalConfiguration TechnichalConfiguration
        {
            get
            {
                return m_technicalConfig;
            }
        }

        public PlatformServicesConfiguration PlatformConfiguration
        {
            get
            {
                return m_platformConfig;
            }
        }

        public MediaConfiguration MediaConfiguration
        {
            get
            {
                return m_mediaConfig;
            }
        }

        public SiteConfiguration SiteConfiguration
        {
            get
            {
                return m_siteConfig;
            }
        }

    }
}
